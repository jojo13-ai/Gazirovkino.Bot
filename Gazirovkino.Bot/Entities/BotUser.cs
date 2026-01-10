using System;

namespace Gazirovkino.Bot.Entities;

public class BotUser
{
    public Guid Id { get; init; }
    public long TelegramUserId { get; init; }
    public string? Username { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTime CreatedAt { get; init; }
}
