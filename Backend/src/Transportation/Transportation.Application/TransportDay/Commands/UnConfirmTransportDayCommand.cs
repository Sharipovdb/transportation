using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;

namespace Transportation.Application.TransportDay.Commands;

public sealed record UnConfirmTransportDayCommand(
    long TransportDayId
) : ICommand<TransportDayDto>;

internal sealed class UnConfirmTransportDayCommandHandler
    : ICommandHandler<UnConfirmTransportDayCommand, TransportDayDto>
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly TransportDayMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public UnConfirmTransportDayCommandHandler(
        ITransportDayRepository transportDayRepository,
        TransportDayMapper mapper,
        TimeProvider timeProvider)
    {
        _transportDayRepository = transportDayRepository;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<TransportDayDto> Handle(
        UnConfirmTransportDayCommand request,
        CancellationToken cancellationToken)
    {
        var spec = new TransportDayByIdSpec(request.TransportDayId);

        var transportDay = await _transportDayRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (transportDay is null)
            throw new ResourceNotFoundException(TransportDayErrors.NotFound);

        if (!transportDay.Confirmed)
            throw new BusinessLogicException(TransportDayErrors.AlreadyUnConfirmed);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        transportDay.Confirmed = false;
        transportDay.UpdatedAt = now;

        // Confirming approved the day's pending fares; unconfirming takes that approval
        // back so the day is editable again. Fares that were rejected, or already paid
        // out, are decisions of their own and stay where they are.
        foreach (var taxiExpense in transportDay.TaxiExpenses
                     .Where(x => !x.IsDeleted && x.TaxiExpenseStatus is TaxiExpenseStatus.Approved))
        {
            taxiExpense.TaxiExpenseStatus = TaxiExpenseStatus.Pending;
            taxiExpense.UpdatedAt = now;
        }

        await _transportDayRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map(transportDay);
    }
}