using Microsoft.EntityFrameworkCore;
using proiect_licenta.Models;

namespace proiect_licenta.Contexts;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<App> Apps { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AppCategory> AppCategories { get; set; }
    public DbSet<Card> Cards { get; set; }
    public DbSet<Install> Installs { get; set; }
    public DbSet<MyUser> MyUsers { get; set; }
    public DbSet<PaymentRecord> PaymentRecords { get; set; }
    public DbSet<RefundRequest> RefundRequests { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<UserCard> UserCards { get; set; }
    public DbSet<UserVoucher> UserVouchers { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AppCategory>().HasKey(ab => new { ab.AppId, ab.CategoryId });
        modelBuilder.Entity<Install>().HasKey(ab => new { ab.UserId, ab.AppId, ab.PaymentId });
        modelBuilder.Entity<Report>().HasKey(ab => new { ab.UserId, ab.AppId });
        modelBuilder.Entity<Review>().HasKey(ab => new { ab.AppId, ab.UserId });
        modelBuilder.Entity<UserCard>().HasKey(ab => new { ab.CardId, ab.UserId });
        modelBuilder.Entity<UserVoucher>().HasKey(ab => new { ab.VoucherId, ab.UserId });
        
        // user - usercard - card relationship
        modelBuilder.Entity<UserCard>()
            .HasOne(t => t.User)
            .WithMany(t => t.UserCards)
            .HasForeignKey(t => t.UserId);

        modelBuilder.Entity<UserCard>()
            .HasOne(t => t.Card)
            .WithMany(t => t.UserCards)
            .HasForeignKey(t => t.CardId);


        // user - uservoucher - voucher relationship
        modelBuilder.Entity<UserVoucher>()
            .HasOne(t => t.User)
            .WithMany(t => t.UserVouchers)
            .HasForeignKey(t => t.UserId);

        modelBuilder.Entity<UserVoucher>()
            .HasOne(t => t.Voucher)
            .WithMany(t => t.UserVouchers)
            .HasForeignKey(t => t.VoucherId);


        // app - appcategory - category relationship
        modelBuilder.Entity<AppCategory>()
            .HasOne(t => t.App)
            .WithMany(t => t.AppCategories)
            .HasForeignKey(t => t.AppId);

        modelBuilder.Entity<AppCategory>()
            .HasOne(t => t.Category)
            .WithMany(t => t.AppCategories)
            .HasForeignKey(t => t.CategoryId);
        
        // install relationship
        modelBuilder.Entity<Install>()
            .HasOne(t => t.User)
            .WithMany(t => t.Installs)
            .HasForeignKey(t => t.UserId);
        modelBuilder.Entity<Install>()
            .HasOne(t => t.App)
            .WithMany(t => t.Installs)
            .HasForeignKey(t => t.AppId);
        modelBuilder.Entity<Install>()
            .HasOne(t => t.PaymentRecord)
            .WithOne(t => t.Install)
            .HasForeignKey<Install>(t => t.PaymentId);
        
        // refund request relationship
        modelBuilder.Entity<RefundRequest>()
            .HasOne(t => t.User)
            .WithMany(t => t.RefundRequests)
            .HasForeignKey(t => t.UserId);
        modelBuilder.Entity<RefundRequest>()
            .HasOne(t => t.PaymentRecord)
            .WithOne(t => t.RefundRequest)
            .HasForeignKey<RefundRequest>(t => t.PaymentId);
    }
}