using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.PayoutLine.Repositories;
using Transportation.Application.PayoutLine.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.PayoutLine.Commands;

public sealed record DeletePayoutLineCommand(long Id) : ICommand<bool>;

internal sealed class DeletePayoutLineCommandHandler : ICommandHandler<DeletePayoutLineCommand, bool>
{
    private readonly IPayoutLineRepository _payoutLineRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePayoutLineCommandHandler(IPayoutLineRepository payoutLineRepository, IUnitOfWork unitOfWork)
    {
        _payoutLineRepository = payoutLineRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeletePayoutLineCommand request, CancellationToken cancellationToken)
    {
        var spec = new PayoutLineByIdSpec(request.Id);
        var entity = await _payoutLineRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(PayoutLineErrors.NotFound);

        entity.IsDeleted = true;
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}