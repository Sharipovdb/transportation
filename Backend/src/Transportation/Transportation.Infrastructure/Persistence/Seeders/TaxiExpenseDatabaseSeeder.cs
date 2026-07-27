using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class TaxiExpenseDatabaseSeeder : IDatabaseSeeder
{
    private readonly TransportationDbContext _context;

    public TaxiExpenseDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.TaxiExpenses.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var transportDays = await _context.TransportDays
            .OrderBy(x => x.Id)
            .ToListAsync();

        if (transportDays.Count < 4)
            throw new InvalidOperationException("Run TransportDaySeeder first.");

        var users = await _context.Users
            .ToDictionaryAsync(x => x.UserName!, x => x.Id);

        long GetUser(string username)
        {
            if (!users.TryGetValue(username, out var id))
                throw new InvalidOperationException($"User {username} not found");

            return id;
        }

        var taxiExpenses = new List<TaxiExpense>
        {
            new()
            {
                TransportDayId = transportDays[1].Id,
                Leg = Leg.Morning,
                Amount = 45m,
                PaidById = GetUser("bekzod.manager"),
                TaxiExpenseStatus = TaxiExpenseStatus.Approved,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                TransportDayId = transportDays[1].Id,
                Leg = Leg.Afternoon,
                Amount = 60m,
                PaidById = GetUser("sardor.lead"),
                TaxiExpenseStatus = TaxiExpenseStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                TransportDayId = transportDays[2].Id,
                Leg = Leg.Afternoon,
                Amount = 80m,
                PaidById = GetUser("sardor.lead"),
                TaxiExpenseStatus = TaxiExpenseStatus.Approved,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                TransportDayId = transportDays[2].Id,
                Leg = Leg.Morning,
                Amount = 55m,
                PaidById = GetUser("bekzod.manager"),
                TaxiExpenseStatus = TaxiExpenseStatus.Paid,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                TransportDayId = transportDays[0].Id,
                Leg = Leg.Morning,
                Amount = 40m,
                PaidById = GetUser("bekzod.manager"),
                TaxiExpenseStatus = TaxiExpenseStatus.Rejected,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                TransportDayId = transportDays[0].Id,
                Leg = Leg.Afternoon,
                Amount = 75m,
                PaidById = GetUser("sardor.lead"),
                TaxiExpenseStatus = TaxiExpenseStatus.Approved,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                TransportDayId = transportDays[3].Id,
                Leg = Leg.Morning,
                Amount = 35m,
                PaidById = GetUser("javlon.lead"),
                TaxiExpenseStatus = TaxiExpenseStatus.Pending,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                TransportDayId = transportDays[3].Id,
                Leg = Leg.Afternoon,
                Amount = 90m,
                PaidById = GetUser("javlon.lead"),
                TaxiExpenseStatus = TaxiExpenseStatus.Approved,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                TransportDayId = transportDays[1].Id,
                Leg = Leg.Morning,
                Amount = 50m,
                PaidById = GetUser("bekzod.manager"),
                TaxiExpenseStatus = TaxiExpenseStatus.Paid,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                TransportDayId = transportDays[2].Id,
                Leg = Leg.Afternoon,
                Amount = 65m,
                PaidById = GetUser("sardor.lead"),
                TaxiExpenseStatus = TaxiExpenseStatus.Approved,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };

        await _context.TaxiExpenses.AddRangeAsync(taxiExpenses);

        await _context.SaveChangesAsync();
    }
}