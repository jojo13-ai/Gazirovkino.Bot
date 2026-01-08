using System;
using System.Threading;
using System.Threading.Tasks;
using Gazirovkino.Bot.Data;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

const string token =""; //TODO разобраться как не коммитить ключ"

using var cts = new CancellationTokenSource();

var dbOptions = new DbContextOptionsBuilder<GazirovkinoDbContext>().Options;
await using var dbContext = new GazirovkinoDbContext(dbOptions);
await dbContext.Database.MigrateAsync(cts.Token);

var bot = new TelegramBotClient(token, cancellationToken: cts.Token);
var botUser = await bot.GetMe(); // регистрация нашего бота в Телеграм

bot.OnMessage += OnMessage;

Console.WriteLine($"@{botUser.Username} is running... Press Enter to terminate");
Console.ReadLine(); // Блокирует выполнение программы, ожидая ввода.
cts.Cancel(); // Останавливает работу, посылая сигнал для отмены.

return;

async Task OnMessage(Message message, UpdateType type)
{
    if (message.Text is null)
        return;

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
           
    }

    // Добавить сюда другие условия в будущем...

    await bot.SendMessage(chatId: message.Chat.Id, text: "Неопознанная команда, попробуйте еще...");
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
