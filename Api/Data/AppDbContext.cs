using System.Runtime.Serialization;
using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Party> Parties { get; set; }
    public DbSet<InvoiceItem> InvoiceItems { get; set; }
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();

    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<InvoiceDocument> InvoiceDocuments => Set<InvoiceDocument>();
    public DbSet<InvoiceSmsLog> InvoiceSmsLogs => Set<InvoiceSmsLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite("Data Source=invoices.db"); // استفاده از دیتابیس سبک SQLite



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

      
        modelBuilder.Entity<InvoiceDocument>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.InvoiceId)
                .IsUnique();

            entity.HasIndex(x => x.AccessTokenHash)
                .IsUnique();

            entity.Property(x => x.FilePath)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.AccessTokenHash)
                .HasMaxLength(64)
                .IsRequired();

            entity.HasOne(x => x.Invoice)
                .WithOne()
                .HasForeignKey<InvoiceDocument>(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CompanyProfile>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(250)
                .IsRequired();

            entity.HasOne(x => x.DefaultBankAccount)
                .WithMany()
                .HasForeignKey(x => x.DefaultBankAccountId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.BankName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ShebaNumber)
                .HasMaxLength(26);
        });

        modelBuilder.Entity<InvoiceSmsLog>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasIndex(x => x.InvoiceId);

            entity.Property(x => x.Mobile)
                .HasMaxLength(20)
                .IsRequired();
        });
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Type)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.Status,
                x.NextAttemptAt
            });

            // جلوگیری از ثبت مجدد یک عملیات برای یک فاکتور
            entity.HasIndex(x => new
            {
                x.Type,
                x.RelatedEntityId
            })
            .IsUnique()
            .HasDatabaseName(
                "UX_OutboxMessages_Type_RelatedEntityId"
            )
            .HasFilter("\"RelatedEntityId\" IS NOT NULL");
        });
            
    }
}