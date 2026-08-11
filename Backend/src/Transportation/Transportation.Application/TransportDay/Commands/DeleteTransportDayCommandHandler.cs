using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.TransportDay.Commands;

public sealed record DeleteTransportDayCommand(long Id) : ICommand<bool>;

internal sealed class DeleteTransportDayCommandHandler : ICommandHandler<DeleteTransportDayCommand, bool>
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTransportDayCommandHandler(
        ITransportDayRepository transportDayRepository,
        IUnitOfWork unitOfWork)
    {
        _transportDayRepository = transportDayRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteTransportDayCommand request, CancellationToken cancellationToken)
    {
        var spec = new TransportDayByIdSpec(request.Id);
        var entity = await _transportDayRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(TransportDayErrors.NotFound);

        entity.IsDeleted = true;

        // Taxi expenses only exist as part of the day that produced them — leaving them
        // behind would keep claiming money for a ride that is no longer on record.
        foreach (var taxiExpense in entity.TaxiExpenses.Where(x => !x.IsDeleted))
            taxiExpense.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}