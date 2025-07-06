using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

const string token = ""; //TODO разобраться как не коммитить ключ

using var cts = new CancellationTokenSource();
var bot = new TelegramBotClient(token, cancellationToken: cts.Token);

var botUser = await bot.GetMe(); // регистрация нашего бота в Телеграм

bot.OnMessage += OnMessage;

Console.WriteLine($"@{botUser.Username} is running... Press Enter to terminate");
Console.ReadLine();  // Блокирует выполнение программы, ожидая ввода.
cts.Cancel();        // Останавливает работу, посылая сигнал для отмены.
return;

async Task OnMessage(Message message, UpdateType type)
{
    if (message.Text is null) 
        return;

    Console.WriteLine($"Received {type} '{message.Text}' in {message.Chat}");

    if (message.Text.StartsWith("/start"))
    {
        var welcomeMessage = GetWelcomeMessage();
        await bot.SendMessage(chatId: message.Chat.Id, text: welcomeMessage);
        return;
    }

    // Добавить сюда другие условия в будущем...

    await bot.SendMessage(chatId: message.Chat.Id, text: "Неопознанная команда, попробуйте еще...");
}

string GetWelcomeMessage()
{
    return "Добро пожаловать! Этот бот позволяет вам ... (опишите функционал бота). Используйте доступные команды для взаимодействия с ботом.";
}