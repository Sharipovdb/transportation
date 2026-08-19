using System.Security.Claims;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Transportation.Application.MonthlyTransportSheet;
using Transportation.Application.MonthlyTransportSheet.Commands;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Services;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.Shared;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.MonthlyTransportSheet.Commands;

public class GenerateMonthlyTransportSheetCommandTests
{
    private const long CrewId = 3;
    private const long LeadId = 7;
    private const int Year = 2026;
    private const int Month = 8;

    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly IMonthlyTransportSheetBuilder _builder;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FakeTimeProvider _fakeTimeProvider = new();

    private readonly GenerateMonthlyTransportSheetCommandHandler _handler;

    public GenerateMonthlyTransportSheetCommandTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        fixture.Register<TimeProvider>(() => _fakeTimeProvider);

        _monthlyTransportSheetRepository = fixture.Freeze<IMonthlyTransportSheetRepository>();
        _builder = fixture.Freeze<IMonthlyTransportSheetBuilder>();
        _unitOfWork = fixture.Freeze<IUnitOfWork>();

        fixture.Freeze<ICurrentUserAccessor>().User = AnAccountant();

        _handler = fixture.Create<GenerateMonthlyTransportSheetCommandHandler>();
    }

    private static ClaimsPrincipal AnAccountant()
        => new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "42")]));

    private static GenerateMonthlyTransportSheetCommand ACommand()
        => new(CrewId, Year, Month);

    private static Domain.Entities.MonthlyTransportSheet ASheet(
        long id = 0,
        params MonthlyTransportSheetDay[] days)
    {
        return new Domain.Entities.MonthlyTransportSheet
        {
            Id = id,
            CrewId = CrewId,
            Year = Year,
            Month = Month,
            RecipientId = LeadId,
            Recipient = new User { Id = LeadId, FirstName = "Abbos", LastName = "Kamolov" },
            Days = days.ToList()
        };
    }

    private static MonthlyTransportSheetDay ADay(int dayOfMonth, double drivenKm = 40, decimal taxi = 0)
        => new()
        {
            TransportDayId = dayOfMonth,
            Date = new DateOnly(Year, Month, dayOfMonth),
            DrivenKm = drivenKm,
            TaxiAmount = taxi
        };

    private void GivenCalculated(params MonthlyTransportSheetDay[] days)
    {
        _builder
            .BuildAsync(CrewId, Year, Month, Arg.Any<CancellationToken>())
            .Returns(ASheet(days: days));
    }

    private void GivenExistingSheet(Domain.Entities.MonthlyTransportSheet? sheet)
    {
        _monthlyTransportSheetRepository
            .FirstOrDefaultAsync(Arg.Any<MonthlyTransportSheetByPeriodSpec>())
            .Returns(sheet);
    }

    [Fact]
    public async Task NoSheetForThePeriod_ShouldCreateOne()
    {
        // Arrange
        GivenExistingSheet(null);
        GivenCalculated(ADay(3), ADay(4));

        Domain.Entities.MonthlyTransportSheet added = null!;
        _monthlyTransportSheetRepository
            .When(x => x.AddAsync(Arg.Any<Domain.Entities.MonthlyTransportSheet>()))
            .Do(x => added = x.Arg<Domain.Entities.MonthlyTransportSheet>());

        // Act
        var dto = await _handler.Handle(ACommand(), CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            added.CrewId.Should().Be(CrewId);
            added.Year.Should().Be(Year);
            added.Month.Should().Be(Month);
            added.RecipientId.Should().Be(LeadId);

            dto.CrewId.Should().Be(CrewId);
            dto.RecipientId.Should().Be(LeadId);
            dto.Days.Should().HaveCount(2);
        }

        _ = _unitOfWork.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task DraftSheet_ShouldBeRecalculatedInPlace()
    {
        // Arrange — the draft was produced before another day was confirmed.
        var existing = ASheet(id: 11, ADay(3));
        GivenExistingSheet(existing);
        GivenCalculated(ADay(3), ADay(4));

        // Act
        var dto = await _handler.Handle(ACommand(), CancellationToken.None);

        // Assert — same sheet row, freshly computed days, no second sheet inserted.
        using (new AssertionScope())
        {
            existing.Days.Should().HaveCount(2);
            existing.UpdatedAt.Should().NotBeNull();

            dto.Id.Should().Be(existing.Id);
            dto.Days.Should().HaveCount(2);
        }

        _ = _monthlyTransportSheetRepository
            .DidNotReceive()
            .AddAsync(Arg.Any<Domain.Entities.MonthlyTransportSheet>());

        _ = _unitOfWork.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task ConfirmedSheet_ShouldNotBeRecalculated()
    {
        // Arrange
        var existing = ASheet(id: 11, ADay(3));
        existing.IsConfirmed = true;
        GivenExistingSheet(existing);
        GivenCalculated(ADay(3));

        // Act
        var action = () => _handler.Handle(ACommand(), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(MonthlyTransportSheetErrors.AlreadyConfirmed);
    }

    [Fact]
    public async Task PeriodWithoutConfirmedDays_ShouldNotProduceAnEmptySheet()
    {
        // Arrange
        GivenExistingSheet(null);
        GivenCalculated();

        // Act
        var action = () => _handler.Handle(ACommand(), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(MonthlyTransportSheetErrors.NothingToReport);
    }
}
