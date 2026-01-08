using System;

namespace Gazirovkino.Bot.Entities;

public class Survey
{
    public Guid Id { get; init; }
    public Guid UserId{ get; init; }
    public DateTime DateCreated { get; init; }
    public SurveyStatus Status { get; set; }
    public DateTime? DateFinished { get; set; }
    public DateTime? DateTerminated { get; set; }
}