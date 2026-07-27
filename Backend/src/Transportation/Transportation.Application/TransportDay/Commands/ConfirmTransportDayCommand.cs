using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Common.Models;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.TransportDay.Commands;

public sealed record ConfirmTransportDayCommand(
    long TransportDayId
) : ICommand<TransportDayDto>;

internal class ConfirmTransportDayCommandHandler : ICommandHandler<ConfirmTransportDayCommand, TransportDayDto>
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TransportDayMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public ConfirmTransportDayCommandHandler(
        ITransportDayRepository transportDayRepository,
        TransportDayMapper mapper,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork)
    {
        _transportDayRepository = transportDayRepository;
        _mapper = mapper;
        _timeProvider = timeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransportDayDto> Handle(ConfirmTransportDayCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var spec = new TransportDayByIdSpec(request.TransportDayId);
            var transportDay = await _transportDayRepository.FirstOrDefaultAsync(spec, cancellationToken);

            if (transportDay is null)
                throw new ResourceNotFoundException(TransportDayErrors.NotFound);

            if (transportDay.Confirmed)
                throw new BusinessLogicException(new Error(
                    "TransportDay.AlreadyConfirmed",
                    "Transport day already confirmed")
                );

            transportDay.Confirmed = true;
            transportDay.UpdatedAt = _timeProvider.GetLocalDateTimeNowKindUtc();

            foreach (var taxiExpense in transportDay.TaxiExpenses
                         .Where(x => !x.IsDeleted && x.TaxiExpenseStatus is TaxiExpenseStatus.Pending))
            {
                taxiExpense.TaxiExpenseStatus = TaxiExpenseStatus.Approved;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            
            return _mapper.Map(transportDay);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}