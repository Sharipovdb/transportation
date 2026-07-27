using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Configurations;

internal sealed class CrewConfigaration : IEntityTypeConfiguration<Crew>
{
    public void Configure(EntityTypeBuilder<Crew> builder)
    {
        builder
            .Property(x => x.Name)
            .HasMaxLength(30);
        
        builder
            .HasIndex(x => x.Name)
            .IsUnique();
        
        builder
            .Property(x => x.SeatCapacity)
            .HasMaxLength(5)
            .IsRequired();
        
        builder
            .Property(x => x.RouteId)
            .IsRequired();
        
        builder.HasOne(x => x.Route)
            .WithMany()
            .HasForeignKey(x => x.RouteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CrewLead)
            .WithMany()
            .HasForeignKey(x => x.CrewLeadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}