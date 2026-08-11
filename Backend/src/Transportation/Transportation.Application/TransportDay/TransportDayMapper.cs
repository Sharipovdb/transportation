using Transportation.Application.TaxiExpense.Models;
using Transportation.Application.TransportDay.Models;

namespace Transportation.Application.TransportDay;

public sealed class TransportDayMapper
{
    public TransportDayDto Map(Domain.Entities.TransportDay entity)
    {
        return new TransportDayDto
        {
            Id = entity.Id,
            CrewId = entity.CrewId,
            Date = entity.Date,
            MorningMode = entity.MorningMode.ToString(),
            AfternoonMode = entity.AfternoonMode?.ToString(),
            DriverId = entity.DriverId,
            CommuteKmPerLeg = entity.CommuteKmPerLeg,
            DrivenCommuteKm = entity.DrivenCommuteKm,
            ExtraBusinessKm = entity.ExtraBusinessKm,
            Notes = entity.Notes,
            LoggedBy = entity.LoggedBy,
            LoggedAt = entity.LoggedAt,
            Confirmed = entity.Confirmed,
            TaxiExpenses = entity.TaxiExpenses.Select(MapTaxiExpense).ToList()
        };
    }

    public List<TransportDayDto> Map(List<Domain.Entities.TransportDay> entities)
    {
        return entities.Select(Map).ToList();
    }

    private static TaxiExpenseDto MapTaxiExpense(Domain.Entities.TaxiExpense entity)
    {
        return new TaxiExpenseDto
        {
            Id = entity.Id,
            TransportDayId = entity.TransportDayId,
            PaidById = entity.PaidById,
            Amount = entity.Amount,
            Leg = entity.Leg.ToString(),
            TaxiExpenseStatus = entity.TaxiExpenseStatus.ToString()
        };
    }
}