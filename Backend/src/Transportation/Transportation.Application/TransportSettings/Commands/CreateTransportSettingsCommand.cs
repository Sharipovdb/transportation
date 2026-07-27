using FluentValidation;
using Transportation.Application.TransportSettings.Repositories;
using Transportation.Application.TransportSettings.Specifications;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Common.Extensions;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.TransportSettings.Commands;

public sealed record CreateTransportSettingsCommand(
    decimal CommuteKmRate,
    decimal ExtraBusinessKmRate,
    DateTime EffectiveFrom
) : ICommand<long>;

// ReSharper disable once UnusedType.Global
public sealed class CreateTransportSettingsCommandValidator : AbstractValidator<CreateTransportSettingsCommand>
{
    public CreateTransportSettingsCommandValidator()
    {
        RuleFor(x => x.CommuteKmRate)
            .GreaterThan(0)
            .WithMessage("CommuteKmRate must be greater than 0");

        RuleFor(x => x.ExtraBusinessKmRate)
            .GreaterThan(0)
            .WithMessage("ExtraBusinessKmRate must be greater than 0");

        RuleFor(x => x.EffectiveFrom)
            .NotEmpty();
    }
}

internal sealed class CreateTransportSettingsCommandHandler : ICommandHandler<CreateTransportSettingsCommand, long>
{
    private readonly ITransportSettingsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public CreateTransportSettingsCommandHandler(
        ITransportSettingsRepository repository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<long> Handle(
        CreateTransportSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var exists = await _repository.AnyAsync(
            new TransportSettingsByEffectiveDateSpec(request.EffectiveFrom), cancellationToken);

        if (exists)
            throw new ResourceNotFoundException(TransportSettingsErrors.NotFound);

        var entity = new Domain.Entities.TransportSettings
        {
            CommuteKmRate = request.CommuteKmRate,
            ExtraBusinessKmRate = request.ExtraBusinessKmRate,
            EffectiveFrom = request.EffectiveFrom.Date,
            CreatedAt = _timeProvider.GetLocalDateTimeNowKindUtc()
        };

        await _repository.AddAsync(entity, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}