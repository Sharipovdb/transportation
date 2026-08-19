using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Transportation.Domain.Entities;
using Transportation.Infrastructure.Persistence.Internal;
using Transportation.Mediator.Helper.Persistence;

namespace Transportation.Infrastructure.Persistence;

public sealed class TransportationDbContext : IdentityDbContext<User, IdentityRole<long>, long>, IUnitOfWork
{
    public TransportationDbContext(DbContextOptions<TransportationDbContext> options) : base(options)
    {
    }

    public DbSet<Route> Routes { get; set; }
    public DbSet<Crew> Crews { get; set; }
    public DbSet<CrewMembership> CrewMemberships { get; set; }
    public DbSet<AbsenceNotice> AbsenceNotices { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<TaxiExpense> TaxiExpenses { get; set; }
    public DbSet<TransportDay> TransportDays { get; set; }
    public DbSet<MonthlyTransportSheet> MonthlyTransportSheets { get; set; }
    public DbSet<MonthlyTransportSheetDay> MonthlyTransportSheetDays { get; set; }
    public DbSet<TransportSettings> TransportSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("transportation");
        modelBuilder.ApplyConfigurationsFromAssembly(TransportationInfrastructureRef.Assembly);
        
        Console.WriteLine(modelBuilder.Model.ToDebugString());
    }

    /// <inheritdoc cref="IUnitOfWork.SaveChangesAsync"/>
    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        return await SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc cref="IUnitOfWork.BeginTransactionAsync(CancellationToken)"/>
    async Task<ITransaction> IUnitOfWork.BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await Database.BeginTransactionAsync(cancellationToken)
            .ConfigureAwait(false);

        return new EfCoreTransactionProxy(transaction);
    }
}