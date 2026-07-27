using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;


namespace Transportation.Infrastructure.Persistence.Configurations;

internal sealed class CrewMemborshipConfiguration : IEntityTypeConfiguration<CrewMembership>
{
    public void Configure(EntityTypeBuilder<CrewMembership> builder)
    {
        builder
            .Property(x => x.CrewId)
            .IsRequired();
        
        builder
            .Property(x => x.UserId)
            .IsRequired();
        
        builder
            .Property(x => x.ActiveFrom)
            .IsRequired();
        
        builder.HasOne(x => x.Crew)
            .WithMany()
            .HasForeignKey(x => x.CrewId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}