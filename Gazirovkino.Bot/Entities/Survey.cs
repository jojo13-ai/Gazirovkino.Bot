using System;
using Gazirovkino.Bot.Entities.Gazirovka;

namespace Gazirovkino.Bot.Entities;

public class Survey
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public GazirovkaTaste Taste { get; set; }
    public GazirovkaAdditions Additions { get; set; }
    public GazirovkaColor Color { get; set; }
    public string? Result { get; set; }
    public SurveyStatus Status { get; set; }
    public DateTime DateCreated { get; init; }
    public DateTime? DateFinished { get; set; }
}
