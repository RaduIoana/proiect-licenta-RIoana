using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using proiect_licenta.Models;

namespace proiect_licenta.Contexts;

public class ApplicationDbContext : IdentityDbContext<MyUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<AppFile> AppFiles { get; set; }
    public DbSet<App> Apps { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<AppCategory> AppCategories { get; set; }
    public DbSet<LibraryRecord> Libraries { get; set; }
    public DbSet<MyUser> MyUsers { get; set; }
    public DbSet<PaymentRecord> PaymentRecords { get; set; }
    public DbSet<License> Licenses { get; set; }
    public DbSet<RefundRequest> RefundRequests { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<UserVoucher> UserVouchers { get; set; }
    public DbSet<Voucher> Vouchers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AppCategory>().HasKey(ab => new { ab.AppId, ab.CategoryId });
        modelBuilder.Entity<LibraryRecord>().HasKey(ab => new { ab.UserId, ab.AppId, ab.PaymentId });
        modelBuilder.Entity<Report>().HasKey(ab => new { ab.UserId, ab.AppId });
        modelBuilder.Entity<Review>().HasKey(ab => new { ab.AppId, ab.UserId });
        modelBuilder.Entity<UserVoucher>().HasKey(ab => new { ab.VoucherId, ab.UserId });
        
        modelBuilder.Entity<App>()
            .HasOne(a => a.AppFile)
            .WithOne(af => af.App)
            .HasForeignKey<AppFile>(af => af.AppId);

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
        modelBuilder.Entity<LibraryRecord>()
            .HasOne(t => t.User)
            .WithMany(t => t.Libraries)
            .HasForeignKey(t => t.UserId);
        modelBuilder.Entity<LibraryRecord>()
            .HasOne(t => t.App)
            .WithMany(t => t.Libraries)
            .HasForeignKey(t => t.AppId);
        modelBuilder.Entity<LibraryRecord>()
            .HasOne(t => t.PaymentRecord)
            .WithOne(t => t.LibraryRecord)
            .HasForeignKey<LibraryRecord>(t => t.PaymentId);
        
        // review relationship
        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId);
        modelBuilder.Entity<Review>()
            .HasOne(r => r.App)
            .WithMany(a => a.Reviews)
            .HasForeignKey(r => r.AppId);
        
        // refund request relationship
        modelBuilder.Entity<RefundRequest>()
            .HasOne(t => t.User)
            .WithMany(t => t.RefundRequests)
            .HasForeignKey(t => t.UserId);
        modelBuilder.Entity<RefundRequest>()
            .HasOne(t => t.PaymentRecord)
            .WithOne(t => t.RefundRequest)
            .HasForeignKey<RefundRequest>(t => t.PaymentId);
        
        // user - uservoucher - voucher relationship
        modelBuilder.Entity<UserVoucher>()
            .HasOne(t => t.User)
            .WithMany(t => t.UserVouchers)
            .HasForeignKey(t => t.UserId);
        modelBuilder.Entity<UserVoucher>()
            .HasOne(t => t.Voucher)
            .WithMany(t => t.UserVouchers)
            .HasForeignKey(t => t.VoucherId);
    }
}