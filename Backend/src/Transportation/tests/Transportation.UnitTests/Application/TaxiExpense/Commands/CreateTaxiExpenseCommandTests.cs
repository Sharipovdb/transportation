using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.CrewMembership.Specification;
using Transportation.Application.TaxiExpense;
using Transportation.Application.TaxiExpense.Commands;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Application.TaxiExpense.Specifications;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.TaxiExpense.Commands;

public class CreateTaxiExpenseCommandTests
{
    private const long TransportDayId = 4;
    private const long CrewId = 9;
    private const long PayerId = 21;

    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly ICrewMembershipRepository _crewMembershipRepository;
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FakeTimeProvider _fakeTimeProvider = new();

    private readonly CreateTaxiExpenseCommandHandler _handler;

    public CreateTaxiExpenseCommandTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        fixture.Register<TimeProvider>(() => _fakeTimeProvider);

        _taxiExpenseRepository = fixture.Freeze<ITaxiExpenseRepository>();
        _crewMembershipRepository = fixture.Freeze<ICrewMembershipRepository>();
        _transportDayRepository = fixture.Freeze<ITransportDayRepository>();
        fixture.Freeze<TaxiExpenseMapper>();
        _unitOfWork = fixture.Freeze<IUnitOfWork>();

        _crewMembershipRepository
            .ListAsync(Arg.Any<CrewMembershipByCrewIdSpec>())
            .Returns([new Domain.Entities.CrewMembership { CrewId = CrewId, UserId = PayerId }]);

        _handler = fixture.Create<CreateTaxiExpenseCommandHandler>();
    }

    private static CreateTaxiExpenseCommand ACommand(Leg leg = Leg.Morning, long payerId = PayerId)
        => new(TransportDayId, leg, 30m, payerId);

    private void GivenTransportDay(
        TransportMode morningMode = TransportMode.Taxi,
        TransportMode afternoonMode = TransportMode.Driven,
        bool confirmed = false)
    {
        _transportDayRepository
            .FirstOrDefaultAsync(Arg.Any<TransportDayByIdSpec>())
            .Returns(new Domain.Entities.TransportDay
            {
                Id = TransportDayId,
                CrewId = CrewId,
                MorningMode = morningMode,
                AfternoonMode = afternoonMode,
                Confirmed = confirmed
            });
    }

    [Fact]
    public async Task TransportationDayIsNull_ShouldThrow()
    {
        // Arrange — the lookup spec also filters out deleted days, so both cases land here.
        _transportDayRepository
            .FirstOrDefaultAsync(Arg.Any<TransportDayByIdSpec>())
            .ReturnsNull();

        // Act
        var action = () => _handler.Handle(ACommand(), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<ResourceNotFoundException>();
        exception.Which.Error.Should().Be(TransportDayErrors.NotFound);
    }

    [Fact]
    public async Task TransportationDay_Confirmed_IsTrue_ShouldThrow()
    {
        // Arrange
        GivenTransportDay(confirmed: true);

        // Act
        var action = () => _handler.Handle(ACommand(), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TransportDayErrors.AlreadyConfirmed);
    }

    [Fact]
    public async Task LegNotTravelledByTaxi_ShouldThrow()
    {
        // Arrange — the afternoon was driven, so no fare can be claimed for it.
        GivenTransportDay(morningMode: TransportMode.Taxi, afternoonMode: TransportMode.Driven);

        // Act
        var action = () => _handler.Handle(ACommand(Leg.Afternoon), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TransportDayErrors.TaxiFareOnNonTaxiLeg);
    }

    [Fact]
    public async Task LegAlreadyHasAnExpense_ShouldThrow()
    {
        // Arrange
        GivenTransportDay();

        _taxiExpenseRepository
            .AnyAsync(Arg.Any<TaxiExpenseByTransportDayIdSpec>())
            .Returns(true);

        // Act
        var action = () => _handler.Handle(ACommand(), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TaxiExpenseErrors.ExpenseThisLegAlreadyExist);
    }

    [Fact]
    public async Task PayerOutsideTheCrew_ShouldThrow()
    {
        // Arrange
        GivenTransportDay();

        // Act
        var action = () => _handler.Handle(ACommand(payerId: 999), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TaxiExpenseErrors.ExpensePaidByNonExistentCrewMember);
    }

    [Fact]
    public async Task TaxiLeg_ShouldAddAPendingExpense()
    {
        // Arrange
        GivenTransportDay();

        Domain.Entities.TaxiExpense entity = null!;
        _taxiExpenseRepository
            .When(x => x.AddAsync(Arg.Any<Domain.Entities.TaxiExpense>()))
            .Do(x => entity = x.Arg<Domain.Entities.TaxiExpense>());

        // Act
        await _handler.Handle(ACommand(), CancellationToken.None);

        // Assert
        _ = _unitOfWork.Received().SaveChangesAsync();

        entity.TransportDayId.Should().Be(TransportDayId);
        entity.Leg.Should().Be(Leg.Morning);
        entity.Amount.Should().Be(30m);
        entity.PaidById.Should().Be(PayerId);
        entity.TaxiExpenseStatus.Should().Be(TaxiExpenseStatus.Pending);
    }
}
