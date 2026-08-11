using System.Security.Claims;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Commands;
using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Services;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Shared;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.TransportDay.Commands;

public class CreateDransportDayCommandTests
{
    private const long CrewId = 7;
    private const long DriverLeadId = 42;
    private const long LoggedByUserId = 5;
    private const double RouteDistanceKm = 18.5;

    private readonly ITransportDayRepository _transportDayRepository;
    private readonly ICrewRepository _crewRepository;
    private readonly ITransportDayTaxiFareService _taxiFareService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FakeTimeProvider _fakeTimeProvider = new();

    private readonly CreateTransportDayCommandHandler _handler;

    public CreateDransportDayCommandTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        fixture.Register<TimeProvider>(() => _fakeTimeProvider);

        _transportDayRepository = fixture.Freeze<ITransportDayRepository>();
        _crewRepository = fixture.Freeze<ICrewRepository>();
        _taxiFareService = fixture.Freeze<ITransportDayTaxiFareService>();
        _unitOfWork = fixture.Freeze<IUnitOfWork>();

        var currentUserAccessor = fixture.Freeze<ICurrentUserAccessor>();
        currentUserAccessor.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, LoggedByUserId.ToString())]));

        _crewRepository
            .FirstOrDefaultAsync(Arg.Any<CrewByIdSpec>())
            .Returns(new Domain.Entities.Crew
            {
                Id = CrewId,
                DriverLeadId = DriverLeadId,
                Route = new Route { DistanceKm = RouteDistanceKm }
            });

        _handler = fixture.Create<CreateTransportDayCommandHandler>();
    }

    private static CreateTransportDayCommand ACommand(
        TransportMode morningMode = TransportMode.Driven,
        TransportMode? afternoonMode = TransportMode.Driven,
        IReadOnlyList<TransportDayTaxiFare>? taxiFares = null)
    {
        return new CreateTransportDayCommand(
            CrewId,
            new DateOnly(2026, 8, 3),
            morningMode,
            afternoonMode,
            ExtraCommuteKm: 2.5,
            ExtraBusinessKm: 4,
            Notes: "Logged from the depot",
            taxiFares);
    }

    [Fact]
    public async Task TransportDayCommandCreteShold()
    {
        //Arrange
        var command = ACommand();

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
        var command = ACommand();

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
            entity.Date.Should().Be(command.Date);
            entity.MorningMode.Should().Be(command.MorningMode);
            entity.AfternoonMode.Should().Be(command.AfternoonMode);
            entity.ExtraCommuteKm.Should().Be(command.ExtraCommuteKm);
            entity.ExtraBusinessKm.Should().Be(command.ExtraBusinessKm);
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

    [Fact]
    public async Task TransportDayCommandCrete_ShouldSnapshotRouteAndDriverFromCrew()
    {
        // Arrange
        var command = ACommand();

        _transportDayRepository
            .AnyAsync(Arg.Any<TransportDayByCrewAndDateSpec>())
            .Returns(false);

        Domain.Entities.TransportDay entity = null!;
        _transportDayRepository
            .When(x => x.AddAsync(Arg.Any<Domain.Entities.TransportDay>()))
            .Do(x => entity = x.Arg<Domain.Entities.TransportDay>());

        // Act
        var dto = await _handler.Handle(command, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            entity.BaseRouteKm.Should().Be(RouteDistanceKm);
            entity.DriverId.Should().Be(DriverLeadId);

            // Both legs driven: the crew covered the route twice under its own steam.
            entity.CommuteKmPerLeg.Should().Be(RouteDistanceKm + command.ExtraCommuteKm);
            entity.DrivenCommuteKm.Should().Be(2 * (RouteDistanceKm + command.ExtraCommuteKm));

            dto.CommuteKmPerLeg.Should().Be(entity.CommuteKmPerLeg);
            dto.DrivenCommuteKm.Should().Be(entity.DrivenCommuteKm);
        }
    }

    [Fact]
    public async Task TransportDayCommandCrete_ShouldRecordTaxiFaresOnTheDay()
    {
        // Arrange
        var fares = new[] { new TransportDayTaxiFare(Leg.Afternoon, 35m, DriverLeadId) };

        var command = ACommand(
            morningMode: TransportMode.Driven,
            afternoonMode: TransportMode.Taxi,
            taxiFares: fares);

        _transportDayRepository
            .AnyAsync(Arg.Any<TransportDayByCrewAndDateSpec>())
            .Returns(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — the day owns its taxi expenses, so the fares travel with it.
        _ = _taxiFareService
            .Received()
            .SyncAsync(
                Arg.Any<Domain.Entities.TransportDay>(),
                Arg.Is<IReadOnlyList<TransportDayTaxiFare>>(x => x.SequenceEqual(fares)),
                Arg.Any<DateTime>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task TransportDayCommandCrete_TaxiLegShouldNotCountAsDrivenKm()
    {
        // Arrange
        var command = ACommand(
            morningMode: TransportMode.Taxi,
            afternoonMode: TransportMode.Taxi,
            taxiFares: new[]
            {
                new TransportDayTaxiFare(Leg.Morning, 30m, DriverLeadId),
                new TransportDayTaxiFare(Leg.Afternoon, 30m, DriverLeadId)
            });

        _transportDayRepository
            .AnyAsync(Arg.Any<TransportDayByCrewAndDateSpec>())
            .Returns(false);

        Domain.Entities.TransportDay entity = null!;
        _transportDayRepository
            .When(x => x.AddAsync(Arg.Any<Domain.Entities.TransportDay>()))
            .Do(x => entity = x.Arg<Domain.Entities.TransportDay>());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — a taxi ride is settled in money, never in kilometres.
        entity.DrivenCommuteKm.Should().Be(0);
    }
}
