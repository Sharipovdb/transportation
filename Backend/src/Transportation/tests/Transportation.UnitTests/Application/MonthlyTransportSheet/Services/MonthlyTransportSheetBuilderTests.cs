using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.MonthlyTransportSheet;
using Transportation.Application.MonthlyTransportSheet.Services;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.MonthlyTransportSheet.Services;

/// <summary>
/// The calculation every screen and the printed report ultimately read, so the rules it
/// encodes are asserted here rather than re-derived anywhere else.
/// </summary>
public class MonthlyTransportSheetBuilderTests
{
    private const long CrewId = 3;
    private const long DriverLeadId = 7;
    private const long ManagerLeadId = 8;
    private const long PassengerId = 9;
    private const int Year = 2026;
    private const int Month = 8;
    private const double RouteKm = 20;

    private readonly ITransportDayRepository _transportDayRepository;
    private readonly ICrewRepository _crewRepository;
    private readonly MonthlyTransportSheetBuilder _builder;

    public MonthlyTransportSheetBuilderTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        _transportDayRepository = fixture.Freeze<ITransportDayRepository>();
        _crewRepository = fixture.Freeze<ICrewRepository>();

        _builder = fixture.Create<MonthlyTransportSheetBuilder>();

        GivenCrewLedBy(driverLeadId: DriverLeadId);
        GivenDays();
    }

    private void GivenCrewLedBy(long? driverLeadId = null, long? managerLeadId = null)
    {
        var crew = new Domain.Entities.Crew
        {
            Id = CrewId,
            Name = "Crew A",
            DriverLeadId = driverLeadId,
            DriverLead = ALead(driverLeadId),
            CrewLeadId = managerLeadId,
            CrewLead = ALead(managerLeadId)
        };

        _crewRepository
            .FirstOrDefaultAsync(Arg.Any<CrewByIdSpec>())
            .Returns(crew);
    }

    private static User? ALead(long? userId)
        => userId is null
            ? null
            : new User { Id = userId.Value, FirstName = "Lead", LastName = $"{userId}" };

    private void GivenDays(params Domain.Entities.TransportDay[] days)
    {
        _transportDayRepository
            .ListAsync(Arg.Any<TransportDaysByPeriodSpec>())
            .Returns(days.ToList());
    }

    private static Domain.Entities.TransportDay ADay(
        int dayOfMonth,
        TransportMode morning = TransportMode.Driven,
        TransportMode? afternoon = TransportMode.Driven,
        double extraBusinessKm = 0,
        params Domain.Entities.TaxiExpense[] taxiExpenses)
    {
        return new Domain.Entities.TransportDay
        {
            Id = dayOfMonth,
            CrewId = CrewId,
            Date = new DateOnly(Year, Month, dayOfMonth),
            MorningMode = morning,
            AfternoonMode = afternoon,
            BaseRouteKm = RouteKm,
            ExtraBusinessKm = extraBusinessKm,
            TaxiExpenses = taxiExpenses.ToList()
        };
    }

    private static Domain.Entities.TaxiExpense AFare(
        decimal amount,
        Leg leg = Leg.Morning,
        long paidById = PassengerId,
        TaxiExpenseStatus status = TaxiExpenseStatus.Approved)
    {
        return new Domain.Entities.TaxiExpense
        {
            Leg = leg,
            Amount = amount,
            PaidById = paidById,
            TaxiExpenseStatus = status
        };
    }

    private Task<Domain.Entities.MonthlyTransportSheet> Build()
        => _builder.BuildAsync(CrewId, Year, Month, CancellationToken.None);

    [Fact]
    public async Task DayDrivenBothLegs_ShouldCountTheRoundTrip()
    {
        GivenDays(ADay(3));

        var sheet = await Build();

        sheet.Days.Should().ContainSingle()
            .Which.DrivenKm.Should().Be(RouteKm * 2);
    }

    [Fact]
    public async Task DayDrivenOneLegOnly_ShouldCountThatLeg()
    {
        GivenDays(ADay(3, afternoon: TransportMode.None));

        var sheet = await Build();

        sheet.Days.Should().ContainSingle()
            .Which.DrivenKm.Should().Be(RouteKm);
    }

    [Fact]
    public async Task TaxiLeg_ShouldProduceMoneyAndNoDistance()
    {
        GivenDays(ADay(
            3,
            morning: TransportMode.Taxi,
            afternoon: TransportMode.Taxi,
            taxiExpenses: [AFare(30), AFare(25, Leg.Afternoon)]));

        var sheet = await Build();

        using (new AssertionScope())
        {
            sheet.Days.Should().ContainSingle();
            sheet.Days[0].DrivenKm.Should().Be(0);
            sheet.Days[0].TaxiAmount.Should().Be(55);
        }
    }

    [Fact]
    public async Task MixedDay_ShouldReportTheDrivenLegAndTheTaxiFare()
    {
        GivenDays(ADay(
            3,
            morning: TransportMode.Driven,
            afternoon: TransportMode.Taxi,
            taxiExpenses: [AFare(35, Leg.Afternoon)]));

        var sheet = await Build();

        sheet.Days.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { DrivenKm = RouteKm, TaxiAmount = 35m });
    }

    [Theory]
    [InlineData(TaxiExpenseStatus.Pending)]
    [InlineData(TaxiExpenseStatus.Rejected)]
    public async Task FareThatIsNotApproved_ShouldNotBeOwed(TaxiExpenseStatus status)
    {
        GivenDays(ADay(
            3,
            morning: TransportMode.Taxi,
            afternoon: TransportMode.None,
            taxiExpenses: [AFare(30, status: status)]));

        var sheet = await Build();

        sheet.Days.Should().BeEmpty();
    }

    [Fact]
    public async Task FareFrontedByAnyMember_ShouldCountTowardsTheCrewsMonth()
    {
        // Who paid is a matter for the lead's own distribution, not for the sheet: the
        // company hands the whole amount to the lead either way.
        GivenDays(ADay(
            3,
            morning: TransportMode.Taxi,
            afternoon: TransportMode.None,
            taxiExpenses: [AFare(40, paidById: PassengerId)]));

        var sheet = await Build();

        using (new AssertionScope())
        {
            sheet.RecipientId.Should().Be(DriverLeadId);
            sheet.TotalTaxiAmount.Should().Be(40);
        }
    }

    [Fact]
    public async Task DayWithoutTravel_ShouldNotReachTheSheet()
    {
        GivenDays(ADay(3, morning: TransportMode.None, afternoon: TransportMode.None));

        var sheet = await Build();

        sheet.Days.Should().BeEmpty();
    }

    [Fact]
    public async Task ExtraBusinessKm_ShouldBeReportedSeparatelyFromTheCommute()
    {
        GivenDays(ADay(3, extraBusinessKm: 12));

        var sheet = await Build();

        using (new AssertionScope())
        {
            sheet.TotalDrivenKm.Should().Be(RouteKm * 2);
            sheet.TotalExtraBusinessKm.Should().Be(12);
        }
    }

    [Fact]
    public async Task Days_ShouldBeOrderedByDateSoTheReportReadsAsACalendar()
    {
        GivenDays(ADay(20), ADay(4), ADay(11));

        var sheet = await Build();

        sheet.Days.Select(x => x.Date.Day).Should().ContainInOrder(4, 11, 20);
    }

    [Fact]
    public async Task ManagerLedCrew_ShouldStillBeSettledWithItsLead()
    {
        // A crew with no driver-lead used to lose its kilometres entirely, because the
        // day had no driver to credit them to.
        GivenCrewLedBy(managerLeadId: ManagerLeadId);
        GivenDays(ADay(3));

        var sheet = await Build();

        using (new AssertionScope())
        {
            sheet.RecipientId.Should().Be(ManagerLeadId);
            sheet.TotalDrivenKm.Should().Be(RouteKm * 2);
        }
    }

    [Fact]
    public async Task CrewWithoutALead_ShouldBeRefused()
    {
        GivenCrewLedBy();
        GivenDays(ADay(3));

        var action = Build;

        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(MonthlyTransportSheetErrors.CrewHasNoLead);
    }
}
