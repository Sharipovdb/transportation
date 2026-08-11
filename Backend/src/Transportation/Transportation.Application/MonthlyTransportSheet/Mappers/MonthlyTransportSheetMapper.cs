using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.PayoutLine.Models;

namespace Transportation.Application.MonthlyTransportSheet.Mappers;

public class MonthlyTransportSheetMapper
{
    public MonthlyTransportSheetDto Map(Domain.Entities.MonthlyTransportSheet entity)
    {
        return new MonthlyTransportSheetDto
        {
            Id = entity.Id,
            CrewId = entity.CrewId,
            Year = entity.Year,
            Month = entity.Month,
            IsConfirmed = entity.IsConfirmed,

            PayoutLines = entity.PayoutLines.Select(MapPayoutLine).ToList()
        };
    }

    public List<MonthlyTransportSheetDto> Map(List<Domain.Entities.MonthlyTransportSheet> entities)
    {
        return entities.Select(Map).ToList();
    }

    private PayoutLineDto MapPayoutLine(Domain.Entities.PayoutLine entity)
    {
        return new PayoutLineDto
        {
            Id = entity.Id,
            UserId = entity.UserId,

            FirstName = entity.User.FirstName,
            LastName = entity.User.LastName,

            DriverKm = entity.DriverKm,
            ExtraBusinessKm = entity.ExtraBusinessKm,

            TaxiCompensation = entity.TaxiCompensation ?? 0,

            IsPaid = entity.IsPaid,
            PaidAt = entity.PaidAt,

            TaxiExpenses = entity.TaxiExpenses
                .Select(x => new TaxiExpenseSummaryDto
                    {
                        Id = x.TaxiExpense.Id,
                        Amount = x.TaxiExpense.Amount,
                        Leg = x.TaxiExpense.Leg,
                        TaxiExpenseStatus = x.TaxiExpense.TaxiExpenseStatus
                    }
                ).ToList()
        };
    }
}