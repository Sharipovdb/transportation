using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Application.TaxiExpense;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Models;
using Transportation.Application.TransportDay.Services;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.TransportDay.Services;

public class TransportDayTaxiFareServiceTests
{
    private const long CrewId = 3;
    private const long PayerId = 11;
    private const long OutsiderId = 99;

    private static readonly DateTime Now = new(2026, 8, 11, 9, 0, 0, DateTimeKind.Utc);

    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly TransportDayTaxiFareService _service;

    public TransportDayTaxiFareServiceTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        _crewMembershipRepository = fixture.Freeze<ICrewMembershipRepository>();

        _crewMembershipRepository
            .ListAsync(Arg.Any<CrewMembershipByCrewIdSpec>())
            .Returns([new Domain.Entities.CrewMembership { CrewId = CrewId, UserId = PayerId }]);

        _service = fixture.Create<TransportDayTaxiFareService>();
    }

    private static Domain.Entities.TransportDay ADay(
        TransportMode morningMode,
        TransportMode? afternoonMode,
        params Domain.Entities.TaxiExpense[] existingExpenses)
    {
        return new Domain.Entities.TransportDay
        {
            CrewId = CrewId,
            MorningMode = morningMode,
            AfternoonMode = afternoonMode,
            TaxiExpenses = existingExpenses.ToList()
        };
    }

    [Fact]
    public async Task TaxiLeg_ShouldProduceAPendingExpense()
    {
        var day = ADay(TransportMode.Driven, TransportMode.Taxi);

        await _service.SyncAsync(
            day,
            [new TransportDayTaxiFare(Leg.Afternoon, 25m, PayerId)],
            Now,
            CancellationToken.None);

        var expense = day.TaxiExpenses.Should().ContainSingle().Subject;
        expense.Leg.Should().Be(Leg.Afternoon);
        expense.Amount.Should().Be(25m);
        expense.PaidById.Should().Be(PayerId);
        expense.TaxiExpenseStatus.Should().Be(TaxiExpenseStatus.Pending);
        expense.CreatedAt.Should().Be(Now);
    }

    [Fact]
    public async Task TaxiLegWithoutFare_ShouldBeRejected()
    {
        var day = ADay(TransportMode.Taxi, TransportMode.None);

        var action = () => _service.SyncAsync(day, [], Now, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TransportDayErrors.TaxiLegWithoutFare);
    }

    [Fact]
    public async Task FareOnADrivenLeg_ShouldBeRejected()
    {
        var day = ADay(TransportMode.Driven, TransportMode.Driven);

        var action = () => _service.SyncAsync(
            day,
            [new TransportDayTaxiFare(Leg.Morning, 25m, PayerId)],
            Now,
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TransportDayErrors.TaxiFareOnNonTaxiLeg);
    }

    [Fact]
    public async Task PayerOutsideTheCrew_ShouldBeRejected()
    {
        var day = ADay(TransportMode.Taxi, TransportMode.None);

        var action = () => _service.SyncAsync(
            day,
            [new TransportDayTaxiFare(Leg.Morning, 25m, OutsiderId)],
            Now,
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TaxiExpenseErrors.ExpensePaidByNonExistentCrewMember);
    }

    [Fact]
    public async Task ExistingFare_ShouldBeUpdatedRatherThanDuplicated()
    {
        var day = ADay(
            TransportMode.Taxi,
            TransportMode.None,
            new Domain.Entities.TaxiExpense
            {
                Leg = Leg.Morning,
                Amount = 20m,
                PaidById = PayerId,
                TaxiExpenseStatus = TaxiExpenseStatus.Pending
            });

        await _service.SyncAsync(
            day,
            [new TransportDayTaxiFare(Leg.Morning, 32m, PayerId)],
            Now,
            CancellationToken.None);

        var expense = day.TaxiExpenses.Should().ContainSingle().Subject;
        expense.Amount.Should().Be(32m);
        expense.UpdatedAt.Should().Be(Now);
    }

    [Fact]
    public async Task LegNoLongerTakenByTaxi_ShouldDropItsExpense()
    {
        var day = ADay(
            TransportMode.Driven,
            TransportMode.None,
            new Domain.Entities.TaxiExpense
            {
                Leg = Leg.Morning,
                Amount = 20m,
                PaidById = PayerId,
                TaxiExpenseStatus = TaxiExpenseStatus.Pending
            });

        await _service.SyncAsync(day, [], Now, CancellationToken.None);

        day.TaxiExpenses.Should().OnlyContain(x => x.IsDeleted);
    }

    [Fact]
    public async Task ApprovedExpense_ShouldNotBeRewrittenByTheDailyLog()
    {
        var day = ADayWithAnApprovedMorningFare();

        var action = () => _service.SyncAsync(
            day,
            [new TransportDayTaxiFare(Leg.Morning, 32m, PayerId)],
            Now,
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TransportDayErrors.FareAlreadyRuledOn);
    }

    [Fact]
    public async Task ApprovedExpense_ShouldNotBeWithdrawnByTurningTheLegIntoADrivenOne()
    {
        // Dropping the taxi leg used to orphan its expense and soft-delete it without
        // ever reaching the per-fare check, silently withdrawing approved money.
        var day = ADayWithAnApprovedMorningFare();

        day.MorningMode = TransportMode.Driven;

        var action = () => _service.SyncAsync(day, [], Now, CancellationToken.None);

        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TransportDayErrors.FareAlreadyRuledOn);

        day.TaxiExpenses.Should().ContainSingle().Which.IsDeleted.Should().BeFalse();
    }

    private static Domain.Entities.TransportDay ADayWithAnApprovedMorningFare()
        => ADay(
            TransportMode.Taxi,
            TransportMode.None,
            new Domain.Entities.TaxiExpense
            {
                Leg = Leg.Morning,
                Amount = 20m,
                PaidById = PayerId,
                TaxiExpenseStatus = TaxiExpenseStatus.Approved
            });
}
