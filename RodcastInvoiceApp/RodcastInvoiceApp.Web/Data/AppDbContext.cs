using Microsoft.EntityFrameworkCore;
using RodcastInvoiceApp.Web.Data.Common;
using RodcastInvoiceApp.Web.Data.Models;

namespace RodcastInvoiceApp.Web.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<PriceRule> PriceRules { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<CompanySettings> CompanySettings { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Un cliente con proyectos asociados no se puede eliminar por accidente.
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // El codigo de proyecto debe ser unico dentro de un mismo cliente.
            modelBuilder.Entity<Project>()
                .HasIndex(p => new { p.ClientId, p.Code })
                .IsUnique();

            modelBuilder.Entity<Project>()
                .Property(p => p.BillingType)
                .HasConversion<string>()
                .HasMaxLength(20);

            modelBuilder.Entity<Project>()
                .Property(p => p.Config)
                .HasColumnType("json");

            // Las tarifas son configuracion del proyecto: si se borra el proyecto, se borran con el.
            modelBuilder.Entity<PriceRule>()
                .HasOne(pr => pr.Project)
                .WithMany(p => p.PriceRules)
                .HasForeignKey(pr => pr.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Un proyecto con facturas no se puede eliminar por accidente.
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Project)
                .WithMany()
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            // Una cuenta bancaria referenciada por facturas no se puede eliminar.
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.BankAccount)
                .WithMany(b => b.Invoices)
                .HasForeignKey(i => i.BankAccountId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasConversion<string>()
                .HasMaxLength(10);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.TimesheetExceptions)
                .HasColumnType("json");

            // Las lineas de detalle son parte de la factura: se borran con ella.
            modelBuilder.Entity<InvoiceItem>()
                .HasOne(ii => ii.Invoice)
                .WithMany(i => i.InvoiceItems)
                .HasForeignKey(ii => ii.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Los pagos son historial financiero: una factura con pagos registrados
            // no se puede eliminar (a diferencia de los InvoiceItems).
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = now;
                else if (entry.State == EntityState.Modified)
                    entry.Entity.UpdatedAt = now;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
