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

async Task OnMessage(Message msg, UpdateType type)
{
    if (msg.Text is null) 
        return;

    Console.WriteLine($"Received {type} '{msg.Text}' in {msg.Chat}");

    // Ответ в этот же чат с текстом, отправленным пользователем
    await bot.SendMessage(msg.Chat, $"{msg.From} said: {msg.Text}");
}