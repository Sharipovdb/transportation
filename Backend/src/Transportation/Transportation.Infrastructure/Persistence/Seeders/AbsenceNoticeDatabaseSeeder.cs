using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders;

internal sealed class AbsenceNoticeDatabaseSeeder : IDatabaseSeeder
{
    private readonly TransportationDbContext _context;

    public AbsenceNoticeDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.AbsenceNotices.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var users = await _context.Users.ToDictionaryAsync(x => x.UserName!, x => x.Id);

        long GetUser(string username)
        {
            if (!users.TryGetValue(username, out var id))
                throw new Exception($"User '{username}' not found");

            return id;
        }

        var absenceNotices = new List<AbsenceNotice>
        {
            new()
            {
                UserId = GetUser("worker.01"),
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                Scope = AbsenceScope.Full,
                Type = AbsenceType.Worker,
                Reason = "Family event",
                IsNotified = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                UserId = GetUser("worker.02"),
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                Scope = AbsenceScope.Morning,
                Type = AbsenceType.Worker,
                Reason = "Doctor appointment",
                IsNotified = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                UserId = GetUser("worker.03"),
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
                Scope = AbsenceScope.Afternoon,
                Type = AbsenceType.Worker,
                Reason = "Personal issue",
                IsNotified = false,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                UserId = GetUser("rustam.driver"),
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(4)),
                Scope = AbsenceScope.Full,
                Type = AbsenceType.Driver,
                Reason = "Vehicle maintenance",
                IsNotified = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                UserId = GetUser("diyor.driver"),
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
                Scope = AbsenceScope.Morning,
                Type = AbsenceType.Driver,
                Reason = "Medical checkup",
                IsNotified = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                UserId = GetUser("worker.04"),
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(6)),
                Scope = AbsenceScope.Full,
                Type = AbsenceType.Worker,
                Reason = "Family emergency",
                IsNotified = false,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },

            new()
            {
                UserId = GetUser("worker.05"),
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
                Scope = AbsenceScope.Afternoon,
                Type = AbsenceType.Worker,
                Reason = "Personal appointment",
                IsNotified = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            },
            
            new()
            {
                UserId = GetUser("worker.06"),
                Date = DateOnly.FromDateTime(DateTime.Today.AddDays(8)),
                Scope = AbsenceScope.Morning,
                Type = AbsenceType.Worker,
                Reason = "Transport issue",
                IsNotified = true,
                CreatedAt = now,
                UpdatedAt = now,
                IsDeleted = false
            }
        };
        
        await _context.AbsenceNotices.AddRangeAsync(absenceNotices);

        await _context.SaveChangesAsync();
    }
}