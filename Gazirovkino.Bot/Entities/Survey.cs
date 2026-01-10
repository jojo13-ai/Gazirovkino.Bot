using System;
using Gazirovkino.Bot.Entities.Gazirovka;

namespace Gazirovkino.Bot.Entities;

public class Survey
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public GazirovkaTaste Taste { get; init; }
    public GazirovkaAdditions Additions { get; init; }
    public GazirovkaColor Color { get; init; }
    public SurveyStatus Status { get; set; }
    public DateTime DateCreated { get; init; }
    public DateTime? DateFinished { get; set; }
}