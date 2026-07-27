using Ardalis.Specification;

namespace Transportation.Application.TaxiExpense.Specifications;

public class TaxiExpenseByIdSpec : Specification<Domain.Entities.TaxiExpense>
{
    public TaxiExpenseByIdSpec(long id)
    {
        Query.Where(x => x.Id == id && !x.IsDeleted);
    }
}