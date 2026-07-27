using FluentValidation;
using Transportation.Application.PayoutLine.Models;
using Transportation.Application.PayoutLine.Repositories;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.PayoutLine.Commands;

public sealed record CreatePayoutLineCommand(
    long MonthlyTransportSheetId,
    long UserId,
    decimal DriverPayment,
    decimal ExtraKmPayment,
    decimal TaxiCompensation
) : ICommand<PayoutLineDto>;

// ReSharper disable once UnusedType.Global
public sealed class CreatePayoutLineCommandValidator : AbstractValidator<CreatePayoutLineCommand>
{
    public CreatePayoutLineCommandValidator()
    {
        RuleFor(x => x.MonthlyTransportSheetId).GreaterThan(0);
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.DriverPayment).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ExtraKmPayment).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TaxiCompensation).GreaterThanOrEqualTo(0);
    }
}

internal sealed class CreatePayoutLineCommandHandler : ICommandHandler<CreatePayoutLineCommand, PayoutLineDto>
{
    private readonly IPayoutLineRepository _payoutLineRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly PayoutLineMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public CreatePayoutLineCommandHandler(
        IPayoutLineRepository payoutLineRepository,
        IUnitOfWork unitOfWork,
        PayoutLineMapper mapper, 
        TimeProvider timeProvider)
    {
        _payoutLineRepository = payoutLineRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<PayoutLineDto> Handle(CreatePayoutLineCommand request, CancellationToken cancellationToken)
    {
        var entity = new Domain.Entities.PayoutLine
        {
            MonthlyTransportSheetId = request.MonthlyTransportSheetId,
            UserId = request.UserId,
            DriverPayment = request.DriverPayment,
            ExtraKmPayment = request.ExtraKmPayment,
            TaxiCompensation = request.TaxiCompensation,
            CreatedAt = _timeProvider.GetLocalDateTimeNowKindUtc(),
        };
        
        await _payoutLineRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}