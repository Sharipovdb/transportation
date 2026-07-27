using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.PayoutLineTaxiExpense.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class PayoutLineTaxiExpenseRepository
    : RepositoryBase<PayoutLineTaxiExpense>, IPayoutLineTaxiExpenseRepository
{
    public PayoutLineTaxiExpenseRepository(TransportationDbContext dbContext) : base(dbContext)
    {
    }
}