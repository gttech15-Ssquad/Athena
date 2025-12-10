using Microsoft.EntityFrameworkCore;
using virtupay_corporate.Models;

namespace virtupay_corporate.Data
{
    /// <summary>
    /// Entity Framework Core DbContext for Virtupay Corporate.
    /// Manages all database entities and relationships.
    /// </summary>
    public class CorporateDbContext : DbContext
    {
   /// <summary>
 /// Initializes a new instance of the <see cref="CorporateDbContext"/> class.
        /// </summary>
 public CorporateDbContext(DbContextOptions<CorporateDbContext> options) : base(options)
    {
        }

     #region DbSets

        public DbSet<Organization> Organizations { get; set; } = null!;
        public DbSet<OrganizationUser> OrganizationUsers { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Department> Departments { get; set; } = null!;

        public DbSet<VirtualCard> VirtualCards { get; set; } = null!;

        /// <summary>
        /// Gets or sets the CardLimits DbSet.
    /// </summary>
      public DbSet<CardLimit> CardLimits { get; set; } = null!;

 /// <summary>
        /// Gets or sets the MerchantCategories DbSet.
   /// </summary>
        public DbSet<MerchantCategory> MerchantCategories { get; set; } = null!;

        /// <summary>
        /// Gets or sets the CardMerchantRestrictions DbSet.
        /// </summary>
  public DbSet<CardMerchantRestriction> CardMerchantRestrictions { get; set; } = null!;

     /// <summary>
/// Gets or sets the CardTransactions DbSet.
   /// </summary>
        public DbSet<CardTransaction> CardTransactions { get; set; } = null!;

    /// <summary>
        /// Gets or sets the CardApprovals DbSet.
        /// </summary>
        public DbSet<CardApproval> CardApprovals { get; set; } = null!;

        /// <summary>
     /// Gets or sets the CardBalances DbSet.
      /// </summary>
        public DbSet<CardBalance> CardBalances { get; set; } = null!;

    /// <summary>
  /// Gets or sets the AuditLogs DbSet.
        /// </summary>
 public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        public DbSet<AccountBalance> AccountBalances { get; set; } = null!;

        /// <summary>
    /// Gets or sets the AccountTransactions DbSet.
      /// </summary>
        public DbSet<AccountTransaction> AccountTransactions { get; set; } = null!;

      #endregion

      /// <summary>
 /// Configures the database model relationships and constraints.
    /// </summary>
     protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
          base.OnModelCreating(modelBuilder);

            // Organization configuration
            modelBuilder.Entity<Organization>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
            });

