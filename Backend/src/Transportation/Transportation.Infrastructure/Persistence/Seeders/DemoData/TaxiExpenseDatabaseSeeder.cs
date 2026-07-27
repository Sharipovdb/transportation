using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class TaxiExpenseDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;

    public TaxiExpenseDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public int Order => 9;

    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;

        var transportDays = await _context.TransportDays
            .Include(x => x.Crew)
            .ToListAsync();

        if (!transportDays.Any())
            throw new InvalidOperationException("TransportDays are empty. Run TransportDaySeeder first.");

        var users = await _context.Users
            .ToDictionaryAsync(x => x.UserName!, x => x.Id);

        var today = DateTime.UtcNow.Date;

        var expenses = new[]
        {
            new
            {
                Crew = "Crew A",
                Date = today.AddDays(1),
                Leg = Leg.Morning,
                Amount = 45m,
                PaidBy = "bekzod.manager",
                Status = TaxiExpenseStatus.Approved
            },
            new
            {
                Crew = "Crew A",
                Date = today.AddDays(1),
                Leg = Leg.Afternoon,
                Amount = 60m,
                PaidBy = "sardor.lead",
                Status = TaxiExpenseStatus.Pending
            },
            new
            {
                Crew = "Crew B",
                Date = today,
                Leg = Leg.Morning,
                Amount = 55m,
                PaidBy = "bekzod.manager",
                Status = TaxiExpenseStatus.Paid
            },
            new
            {
                Crew = "Crew B",
                Date = today,
                Leg = Leg.Afternoon,
                Amount = 80m,
                PaidBy = "sardor.lead",
                Status = TaxiExpenseStatus.Approved
            },
            new
            {
                Crew = "Crew A",
                Date = today,
                Leg = Leg.Morning,
                Amount = 40m,
                PaidBy = "bekzod.manager",
                Status = TaxiExpenseStatus.Rejected
            },
            new
            {
                Crew = "Crew A",
                Date = today,
                Leg = Leg.Afternoon,
                Amount = 75m,
                PaidBy = "sardor.lead",
                Status = TaxiExpenseStatus.Approved
            },
            new
            {
                Crew = "Crew C",
                Date = today.AddDays(2),
                Leg = Leg.Morning,
                Amount = 35m,
                PaidBy = "javlon.lead",
                Status = TaxiExpenseStatus.Pending
            },
            new
            {
                Crew = "Crew C",
                Date = today.AddDays(2),
                Leg = Leg.Afternoon,
                Amount = 90m,
                PaidBy = "javlon.lead",
                Status = TaxiExpenseStatus.Approved
            }
        };

        foreach (var item in expenses)
        {
            await CreateTaxiExpenseIfNotExistsAsync(
                item.Crew,
                item.Date,
                item.Leg,
                item.Amount,
                item.PaidBy,
                item.Status,
                transportDays,
                users,
                now
            );
        }

        await _context.SaveChangesAsync();
    }

    private async Task CreateTaxiExpenseIfNotExistsAsync(
        string crewName,
        DateTime date,
        Leg leg,
        decimal amount,
        string paidByUsername,
        TaxiExpenseStatus status,
        List<TransportDay> transportDays,
        Dictionary<string, long> users,
        DateTime now)
    {
        var transportDay = transportDays
            .FirstOrDefault(x => x.Crew.Name == crewName && x.Date.Date == date.Date);

        if (transportDay is null)
            throw new InvalidOperationException($"TransportDay not found: {crewName} {date:d}");
        
        if (!users.TryGetValue(paidByUsername, out var paidById))
            throw new InvalidOperationException($"User not found: {paidByUsername}");

        var exists = await _context.TaxiExpenses
            .AnyAsync(x => x.TransportDayId == transportDay.Id && x.Leg == leg && x.PaidById == paidById);

        if (exists)
            return;

        var expense = new TaxiExpense
        {
            TransportDayId = transportDay.Id,
            Leg = leg,
            Amount = amount,
            PaidById = paidById,
            TaxiExpenseStatus = status,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        await _context.TaxiExpenses.AddAsync(expense);
    }
}