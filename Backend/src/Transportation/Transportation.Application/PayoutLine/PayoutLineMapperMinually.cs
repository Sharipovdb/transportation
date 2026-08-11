using Transportation.Application.MonthlyTransportSheet.Models;
using Transportation.Application.PayoutLine.Models;

namespace Transportation.Application.PayoutLine;

public class PayoutLineMapperMinually
{
    public PayoutLineDto Map(Domain.Entities.PayoutLine entity)
    {
        return new PayoutLineDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            FirstName = entity.User?.FirstName ?? string.Empty,
            LastName = entity.User?.LastName ?? string.Empty,
            DriverKm = entity.DriverKm,
            ExtraBusinessKm = entity.ExtraBusinessKm,
            TaxiCompensation = entity.TaxiCompensation ?? 0,
            IsPaid = entity.IsPaid,
            PaidAt = entity.PaidAt,
            TaxiExpenses = entity.TaxiExpenses.Select(x =>
                new TaxiExpenseSummaryDto
                {
                    Id = x.TaxiExpenseId,
                    Amount = x.TaxiExpense.Amount,
                    Leg = x.TaxiExpense.Leg,
                    TaxiExpenseStatus = x.TaxiExpense.TaxiExpenseStatus
                }
            ).ToList()
        };
    }

    public List<PayoutLineDto> Map(IEnumerable<Domain.Entities.PayoutLine> entities)
    {
        return entities.Select(Map).ToList();
    }
}