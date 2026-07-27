using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Transportation.Domain.Entities;

namespace Transportation.Infrastructure.Persistence.Configurations;

public sealed class AbsenceNoticeConfiguration : IEntityTypeConfiguration<AbsenceNotice>
{
    public void Configure(EntityTypeBuilder<AbsenceNotice> builder)
    {
        builder.ToTable("absence_notices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Scope)
            .IsRequired();

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.IsNotified)
            .HasDefaultValue(false);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}