using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Seeders.Interfaces;

namespace Transportation.Infrastructure.Persistence.Seeders.DemoData;

internal sealed class AbsenceNoticeDatabaseSeeder : IDemoDataSeeder
{
    private readonly TransportationDbContext _context;

    public AbsenceNoticeDatabaseSeeder(TransportationDbContext context)
    {
        _context = context;
    }

    public int Order => 7;

    public async Task SeedAsync()
    {
        var now = DateTime.UtcNow;

        var users = await _context.Users
            .ToDictionaryAsync(x => x.UserName!, x => x.Id);

        await CreateAbsenceIfNotExistsAsync(
            username: "worker.01",
            date: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            scope: AbsenceScope.Full,
            type: AbsenceType.Worker,
            reason: "Family event",
            isNotified: true,
            users,
            now);

        await CreateAbsenceIfNotExistsAsync(
            username: "worker.02",
            date: DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
            scope: AbsenceScope.Morning,
            type: AbsenceType.Worker,
            reason: "Doctor appointment",
            isNotified: true,
            users,
            now);

        await CreateAbsenceIfNotExistsAsync(
            username: "worker.03",
            date: DateOnly.FromDateTime(DateTime.Today.AddDays(3)),
            scope: AbsenceScope.Afternoon,
            type: AbsenceType.Worker,
            reason: "Personal issue",
            isNotified: false,
            users,
            now);

        await CreateAbsenceIfNotExistsAsync(
            username: "rustam.driver",
            date: DateOnly.FromDateTime(DateTime.Today.AddDays(4)),
            scope: AbsenceScope.Full,
            type: AbsenceType.Driver,
            reason: "Vehicle maintenance",
            isNotified: true,
            users,
            now);

        await CreateAbsenceIfNotExistsAsync(
            username: "diyor.driver",
            date: DateOnly.FromDateTime(DateTime.Today.AddDays(5)),
            scope: AbsenceScope.Morning,
            type: AbsenceType.Driver,
            reason: "Medical checkup",
            isNotified: true,
            users,
            now);

        await CreateAbsenceIfNotExistsAsync(
            username: "worker.04",
            date: DateOnly.FromDateTime(DateTime.Today.AddDays(6)),
            scope: AbsenceScope.Full,
            type: AbsenceType.Worker,
            reason: "Family emergency",
            isNotified: false,
            users,
            now);

        await CreateAbsenceIfNotExistsAsync(
            username: "worker.05",
            date: DateOnly.FromDateTime(DateTime.Today.AddDays(7)),
            scope: AbsenceScope.Afternoon,
            type: AbsenceType.Worker,
            reason: "Personal appointment",
            isNotified: true,
            users,
            now);

        await CreateAbsenceIfNotExistsAsync(
            username: "worker.06",
            date: DateOnly.FromDateTime(DateTime.Today.AddDays(8)),
            scope: AbsenceScope.Morning,
            type: AbsenceType.Worker,
            reason: "Transport issue",
            isNotified: true,
            users,
            now);

        await _context.SaveChangesAsync();
    }

    private async Task CreateAbsenceIfNotExistsAsync(
        string username,
        DateOnly date,
        AbsenceScope scope,
        AbsenceType type,
        string reason,
        bool isNotified,
        Dictionary<string, long> users,
        DateTime now)
    {
        if (!users.TryGetValue(username, out var userId))
            throw new InvalidOperationException($"User '{username}' not found. " +
                                                "UserDatabaseSeeder must run before AbsenceNoticeDatabaseSeeder.");

        var exists = await _context.AbsenceNotices
            .AnyAsync(x =>
                x.UserId == userId &&
                x.Date == date &&
                x.Scope == scope &&
                x.Type == type);
        
        if (exists)
            return;
        
        var absence = new AbsenceNotice
        {
            UserId = userId,
            Date = date,
            Scope = scope,
            Type = type,
            Reason = reason,
            IsNotified = isNotified,
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };
        
        await _context.AbsenceNotices.AddAsync(absence);
    }
}