            // OrganizationUser configuration
            modelBuilder.Entity<OrganizationUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrgRole).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => new { e.OrganizationId, e.UserId }).IsUnique();
                entity.HasOne(e => e.Organization).WithMany(o => o.Members).HasForeignKey(e => e.OrganizationId);
                entity.HasOne(e => e.User).WithMany(u => u.Memberships).HasForeignKey(e => e.UserId);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
                entity.Property(e => e.GlobalStatus).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasOne(e => e.Department).WithMany(d => d.Users).HasForeignKey(e => e.DepartmentId);
            });

            // Department configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Budget).HasPrecision(18, 2);
                entity.HasOne(e => e.Organization).WithMany(o => o.Departments).HasForeignKey(e => e.OrganizationId);
            });

            // VirtualCard configuration
            modelBuilder.Entity<VirtualCard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CardNumber).IsRequired().HasMaxLength(19);
                entity.Property(e => e.CVV).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CardholderName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.HasIndex(e => e.CardNumber).IsUnique();
                entity.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId);
                entity.HasOne(e => e.OwnerMembership).WithMany().HasForeignKey(e => e.OwnerMembershipId);
                entity.HasOne(e => e.CardBalance).WithOne(b => b.VirtualCard).HasForeignKey<CardBalance>(b => b.CardId);
            });

   // CardLimit configuration
            modelBuilder.Entity<CardLimit>(entity =>
        {
     entity.HasKey(e => e.Id);
     entity.Property(e => e.LimitType).IsRequired().HasMaxLength(50);
  entity.Property(e => e.Amount).HasPrecision(18, 2);
     entity.Property(e => e.Period).IsRequired().HasMaxLength(50);
   entity.Property(e => e.Threshold).HasPrecision(18, 2);
entity.HasOne(e => e.VirtualCard).WithMany(c => c.CardLimits).HasForeignKey(e => e.CardId);
    });

          // MerchantCategory configuration
       modelBuilder.Entity<MerchantCategory>(entity =>
      {
    entity.HasKey(e => e.Id);
  entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
 entity.Property(e => e.Code).IsRequired().HasMaxLength(10);
     entity.HasIndex(e => e.Code).IsUnique();
        });

        // CardMerchantRestriction configuration
 modelBuilder.Entity<CardMerchantRestriction>(entity =>
   {
        entity.HasKey(e => e.Id);
    entity.HasOne(e => e.VirtualCard).WithMany(c => c.MerchantRestrictions).HasForeignKey(e => e.CardId);
       entity.HasOne(e => e.MerchantCategory).WithMany().HasForeignKey(e => e.MerchantCategoryId);
       });

  // CardTransaction configuration
            modelBuilder.Entity<CardTransaction>(entity =>
  {
        entity.HasKey(e => e.Id);
       entity.Property(e => e.Amount).HasPrecision(18, 2);
       entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
         entity.Property(e => e.Merchant).IsRequired().HasMaxLength(255);
    entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
       entity.HasOne(e => e.VirtualCard).WithMany(c => c.Transactions).HasForeignKey(e => e.CardId);
            });

            // CardApproval configuration
            modelBuilder.Entity<CardApproval>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ActionType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Reason).HasMaxLength(500);
                entity.HasOne(e => e.VirtualCard).WithMany(c => c.Approvals).HasForeignKey(e => e.CardId);
                entity.HasOne(e => e.RequestedByMembership).WithMany().HasForeignKey(e => e.RequestedByMembershipId);
                entity.HasOne(e => e.ApprovedByMembership).WithMany().HasForeignKey(e => e.ApprovedByMembershipId);
            });

          // CardBalance configuration
     modelBuilder.Entity<CardBalance>(entity =>
       {
      entity.HasKey(e => e.Id);
         entity.Property(e => e.AvailableBalance).HasPrecision(18, 2);
  entity.Property(e => e.ReservedBalance).HasPrecision(18, 2);
     entity.Property(e => e.UsedBalance).HasPrecision(18, 2);
     entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
            });

          // AuditLog configuration
      modelBuilder.Entity<AuditLog>(entity =>
      {
      entity.HasKey(e => e.Id);
 entity.Property(e => e.Action).IsRequired().HasMaxLength(255);
      entity.Property(e => e.Resource).IsRequired().HasMaxLength(255);
        entity.Property(e => e.Changes).HasMaxLength(2000);
 entity.Property(e => e.IpAddress).HasMaxLength(50);
   entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
    });

      // Add unique constraint for email
 modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // AccountBalance configuration
            modelBuilder.Entity<AccountBalance>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AvailableBalance).HasPrecision(18, 2);
                entity.Property(e => e.TotalFunded).HasPrecision(18, 2);
                entity.Property(e => e.TotalWithdrawn).HasPrecision(18, 2);
                entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
                entity.HasOne(e => e.Organization).WithMany().HasForeignKey(e => e.OrganizationId);
                entity.HasOne(e => e.User).WithOne(u => u.AccountBalance).HasForeignKey<AccountBalance>(a => a.UserId);
                entity.HasOne(e => e.OrganizationUser).WithMany().HasForeignKey(e => e.OrganizationUserId);
            });

        // AccountTransaction configuration
 modelBuilder.Entity<AccountTransaction>(entity =>
    {
   entity.HasKey(e => e.Id);
      entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(50);
       entity.Property(e => e.Amount).HasPrecision(18, 2);
        entity.Property(e => e.BalanceBefore).HasPrecision(18, 2);
      entity.Property(e => e.BalanceAfter).HasPrecision(18, 2);
     entity.Property(e => e.Description).HasMaxLength(500);
   entity.Property(e => e.ReferenceId).HasMaxLength(100);
    entity.Property(e => e.Currency).IsRequired().HasMaxLength(3);
        entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
      entity.HasOne(e => e.AccountBalance).WithMany(a => a.Transactions).HasForeignKey(e => e.AccountBalanceId);
  });
      }

    /// <summary>
      /// Saves all changes made in this context to the database.
        /// Automatically logs audit information for tracked changes.
      /// </summary>
 public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
      {
            return await base.SaveChangesAsync(cancellationToken);
        }
  }
}
