using FluentValidation;
using Transportation.Application.AbsenceNotice.Models;
using Transportation.Application.AbsenceNotice.Repositories;
using Transportation.Application.AbsenceNotice.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.AbsenceNotice.Commands;

public sealed record CreateAbsenceNoticeCommand(
    long UserId,
    DateOnly Date,
    AbsenceScope Scope,
    AbsenceType Type,
    string? Reason
) : ICommand<AbsenceNoticeDto>;

// ReSharper disable once UnusedType.Global
public sealed class CreateAbsenceNoticeValidator : AbstractValidator<CreateAbsenceNoticeCommand>
{
    public CreateAbsenceNoticeValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithMessage("User id must be greater than 0");

        RuleFor(x => x.Date).NotEmpty();

        RuleFor(x => x.Scope).IsInEnum();
        
        RuleFor(x => x.Type).IsInEnum();
        
        RuleFor(x => x.Reason).MaximumLength(500);
    }
}

internal sealed class CreateAbsenceNoticeHandler : ICommandHandler<CreateAbsenceNoticeCommand, AbsenceNoticeDto>
{
    private readonly IAbsenceNoticeRepository _absenceNoticeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly AbsenceNoticeMapper _mapper;
    private readonly TimeProvider _timeProvider;

    public CreateAbsenceNoticeHandler(
        IAbsenceNoticeRepository absenceNoticeRepository,
        IUnitOfWork unitOfWork,
        AbsenceNoticeMapper mapper,
        TimeProvider timeProvider)
    {
        _absenceNoticeRepository = absenceNoticeRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    public async Task<AbsenceNoticeDto> Handle(CreateAbsenceNoticeCommand request, CancellationToken cancellationToken)
    {
        var existsSpec = new AbsenceNoticeByUserDateSpec(request.UserId, request.Date, request.Scope);
        var exists = await _absenceNoticeRepository.AnyAsync(existsSpec, cancellationToken);

        if (exists)
            throw new BusinessLogicException(AbsenceNoticeErrors.AlreadyExists);

        var entity = new Domain.Entities.AbsenceNotice
        {
            UserId = request.UserId,
            Date = request.Date,
            Scope = request.Scope,
            Type = request.Type,
            Reason = request.Reason!,
            IsNotified = false,
            CreatedAt = _timeProvider.GetLocalDateTimeNowKindUtc()
        };

        await _absenceNoticeRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map(entity);
    }
}