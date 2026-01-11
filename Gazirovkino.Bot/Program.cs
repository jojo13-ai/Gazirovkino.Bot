using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Gazirovkino.Bot.Data;
using Gazirovkino.Bot.Entities;
using Gazirovkino.Bot.Entities.Gazirovka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

const string token = ""; //TODO разобраться как не коммитить ключ"

using var cts = new CancellationTokenSource();

var serviceProvider = BuildServiceProvider();

var dbOptions = new DbContextOptionsBuilder<GazirovkinoDbContext>().Options;
await using var dbContext = new GazirovkinoDbContext(dbOptions);
await dbContext.Database.MigrateAsync(cts.Token);

var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var botUser = await bot.GetMe(); // регистрация нашего бота в Телеграм

var tasteToRu = new Dictionary<GazirovkaTaste, string>
{
    { GazirovkaTaste.CherryTaste, "Вишня" },
    { GazirovkaTaste.OrangeTaste, "Апельсин" },
    { GazirovkaTaste.ColaTaste, "Кола" }
};
var ruToTaste = tasteToRu.ToDictionary(kv => kv.Value, kv => kv.Key);

var colorToRu = new Dictionary<GazirovkaColor, string>
{
    { GazirovkaColor.Dark, "Темный" },
    { GazirovkaColor.Orange, "Оранжевый" },
    { GazirovkaColor.Clear, "Прозрачный" }
};
var ruToColor = colorToRu.ToDictionary(kv => kv.Value, kv => kv.Key);

var additionsToRu = new Dictionary<GazirovkaAdditions, string>
{
    { GazirovkaAdditions.NoAdditions, "Без добавок" },
    { GazirovkaAdditions.Jelly, "Желе" }
};
var ruToAdditions = additionsToRu.ToDictionary(kv => kv.Value, kv => kv.Key);

bot.OnMessage += OnMessage;

Console.WriteLine($"@{botUser.Username} is running... Press Enter to terminate");
Console.ReadLine(); // Блокирует выполнение программы, ожидая ввода.
cts.Cancel(); // Останавливает работу, посылая сигнал для отмены.

await serviceProvider.DisposeAsync();

return;

async Task OnMessage(Message message, UpdateType type)
{
    if (message.Text is null)
        return;

    if (message.From is null)
        return;

    await using var scope = serviceProvider.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<GazirovkinoDbContext>();

    var user = await GetOrCreateUserAsync(db, message, cts.Token);

    Console.WriteLine($"Received {type} '{message.Text}' in {message.Chat}");

    if (message.Text.StartsWith("/start"))
    {
        //Приветствие и вывод кнопок 
        var welcomeMessage = GetWelcomeMessage();
        var replyKeyboard = GetMainKeyboard(); // Генерация кнопок
        await bot.SendMessage(chatId: message.Chat.Id, text: welcomeMessage, replyMarkup: replyKeyboard);

        return;
    }

    if (message.Text == "Поиск газировки")
    {
        var currentSurvey = await GetOrCreateCurrentSurveyAsync(db, user, cts.Token);

        var tasteKeyboard = GetTasteKeyboard();

        await bot.SendMessage(chatId: message.Chat.Id, text: "Выберите вкус:", replyMarkup: tasteKeyboard);
        return;
    }

    if (ruToTaste.TryGetValue(message.Text, out var taste))
    {
        var currentSurvey = await GetOrCreateCurrentSurveyAsync(db, user, cts.Token);

        currentSurvey.Taste = taste;
        await db.SaveChangesAsync(cts.Token);

        await bot.SendMessage(
            chatId: message.Chat.Id,
            text: $"Вкус сохранен: {tasteToRu[taste]}. Выберите цвет:",
            replyMarkup: GetColorKeyboard());
        return;
    }

    if (ruToColor.TryGetValue(message.Text, out var color))
    {
        var currentSurvey = await GetOrCreateCurrentSurveyAsync(db, user, cts.Token);

        currentSurvey.Color = color;
        await db.SaveChangesAsync(cts.Token);

        await bot.SendMessage(
            chatId: message.Chat.Id,
            text: $"Цвет сохранен: {colorToRu[color]}. Выберите добавки:",
            replyMarkup: GetAdditionsKeyboard());
        return;
    }

    if (ruToAdditions.TryGetValue(message.Text, out var additions))
    {
        var currentSurvey = await GetOrCreateCurrentSurveyAsync(db, user, cts.Token);

        currentSurvey.Additions = additions;
        await db.SaveChangesAsync(cts.Token);

        var gazirovka = await CalculateGazirovka(db, currentSurvey, cts.Token);
        var resultMessage = $"Ваша газировка: {gazirovka}.";
        
        var storagePath = @"C:\Users\user\RiderProjects\Gazirovkino.Bot\Gazirovkino.Bot\Storage";
        string? gazirovkaFilePath = null;
        if (Directory.Exists(storagePath))
        {
            gazirovkaFilePath = Directory.EnumerateFiles(storagePath, $"{gazirovka}.*").FirstOrDefault()
                ?? Directory.EnumerateFiles(storagePath, gazirovka).FirstOrDefault();
        }

        if (gazirovkaFilePath is not null)
        {
            await using var stream = File.OpenRead(gazirovkaFilePath);
            var inputFile = InputFile.FromStream(stream, Path.GetFileName(gazirovkaFilePath));
            await bot.SendPhoto(
                chatId: message.Chat.Id,
                photo: inputFile,
                caption: resultMessage,
                replyMarkup: GetMainKeyboard());
        }
        else
        {
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: resultMessage,
                replyMarkup: GetMainKeyboard());
        }
        return;
    }

    await bot.SendMessage(chatId: message.Chat.Id, text: "Неопознанная команда, попробуйте еще...");
}

