using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.PayoutLine.Models;
using Transportation.Application.PayoutLine.Repositories;
using Transportation.Application.PayoutLine.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.PayoutLine.Commands;

public sealed record UpdatePayoutLineCommand(
    long Id,
    decimal DriverPayment,
    decimal ExtraKmPayment,
    decimal TaxiCompensation
) : ICommand<PayoutLineDto>;

internal sealed class UpdatePayoutLineCommandHandler : ICommandHandler<UpdatePayoutLineCommand, PayoutLineDto>
{
    private readonly IPayoutLineRepository _payoutLineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayoutLineMapper _mapper;

    public UpdatePayoutLineCommandHandler(
        IPayoutLineRepository payoutLineRepository,
        IUnitOfWork unitOfWork,
        PayoutLineMapper mapper)
    {
        _payoutLineRepository = payoutLineRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PayoutLineDto> Handle(UpdatePayoutLineCommand request, CancellationToken cancellationToken)
    {
        var spec = new PayoutLineByIdSpec(request.Id);
        var entity = await _payoutLineRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(PayoutLineErrors.NotFound);

        entity.DriverPayment = request.DriverPayment;
        entity.ExtraKmPayment = request.ExtraKmPayment;
        entity.TaxiCompensation = request.TaxiCompensation;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}