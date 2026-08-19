using Transportation.Application.Crew;
using Transportation.Application.Crew.Repositories;
using Transportation.Application.Crew.Specification;
using Transportation.Application.TransportDay.Repositories;
using Transportation.Application.TransportDay.Specifications;
using Transportation.Domain.Entities;
using Transportation.Mediator.Helper.Exceptions;

namespace Transportation.Application.MonthlyTransportSheet.Services;

public interface IMonthlyTransportSheetBuilder
{
    /// <summary>
    /// Computes a crew's month from its confirmed transport days. The result is an
    /// unsaved entity: previewing maps it straight to a DTO, generating persists it.
    /// </summary>
    Task<Domain.Entities.MonthlyTransportSheet> BuildAsync(
        long crewId,
        int year,
        int month,
        CancellationToken cancellationToken);
}

/// <summary>
/// The single place a crew's month is calculated.
///
/// A working day is one row. Legs the crew covered in its own car produce distance —
/// both legs driven is a round trip — and legs taken by taxi produce money, never both
/// for the same leg. The whole month is owed to one person, the crew's lead, who
/// distributes it inside the team; that is why nothing here is split per member and why
/// who fronted a fare is not carried onto the sheet.
/// </summary>
internal sealed class MonthlyTransportSheetBuilder : IMonthlyTransportSheetBuilder
{
    private readonly ITransportDayRepository _transportDayRepository;
    private readonly ICrewRepository _crewRepository;

    public MonthlyTransportSheetBuilder(
        ITransportDayRepository transportDayRepository,
        ICrewRepository crewRepository)
    {
        _transportDayRepository = transportDayRepository;
        _crewRepository = crewRepository;
    }

    public async Task<Domain.Entities.MonthlyTransportSheet> BuildAsync(
        long crewId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var crew = await _crewRepository
            .FirstOrDefaultAsync(new CrewByIdSpec(crewId), cancellationToken);

        if (crew is null)
            throw new ResourceNotFoundException(CrewErrors.NotFound);

        // A driver-lead drives the crew, a manager-lead arranges taxis for it. Either
        // way there is exactly one lead, and the month is settled with them.
        var recipient = crew.DriverLead ?? crew.CrewLead;

        if (recipient is null)
            throw new BusinessLogicException(MonthlyTransportSheetErrors.CrewHasNoLead);

        var transportDays = await _transportDayRepository
            .ListAsync(new TransportDaysByPeriodSpec(crewId, year, month), cancellationToken);

        return new Domain.Entities.MonthlyTransportSheet
        {
            CrewId = crew.Id,
            Crew = crew,
            Year = year,
            Month = month,
            RecipientId = recipient.Id,
            Recipient = recipient,
            Days = transportDays
                .Select(ToSheetDay)
                // A day the crew neither drove nor paid for says nothing on the report,
                // so it stays off the sheet instead of printing as an empty row.
                .Where(day => day.DrivenKm > 0 || day.ExtraBusinessKm > 0 || day.TaxiAmount > 0)
                .OrderBy(day => day.Date)
                .ToList()
        };
    }

    private static MonthlyTransportSheetDay ToSheetDay(Domain.Entities.TransportDay transportDay)
    {
        return new MonthlyTransportSheetDay
        {
            TransportDayId = transportDay.Id,
            Date = transportDay.Date,
            DrivenKm = transportDay.DrivenCommuteKm,
            ExtraBusinessKm = transportDay.ExtraBusinessKm,
            TaxiAmount = transportDay.TaxiExpenses
                .Where(x => !x.IsDeleted && x.TaxiExpenseStatus == TaxiExpenseStatus.Approved)
                .Sum(x => x.Amount)
        };
    }
}