async Task<BotUser> GetOrCreateUserAsync(GazirovkinoDbContext db, Message message, CancellationToken cancellationToken)
{
    var from = message.From!;
    var existingUser = await db.Users.SingleOrDefaultAsync(x => x.TelegramUserId == from.Id, cancellationToken);
    if (existingUser is not null)
        return existingUser;

    var user = new BotUser
    {
        Id = Guid.NewGuid(),
        TelegramUserId = from.Id,
        Username = from.Username,
        FirstName = from.FirstName,
        LastName = from.LastName,
        CreatedAt = DateTime.UtcNow
    };

    db.Users.Add(user);
    await db.SaveChangesAsync(cancellationToken);

    return user;
}

async Task<Survey> GetOrCreateCurrentSurveyAsync(GazirovkinoDbContext db, BotUser user,
    CancellationToken cancellationToken)
{
    var existingSurvey = await db.Surveys
        .SingleOrDefaultAsync(x => x.UserId == user.Id && x.Status == SurveyStatus.StartSearch, cancellationToken);
    if (existingSurvey is not null)
        return existingSurvey;

    var survey = new Survey
    {
        Id = Guid.NewGuid(),
        UserId = user.Id,
        Taste = GazirovkaTaste.UnknownTaste,
        Additions = default,
        Color = default,
        Status = SurveyStatus.StartSearch,
        DateCreated = DateTime.UtcNow
    };

    db.Surveys.Add(survey);
    await db.SaveChangesAsync(cancellationToken);

    return survey;
}

string GetWelcomeMessage()
{
    return
        "Добро пожаловать! Этот бот позволяет вам найти подходящую газировку с нужным вкусом). Используйте доступные команды для взаимодействия с ботом.";
}

ReplyKeyboardMarkup GetMainKeyboard()
{
    var buttons = new KeyboardButton[] { "Поиск газировки", "Помощь" };
    var keyboard = new[] { buttons };

    var keyboardMarkup = new ReplyKeyboardMarkup(keyboard)
    {
        ResizeKeyboard = true // Уменьшен размер кнопок для удобства
    };

    return keyboardMarkup;
}

ReplyKeyboardMarkup GetTasteKeyboard()
{
    var tasteButtons = tasteToRu.Values
        .Select(text => new KeyboardButton(text))
        .ToArray();
    var keyboard = new[] { tasteButtons };

    var keyboardMarkup = new ReplyKeyboardMarkup(keyboard)
    {
        ResizeKeyboard = true
    };

    return keyboardMarkup;
}

ReplyKeyboardMarkup GetColorKeyboard()
{
    var colorButtons = colorToRu.Values
        .Select(text => new KeyboardButton(text))
        .ToArray();
    var keyboard = new[] { colorButtons };

    var keyboardMarkup = new ReplyKeyboardMarkup(keyboard)
    {
        ResizeKeyboard = true
    };

    return keyboardMarkup;
}

ReplyKeyboardMarkup GetAdditionsKeyboard()
{
    var additionsButtons = additionsToRu.Values
        .Select(text => new KeyboardButton(text))
        .ToArray();
    var keyboard = new[] { additionsButtons };

    var keyboardMarkup = new ReplyKeyboardMarkup(keyboard)
    {
        ResizeKeyboard = true
    };

    return keyboardMarkup;
}

async Task<string> CalculateGazirovka(GazirovkinoDbContext db, Survey survey, CancellationToken cancellationToken)
{
    if (survey.Taste == GazirovkaTaste.UnknownTaste)
        return "Ошибка: не выбран вкус газировки. Попробуйте снова с выбора вкуса.";

    if (!Enum.IsDefined(typeof(GazirovkaColor), survey.Color))
        return "Ошибка: не выбран цвет газировки. Попробуйте снова с выбора цвета.";

    if (!Enum.IsDefined(typeof(GazirovkaAdditions), survey.Additions))
        return "Ошибка: не выбраны добавки. Попробуйте снова с выбора добавок.";

    string gazirovka = string.Empty;

    if (survey.Additions == GazirovkaAdditions.Jelly)
    {
        gazirovka = "A4 Cola";
    }
    
    if (survey.Taste == GazirovkaTaste.CherryTaste)
    {
        gazirovka = "Dr.Pepper Cherry Crush";
    }
    
    if (survey.Taste == GazirovkaTaste.ColaTaste)
    {
        gazirovka = "Aziano Cola";
    }
    
    if (survey.Taste == GazirovkaTaste.OrangeTaste)
    {
        gazirovka = "Fancy";
    }

    if (string.IsNullOrEmpty(gazirovka))
    {
        return "Произошла ошибка, не удалось выбрать газировку";
    }
    
    survey.Result = gazirovka;
    survey.Status = SurveyStatus.FinishedSuccessfully;
    survey.DateFinished = DateTime.UtcNow;

    await db.SaveChangesAsync(cancellationToken);

    return gazirovka;
}

ServiceProvider BuildServiceProvider()
{
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false)
        .Build();
    var connectionString = configuration.GetConnectionString("GazirovkinoDb")
                           ?? throw new InvalidOperationException("Missing connection string: GazirovkinoDb");

    var services = new ServiceCollection();
    services.AddDbContext<GazirovkinoDbContext>(options =>
        options.UseNpgsql(connectionString));

    return services.BuildServiceProvider();
}
