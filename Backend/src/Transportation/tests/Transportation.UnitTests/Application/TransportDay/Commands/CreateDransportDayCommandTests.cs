using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Commands;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.TransportDay.Commands;

public class CreateDransportDayCommandTests
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FakeTimeProvider _fakeTimeProvider = new();

    private readonly CreateTransportDayCommandHandler _handler;

    public CreateDransportDayCommandTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        fixture.Register<TimeProvider>(() => _fakeTimeProvider);

        _transportDayRepository = fixture.Freeze<ITransportDayRepository>();
        _unitOfWork = fixture.Freeze<IUnitOfWork>();

        _handler = fixture.Create<CreateTransportDayCommandHandler>();
    }

    [Fact]
    public async Task TransportDayCommandCreteShold()
    {
        //Arrange
        var fixture = new Fixture();
        var command = fixture.Create<CreateTransportDayCommand>();

        _transportDayRepository
            .AnyAsync(Arg.Any<TransportDayByCrewAndDateSpec>())
            .Returns(true);

        //Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        //Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TransportDayErrors.AlreadyExists);
    }

    [Fact]
    public async Task TransportDayCommandCrete_SholdBeSuccess()
    {
        //Arrange 
        var fixture = new Fixture();
        var command = fixture.Create<CreateTransportDayCommand>();

        _transportDayRepository
            .AnyAsync(Arg.Any<TransportDayByCrewAndDateSpec>())
            .Returns(false);

        Domain.Entities.TransportDay entity = null!;
        _transportDayRepository
            .When(x => x.AddAsync(Arg.Any<Domain.Entities.TransportDay>()))
            .Do(x => entity = x.Arg<Domain.Entities.TransportDay>());

        var dateTimeOffsetNow = DateTimeOffset.Now;
        var datetimeNow = DateTime.SpecifyKind(dateTimeOffsetNow.UtcDateTime, DateTimeKind.Utc);
        _fakeTimeProvider.SetUtcNow(dateTimeOffsetNow);

        //Act
        var dto = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _ = _transportDayRepository
            .Received()
            .AnyAsync(Arg.Is<TransportDayByCrewAndDateSpec>(x => true));

        _ = _transportDayRepository
            .Received()
            .AddAsync(Arg.Any<Domain.Entities.TransportDay>());

        _ = _unitOfWork
            .Received()
            .SaveChangesAsync();

        using (new AssertionScope())
        {
            entity.CrewId.Should().Be(command.CrewId);
            entity.Date.Should().Be(command.Date.ToUniversalTime());
            entity.MorningMode.Should().Be(command.MorningMode);
            entity.AfternoonMode.Should().Be(command.AfternoonMode);
            entity.ExtraCommuteKm.Should().Be(command.ExtraCommuteKm);
            entity.ExtraBusinessKm.Should().Be(command.ExtraBusinessCm);
            entity.Notes.Should().Be(command.Notes);
            entity.LoggedAt.Should().Be(datetimeNow);
            entity.CreatedAt.Should().Be(datetimeNow);
            entity.Confirmed.Should().BeFalse();

            dto.CrewId.Should().Be(entity.CrewId);
            dto.Date.Should().Be(entity.Date);
            dto.ExtraBusinessKm.Should().Be(entity.ExtraBusinessKm);
            dto.Notes.Should().Be(entity.Notes);
            dto.LoggedBy.Should().Be(entity.LoggedBy);
            dto.LoggedAt.Should().Be(entity.LoggedAt);
            dto.Confirmed.Should().Be(entity.Confirmed);
        }
    }
}