using Ardalis.Specification;

namespace Transportation.Application.PayoutLine.Specifications;

public sealed class PayoutLineByIdSpec : Specification<Domain.Entities.PayoutLine>
{
    public long Id { get; set; }
    
    public PayoutLineByIdSpec(long id, bool asNoTracking = false)
    {
        Id = id;
        
        if (asNoTracking)
            Query.AsNoTracking();

        Query.Where(x => x.Id == id && !x.IsDeleted);
    }
}