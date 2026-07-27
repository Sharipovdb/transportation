using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Transportation.Application.CrewMembership.Repositories;
using Transportation.Application.TaxiExpense;
using Transportation.Application.TaxiExpense.Commands;
using Transportation.Application.TaxiExpense.Repositories;
using Transportation.Application.TransportDay;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.TaxiExpense.Commands;

public class CreateTaxiExpenseCommandTests
{
    private readonly ITaxiExpenseRepository _taxiExpenseRepository;
    private readonly ICrewMembershipRepository _crewMemborshipRepository;
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly TaxiExpenseMapper _taxiExpenseMapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FakeTimeProvider _fakeTimeProvider = new();

    private readonly CreateTaxiExpenseCommandHandler _handler;

    public CreateTaxiExpenseCommandTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        fixture.Register<TimeProvider>(() => _fakeTimeProvider);

        _taxiExpenseRepository = fixture.Freeze<ITaxiExpenseRepository>();
        _crewMemborshipRepository = fixture.Freeze<ICrewMembershipRepository>();
        _transportDayRepository = fixture.Freeze<ITransportDayRepository>();
        _taxiExpenseMapper = fixture.Freeze<TaxiExpenseMapper>();
        _unitOfWork = fixture.Freeze<IUnitOfWork>();

        _handler = fixture.Create<CreateTaxiExpenseCommandHandler>();
    }

    [Fact]
    public async Task TransportationDayIsNull_ShouldThrow()
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<CreateTaxiExpenseCommand>();

        _transportDayRepository
            .GetByIdAsync(command.TransportDayId)
            .ReturnsNull();

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TransportDayErrors.NotFound);
    }

    [Fact]
    public async Task TransportationDay_IsDelete_true_ShouldThrow()
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<CreateTaxiExpenseCommand>();

        var transportationDay = fixture.Build<Domain.Entities.TransportDay>()
            .Without(x => x.Crew)
            .Without(x => x.TaxiExpenses)
            .With(x => x.IsDeleted, true)
            .Create();

        _transportDayRepository
            .GetByIdAsync(command.TransportDayId)
            .Returns(transportationDay);

        // Acr
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TransportDayErrors.NotFound);
    }

    [Fact]
    public async Task TransportationDay_Confirmed_IsTrue_ShouldThrow()
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<CreateTaxiExpenseCommand>();

        var transportationDay = fixture.Build<Domain.Entities.TransportDay>()
            .Without(x => x.Crew)
            .Without(x => x.TaxiExpenses)
            .With(x => x.IsDeleted, false)
            .With(x => x.Confirmed, true)
            .Create();

        _transportDayRepository
            .GetByIdAsync(command.TransportDayId)
            .Returns(transportationDay);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TaxiExpenseErrors.AlreadyExists);
    }

    [Fact]
    public async Task Handle_LegIsMorning_ShouldReturnMorningMode()
    {
        // Arrange
        var fixture = new Fixture();
        var command = fixture.Create<CreateTaxiExpenseCommand>();

        var transportationDay = fixture.Build<Domain.Entities.TransportDay>()
            .Without(x => x.Crew)
            .Without(x => x.TaxiExpenses)
            .With(x => x.MorningMode, TransportMode.Taxi)
            .With(x => x.AfternoonMode, TransportMode.Driven)
            .Create();

        _transportDayRepository
            .GetByIdAsync(command.TransportDayId)
            .Returns(transportationDay);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(TaxiExpenseErrors.InvalidEnumValue);
    }
}