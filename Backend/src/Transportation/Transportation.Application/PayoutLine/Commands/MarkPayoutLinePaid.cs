using FluentValidation;
using Transportation.Application.PayoutLine.Repositories;
using Transportation.Application.PayoutLine.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Shared;
using Transportation.Shared.Extensions;
using Transportation.Shared.Middlewares;

namespace Transportation.Application.PayoutLine.Commands;

public sealed record MarkPayoutLinePaidCommand(long Id) : ICommand;

// ReSharper disable once UnusedType.Global
public sealed class MarkPayoutLinePaidCommandValidator : AbstractValidator<MarkPayoutLinePaidCommand>
{
    public MarkPayoutLinePaidCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0");
    }
}

/// <summary>
/// Settles one crew member's payout for one month. The line is the unit of settlement:
/// driver km, extra business km and taxi reimbursement are released together, and the
/// <see cref="Domain.Entities.PayoutLine.IsPaid"/> flag makes a second payment for the
/// same member and month impossible.
/// </summary>
internal sealed class MarkPayoutLinePaidCommandHandler : ICommandHandler<MarkPayoutLinePaidCommand>
{
    private readonly IPayoutLineRepository _payoutLineRepository;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public MarkPayoutLinePaidCommandHandler(
        IPayoutLineRepository payoutLineRepository,
        ICurrentUserAccessor currentUserAccessor,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _payoutLineRepository = payoutLineRepository;
        _currentUserAccessor = currentUserAccessor;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task Handle(MarkPayoutLinePaidCommand request, CancellationToken cancellationToken)
    {
        var payoutLine = await _payoutLineRepository.FirstOrDefaultAsync(
            new PayoutLineByIdSpec(request.Id),
            cancellationToken);

        if (payoutLine is null)
            throw new ResourceNotFoundException(PayoutLineErrors.NotFound);

        if (payoutLine.IsPaid)
            throw new BusinessLogicException(PayoutLineErrors.AlreadyPaid);

        // A zero line exists only so the member shows up on the sheet — there is no
        // money to release, so marking it paid would be a meaningless audit entry.
        if (payoutLine.TotalAmount <= 0)
            throw new BusinessLogicException(PayoutLineErrors.NothingToPay);

        var now = _timeProvider.GetLocalDateTimeNowKindUtc();

        payoutLine.IsPaid = true;
        payoutLine.PaidAt = now;
        payoutLine.PaidById = _currentUserAccessor.GetRequiredUser().GetUserId();
        payoutLine.UpdatedAt = now;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
