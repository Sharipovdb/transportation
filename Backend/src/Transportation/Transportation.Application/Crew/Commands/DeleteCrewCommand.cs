using FluentValidation;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Mediator.Helper.Commands;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Application.Crew.Commands;

public record DeleteCrewCommand(long Id) : ICommand<bool>;

// ReSharper disable once UnusedType.Global
public class DeleteCrewCommandValidator : AbstractValidator<DeleteCrewCommand>
{
    public DeleteCrewCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Crew ID must be greater than 0");
    }
}

internal sealed class DeleteCrewCommandHandler : ICommandHandler<DeleteCrewCommand, bool>
{
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCrewCommandHandler(ICrewRepository crewRepository, IUnitOfWork unitOfWork)
    {
        _crewRepository = crewRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(DeleteCrewCommand request, CancellationToken cancellationToken)
    {
        var spec = new CrewByIdSpec(request.Id);
        var entity = await _crewRepository.FirstOrDefaultAsync(spec, cancellationToken);

        if (entity is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);

        entity.IsDeleted = true;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}