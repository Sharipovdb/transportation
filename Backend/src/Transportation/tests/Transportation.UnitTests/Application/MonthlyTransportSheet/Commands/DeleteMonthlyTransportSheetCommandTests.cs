using AutoFixture;
using FluentAssertions;
using NSubstitute;
using Transportation.Application.MonthlyTransportSheet;
using Transportation.Application.MonthlyTransportSheet.Commands;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.MonthlyTransportSheet.Commands;

public class DeleteMonthlyTransportSheetCommandTests
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly DeleteMonthlyTransportSheetCommandHandler _handler;

    public DeleteMonthlyTransportSheetCommandTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        _monthlyTransportSheetRepository = fixture.Freeze<IMonthlyTransportSheetRepository>();
        _unitOfWork = fixture.Freeze<IUnitOfWork>();

        _handler = fixture.Create<DeleteMonthlyTransportSheetCommandHandler>();
    }

    [Fact]
    public async Task DeleteMonthlyTransportSheet_NotFound_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var fixture = new Fixture();

        var command = fixture.Create<DeleteMonthlyTransportSheetCommand>();
        _monthlyTransportSheetRepository
            .GetByIdAsync(Arg.Any<MonthlyTransportSheetByIdSpec>())
            .Returns((Domain.Entities.MonthlyTransportSheet?)null);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(MonthlyTransportSheetErrors.NotFound);
    }

    [Fact]
    public async Task DeleteMonthlyTransportSheet_ShouldMarkAsDeleted()
    {
        // Arrange
        var fixture = new Fixture();

        var command = fixture.Create<DeleteMonthlyTransportSheetCommand>();

        var entity = new Domain.Entities.MonthlyTransportSheet
        {
            Id = command.Id,
            CrewId = fixture.Create<long>(),
            Year = 2024,
            Month = 5,
            IsConfirmed = false,
            IsDeleted = false
        };

        _monthlyTransportSheetRepository
            .GetByIdAsync(Arg.Any<MonthlyTransportSheetByIdSpec>())
            .Returns(entity);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert — the sheet is soft-deleted in place; the tracked entity is saved as is.
        _ = _unitOfWork
            .Received()
            .SaveChangesAsync();

        result.Should().BeTrue();
        entity.IsDeleted.Should().BeTrue();
    }
}