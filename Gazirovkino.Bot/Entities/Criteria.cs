using System;

namespace Gazirovkino.Bot.Entities;

public class Criteria 
{
    public Guid Id { get; init; }
    public Guid SurveyId { get; init; }
    public DateTime DateCreated { get; init; }
    public DateTime? DateFinished { get; set; }
    public CriteriaType Type { get; init; }
    public CriteriaStatus Status { get; set; } 
    public string? Result { get; set; }
    public int Order { get; init; }
}