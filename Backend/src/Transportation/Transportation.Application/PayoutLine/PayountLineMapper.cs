using Riok.Mapperly.Abstractions;
using Transportation.Application.PayoutLine.Models;

namespace Transportation.Application.PayoutLine;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PayoutLineMapper
{
    public partial PayoutLineDto Map(Domain.Entities.PayoutLine entity);

    public partial List<PayoutLineDto> Map(List<Domain.Entities.PayoutLine> entities);
}
