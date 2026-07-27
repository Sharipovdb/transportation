using Riok.Mapperly.Abstractions;
using Transportation.Application.TaxiExpense.Models;

namespace Transportation.Application.TaxiExpense;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class TaxiExpenseMapper
{
    public partial TaxiExpenseDto Map(Domain.Entities.TaxiExpense entity);
    
    public partial List<TaxiExpenseDto> Map (List<Domain.Entities.TaxiExpense> entities);
}
