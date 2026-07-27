using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Commands;
using Transportation.Application.Crew.Specification;
using Transportation.Application.Crew;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.Crew;

public class CreateCrewTest
{
    private readonly ICrewRepository _crewRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FakeTimeProvider _timeProvider = new();

    private readonly CreateCrewCommandHandler _handler;

    public CreateCrewTest()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();
        fixture.Register<TimeProvider>(() => _timeProvider);

        _crewRepository = fixture.Freeze<ICrewRepository>();
        _unitOfWork = fixture.Freeze<IUnitOfWork>();

        _handler = fixture.Create<CreateCrewCommandHandler>();
    }

    [Fact]
    public async Task CrewAlreadyExists_ShouldThrow()
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<CreateCrewCommand>();

        _crewRepository
            .AnyAsync(Arg.Any<CrewExistsSpec>())
            .Returns(true);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(CrewErrors.AlreadyExists);
    }
    
}