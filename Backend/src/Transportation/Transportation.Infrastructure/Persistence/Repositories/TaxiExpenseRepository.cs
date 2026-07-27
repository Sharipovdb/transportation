using Ardalis.Specification.EntityFrameworkCore;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Repositories;

internal sealed class TaxiExpenseRepository : RepositoryBase<TaxiExpense>, ITaxiExpenseRepository
{
    public TaxiExpenseRepository(TransportationDbContext dbContext) : base(dbContext)
    {
    }
}