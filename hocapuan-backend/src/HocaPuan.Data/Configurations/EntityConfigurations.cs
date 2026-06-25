using HocaPuan.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HocaPuan.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();
        builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Username).HasMaxLength(50).IsRequired();
        builder.Property(u => u.PasswordHash).IsRequired();
    }
}

public class UniversityConfiguration : IEntityTypeConfiguration<University>
{
    public void Configure(EntityTypeBuilder<University> builder)
    {
        builder.HasIndex(u => u.Name).IsUnique();
        builder.Property(u => u.Name).HasMaxLength(200).IsRequired();
        builder.Property(u => u.ShortName).HasMaxLength(20).IsRequired();
        builder.Property(u => u.City).HasMaxLength(100).IsRequired();
    }
}

public class ProfessorConfiguration : IEntityTypeConfiguration<Professor>
{
    public void Configure(EntityTypeBuilder<Professor> builder)
    {
        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Title).HasMaxLength(50).IsRequired();
        builder.HasIndex(p => new { p.FirstName, p.LastName, p.UniversityId, p.DepartmentId });
        builder.HasIndex(p => p.FirstName);
        builder.HasIndex(p => p.LastName);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasIndex(d => d.Name);
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.Property(r => r.Comment).HasMaxLength(2000).IsRequired();
        builder.Property(r => r.TagsJson).HasDefaultValue("[]");

        // Bir kullanıcı aynı hocaya birden fazla aktif yorum bırakabilmeli ama
        // spam önlemi için servis katmanında kontrol edilecek
        builder.HasIndex(r => new { r.ProfessorId, r.UserId });

        builder.HasOne(r => r.User)
               .WithMany(u => u.Reviews)
               .HasForeignKey(r => r.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReviewVoteConfiguration : IEntityTypeConfiguration<ReviewVote>
{
    public void Configure(EntityTypeBuilder<ReviewVote> builder)
    {
        // Bir kullanıcı bir yoruma sadece bir kez oy verebilir
        builder.HasIndex(v => new { v.ReviewId, v.UserId }).IsUnique();

        builder.HasOne(v => v.User)
               .WithMany(u => u.ReviewVotes)
               .HasForeignKey(v => v.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReviewReportConfiguration : IEntityTypeConfiguration<ReviewReport>
{
    public void Configure(EntityTypeBuilder<ReviewReport> builder)
    {
        builder.HasIndex(r => new { r.ReviewId, r.ReporterUserId }).IsUnique();

        builder.HasOne(r => r.Review)
               .WithMany(rv => rv.Reports)
               .HasForeignKey(r => r.ReviewId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Reporter)
               .WithMany()
               .HasForeignKey(r => r.ReporterUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ReviewFreshnessVoteConfiguration : IEntityTypeConfiguration<ReviewFreshnessVote>
{
    public void Configure(EntityTypeBuilder<ReviewFreshnessVote> builder)
    {
        // Bir kullanıcı bir yoruma sadece bir kez güncellik oyu verebilir
        builder.HasIndex(v => new { v.ReviewId, v.VoterUserId }).IsUnique();

        builder.HasOne(v => v.Review)
               .WithMany(r => r.FreshnessVotes)
               .HasForeignKey(v => v.ReviewId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.Voter)
               .WithMany()
               .HasForeignKey(v => v.VoterUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
