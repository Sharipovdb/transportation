using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.MonthlyTransportSheet.Commands;

public sealed record DeleteMonthlyTransportSheetCommand(long Id) : ICommand<bool>;

internal sealed class DeleteMonthlyTransportSheetCommandHandler
    : ICommandHandler<DeleteMonthlyTransportSheetCommand, bool>
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteMonthlyTransportSheetCommandHandler(
        IMonthlyTransportSheetRepository monthlyTransportSheetRepository,
        IUnitOfWork unitOfWork)
    {
        _monthlyTransportSheetRepository = monthlyTransportSheetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteMonthlyTransportSheetCommand request, CancellationToken cancellationToken)
    {
        var spec = new MonthlyTransportSheetByIdSpec(request.Id);
        var monthlyTransportSheet = await _monthlyTransportSheetRepository
            .FirstOrDefaultAsync(spec, cancellationToken);

        if (monthlyTransportSheet is null)
            throw new ResourceNotFoundException(MonthlyTransportSheetErrors.NotFound);

        if (monthlyTransportSheet.IsConfirmed)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.AlreadyConfirmed);


        monthlyTransportSheet.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}