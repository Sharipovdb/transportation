using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.PayoutLine.Models;
using Transportation.Application.PayoutLine.Repositories;
using Transportation.Application.PayoutLine.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Queries;

namespace Transportation.Application.PayoutLine.Queries;

public sealed record GetPayoutLineById(long Id) : IQuery<PayoutLineDto>;
    
internal sealed class GetPayoutLineByIdHandler : IQueryHandler<GetPayoutLineById, PayoutLineDto>
{
    private readonly IPayoutLineRepository _payoutLineRepository;
    private readonly PayoutLineMapper _mapper;
    public GetPayoutLineByIdHandler(IPayoutLineRepository payoutLineRepository, PayoutLineMapper mapper)
    {
        _payoutLineRepository = payoutLineRepository;
        _mapper = mapper;
    }

    public async Task<PayoutLineDto> Handle(GetPayoutLineById request, CancellationToken cancellationToken)
    {
        var spec = new PayoutLineByIdSpec(request.Id);
        var entity = await _payoutLineRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(PayoutLineErrors.NotFound);

        return _mapper.Map(entity);
    }
}
