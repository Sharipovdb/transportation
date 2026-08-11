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
using Transportation.Mediator.Helper.Exceptions;
using Transportation.Mediator.Helper.Persistence;
using Transportation.UnitTests.Utils;

namespace Transportation.UnitTests.Application.MonthlyTransportSheet.Commands;

public class GenerateMonthlyTransportSheetCommandTests
{
    private const long CrewId = 3;
    private const int Year = 2026;
    private const int Month = 8;

    private readonly IMonthlyTransportSheetRepository _monthlyTransportSheetRepository;
    private readonly IMonthlyTransportSheetGenerator _generator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FakeTimeProvider _fakeTimeProvider = new();

    private readonly GenerateMonthlyTransportSheetCommandHandler _handler;

    public GenerateMonthlyTransportSheetCommandTests()
    {
        var fixture = new Fixture().WithAutoNSubstitutions();

        fixture.Register<TimeProvider>(() => _fakeTimeProvider);

        _monthlyTransportSheetRepository = fixture.Freeze<IMonthlyTransportSheetRepository>();
        _generator = fixture.Freeze<IMonthlyTransportSheetGenerator>();
        _unitOfWork = fixture.Freeze<IUnitOfWork>();

        _handler = fixture.Create<GenerateMonthlyTransportSheetCommandHandler>();
    }

    private static GenerateMonthlyTransportSheetCommand ACommand()
        => new(CrewId, Year, Month);

    private static Domain.Entities.MonthlyTransportSheet ASheet(
        params Domain.Entities.PayoutLine[] payoutLines)
    {
        return new Domain.Entities.MonthlyTransportSheet
        {
            Id = 11,
            CrewId = CrewId,
            Year = Year,
            Month = Month,
            PayoutLines = payoutLines.ToList()
        };
    }

    private static Domain.Entities.PayoutLine APayoutLine(long userId, bool isPaid = false)
        => new()
        {
            UserId = userId,
            IsPaid = isPaid,
            User = new Domain.Entities.User { FirstName = "Test", LastName = $"Member {userId}" }
        };

    /// <summary>Freshly calculated result handed back by the generator.</summary>
    private void GivenCalculated(params Domain.Entities.PayoutLine[] payoutLines)
    {
        _generator
            .GenerateAsync(CrewId, Year, Month, Arg.Any<CancellationToken>())
            .Returns(ASheet(payoutLines));
    }

    private void GivenExistingSheet(Domain.Entities.MonthlyTransportSheet? sheet)
    {
        _monthlyTransportSheetRepository
            .FirstOrDefaultAsync(Arg.Any<MonthlyTransportSheetByPeriodSpec>())
            .Returns(sheet);

        if (sheet is not null)
        {
            _monthlyTransportSheetRepository
                .FirstOrDefaultAsync(Arg.Any<MonthlyTransportSheetWithPayoutsSpec>())
                .Returns(sheet);
        }
    }

    [Fact]
    public async Task NoSheetForThePeriod_ShouldCreateOne()
    {
        // Arrange
        GivenExistingSheet(null);
        GivenCalculated(APayoutLine(userId: 1));

        Domain.Entities.MonthlyTransportSheet added = null!;
        _monthlyTransportSheetRepository
            .When(x => x.AddAsync(Arg.Any<Domain.Entities.MonthlyTransportSheet>()))
            .Do(x =>
            {
                added = x.Arg<Domain.Entities.MonthlyTransportSheet>();

                // The handler re-reads the saved sheet before mapping it.
                _monthlyTransportSheetRepository
                    .FirstOrDefaultAsync(Arg.Any<MonthlyTransportSheetWithPayoutsSpec>())
                    .Returns(added);
            });

        // Act
        var dto = await _handler.Handle(ACommand(), CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            added.CrewId.Should().Be(CrewId);
            added.Year.Should().Be(Year);
            added.Month.Should().Be(Month);

            dto.CrewId.Should().Be(CrewId);
            dto.PayoutLines.Should().HaveCount(1);
        }

        _ = _unitOfWork.Received().SaveChangesAsync();
    }

    [Fact]
    public async Task DraftSheet_ShouldBeRecalculatedInPlace()
    {
        // Arrange — the draft was produced before another day was confirmed.
        var existing = ASheet(APayoutLine(userId: 1));
        GivenExistingSheet(existing);
        GivenCalculated(APayoutLine(userId: 1), APayoutLine(userId: 2));

        // Act
        var dto = await _handler.Handle(ACommand(), CancellationToken.None);

        // Assert — same sheet row, freshly computed lines, no second sheet inserted.
        using (new AssertionScope())
        {
            existing.PayoutLines.Should().HaveCount(2);
            existing.UpdatedAt.Should().NotBeNull();

            dto.Id.Should().Be(existing.Id);
            dto.PayoutLines.Should().HaveCount(2);
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
        var existing = ASheet(APayoutLine(userId: 1));
        existing.IsConfirmed = true;
        GivenExistingSheet(existing);
        GivenCalculated(APayoutLine(userId: 1));

        // Act
        var action = () => _handler.Handle(ACommand(), CancellationToken.None);

        // Assert
        var exception = await action.Should().ThrowAsync<BusinessLogicException>();
        exception.Which.Error.Should().Be(MonthlyTransportSheetErrors.AlreadyConfirmed);
    }

    [Fact]
    public async Task AlreadyPaidMember_ShouldKeepTheirLineWhileTheRestIsRefreshed()
    {
        // Arrange — member 1 has been settled in person; member 2 has not.
        var paidLine = APayoutLine(userId: 1, isPaid: true);
        var existing = ASheet(paidLine, APayoutLine(userId: 2));
        GivenExistingSheet(existing);
        GivenCalculated(APayoutLine(userId: 1), APayoutLine(userId: 2), APayoutLine(userId: 3));

        // Act
        await _handler.Handle(ACommand(), CancellationToken.None);

        // Assert — the paid line survives untouched and is not duplicated by the
        // freshly calculated one for the same member.
        using (new AssertionScope())
        {
            existing.PayoutLines.Should().HaveCount(3);
            existing.PayoutLines.Should().ContainSingle(x => x.UserId == 1)
                .Which.Should().BeSameAs(paidLine);
            existing.PayoutLines.Select(x => x.UserId).Should().BeEquivalentTo([1L, 2L, 3L]);
        }
    }
}
