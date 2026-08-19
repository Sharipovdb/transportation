using Transportation.Application.MonthlyTransportSheet.Models;

namespace Transportation.Application.MonthlyTransportSheet.Mappers;

/// <summary>
/// The only place a sheet becomes a DTO. A preview is an unsaved sheet entity, so it
/// travels through here too — which is what keeps "what you previewed" and "what was
/// generated" from ever being two different calculations.
/// </summary>
public sealed class MonthlyTransportSheetMapper
{
    public MonthlyTransportSheetDto Map(Domain.Entities.MonthlyTransportSheet entity)
    {
        return new MonthlyTransportSheetDto
        {
            Id = entity.Id,
            CrewId = entity.CrewId,
            Year = entity.Year,
            Month = entity.Month,
            RecipientId = entity.RecipientId,
            RecipientFullname = entity.Recipient?.Fullname ?? string.Empty,
            IsConfirmed = entity.IsConfirmed,
            IsPaid = entity.IsPaid,
            PaidAt = entity.PaidAt,
            Days = entity.Days
                .OrderBy(x => x.Date)
                .Select(MapDay)
                .ToList()
        };
    }

    public List<MonthlyTransportSheetDto> Map(List<Domain.Entities.MonthlyTransportSheet> entities)
    {
        return entities.Select(Map).ToList();
    }

    private static MonthlyTransportSheetDayDto MapDay(Domain.Entities.MonthlyTransportSheetDay entity)
    {
        return new MonthlyTransportSheetDayDto
        {
            TransportDayId = entity.TransportDayId,
            Date = entity.Date,
            DrivenKm = entity.DrivenKm,
            ExtraBusinessKm = entity.ExtraBusinessKm,
            TaxiAmount = entity.TaxiAmount
        };
    }
}
