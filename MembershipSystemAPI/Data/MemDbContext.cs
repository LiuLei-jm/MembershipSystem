using Namotion.Reflection;

namespace MembershipSystemAPI.Data;

public class MemDbContext : DbContext
{
    public MemDbContext(DbContextOptions<MemDbContext> options) : base(options)
    {
    }

    protected MemDbContext()
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<ApiKey> ApiKeys { get; set; } = null!;
    public DbSet<MembershipCard> MembershipCards { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
                    .HasIndex(u => u.Username)
                    .IsUnique();

        modelBuilder.Entity<User>()
                    .Property(u => u.Id)
                    .HasMaxLength(36)
                    .HasConversion(
            v => v.ToString(),
            v => Guid.Parse(v));

        modelBuilder.Entity<User>()
                    .Property(u => u.Username)
                    .HasMaxLength(100);

        modelBuilder.Entity<User>()
                    .Property(u => u.PasswordHash)
                    .HasMaxLength(256);


        modelBuilder.Entity<User>()
                    .Property(u => u.Role)
                    .HasMaxLength(50);

        modelBuilder.Entity<User>()
                    .Property(u => u.CreatedAt)
                    .HasMaxLength(50);

        modelBuilder.Entity<User>()
            .HasOne(u => u.ApiKey)
            .WithOne(a => a.User)
            .HasForeignKey<ApiKey>(a => a.UserId);

        modelBuilder.Entity<ApiKey>()
            .Property(a => a.Id)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v));

        modelBuilder.Entity<ApiKey>()
            .Property(a => a.Key)
            .HasMaxLength(512);

        modelBuilder.Entity<ApiKey>()
            .Property(a => a.CreatedAt)
            .HasMaxLength(50);

        modelBuilder.Entity<ApiKey>()
            .Property(a => a.UserId)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v));

        modelBuilder.Entity<MembershipCard>()
            .Property(mc => mc.Id)
            .HasConversion(
                v => v.ToString(),
                v => Guid.Parse(v));

        modelBuilder.Entity<MembershipCard>()
            .Property(mc => mc.MembershipName)
            .HasMaxLength(100);

        modelBuilder.Entity<MembershipCard>()
            .Property(mc => mc.Cdk)
            .HasMaxLength(100);

        modelBuilder.Entity<MembershipCard>()
            .HasOne(mc => mc.User)
            .WithMany(u => u.MembershipCards)
            .HasForeignKey(mc => mc.UserId)
            .OnDelete(DeleteBehavior.SetNull);


    }
}
