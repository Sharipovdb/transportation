using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;
using Transportation.Application.MonthlyTransportSheet;
using Transportation.Application.MonthlyTransportSheet.Commands;
using Transportation.Application.MonthlyTransportSheet.Repositories;
using Transportation.Application.MonthlyTransportSheet.Specifications;
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.MonthlyTransportSheet.Commands;

public class GenerateMonthlyTransportSheetCommandTests
{
    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FakeTimeProvider _fakeTimeProvider = new();

    private readonly GenerateMonthlyTransportSheetCommandHandler _handler;

    public GenerateMonthlyTransportSheetCommandTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        fixture.Register<TimeProvider>(() => _fakeTimeProvider);

        _monthlyTransportSheetRepository = fixture.Freeze<IMonthlyTransportSheetRepository>();
        _unitOfWork = fixture.Freeze<IUnitOfWork>();

        _handler = fixture.Create<GenerateMonthlyTransportSheetCommandHandler>();
    }

    [Fact]
    public async Task MonthlyTransportSheetAlreadyExists_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var fixture = new Fixture();

        var command = fixture.Create<GenerateMonthlyTransportSheetCommand>();

        _monthlyTransportSheetRepository
            .AnyAsync(Arg.Any<MonthlyTransportSheetByPeriodSpec>())
            .Returns(true);

        // Act
        var action = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(MonthlyTransportSheetErrors.AlreadyExists);
    }

    [Fact]
    public async Task CreateMonthlyTransportSheet_ShouldBeSuccess()
    {
        // Arrange
        var fixture = new Fixture();

        var command = fixture.Create<GenerateMonthlyTransportSheetCommand>();

        _monthlyTransportSheetRepository
            .AnyAsync(Arg.Any<MonthlyTransportSheetByPeriodSpec>())
            .Returns(false);

        Domain.Entities.MonthlyTransportSheet entity = null!;

        _monthlyTransportSheetRepository
            .When(x => x.AddAsync(Arg.Any<Domain.Entities.MonthlyTransportSheet>()))
            .Do(x => entity = x.Arg<Domain.Entities.MonthlyTransportSheet>());

        var dateTimeOffsetNow = DateTimeOffset.Now;
        var dateTimeNow = DateTime.SpecifyKind(dateTimeOffsetNow.UtcDateTime, DateTimeKind.Utc);
        _fakeTimeProvider.SetUtcNow(dateTimeOffsetNow);

        // Act
        var dto = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _ = _monthlyTransportSheetRepository
            .Received()
            .AnyAsync(
                Arg.Is<MonthlyTransportSheetByPeriodSpec>(x =>
                    x.CrewId == command.CrewId &&
                    x.Year == command.Year &&
                    x.Month == command.Month)
            );

        _ = _monthlyTransportSheetRepository
            .Received()
            .AddAsync(Arg.Any<Domain.Entities.MonthlyTransportSheet>());

        _ = _unitOfWork
            .Received()
            .SaveChangesAsync();

        using (new AssertionScope())
        {
            entity.CrewId.Should().Be(command.CrewId);
            entity.Year.Should().Be(command.Year);
            entity.Month.Should().Be(command.Month);
            entity.IsConfirmed.Should().BeFalse();
            entity.CreatedAt.Should().Be(dateTimeNow);

            dto.Should().BeEquivalentTo(entity, options => options.ExcludingMissingMembers());
        }
    }
}