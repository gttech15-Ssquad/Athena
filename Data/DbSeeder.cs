using Microsoft.EntityFrameworkCore;
using virtupay_corporate.Models;
using virtupay_corporate.Helpers;

namespace virtupay_corporate.Data
{
    /// <summary>
    /// Database seeder for initial data setup with multi-organization support.
    /// </summary>
    public static class DbSeeder
    {
        private static readonly IPasswordHasher _passwordHasher = new PasswordHasher();

        /// <summary>
        /// Seeds the database with initial data including organizations and memberships.
        /// </summary>
        public static async Task SeedAsync(CorporateDbContext context)
        {
            // Seed merchant categories if not already seeded (global, not org-specific)
            if (!context.MerchantCategories.Any())
            {
                var categories = new List<MerchantCategory>
                {
                    new MerchantCategory { Name = "Gas Stations", Code = "5541" },
                    new MerchantCategory { Name = "Hotels", Code = "7011" },
                    new MerchantCategory { Name = "Restaurants", Code = "5812" },
                    new MerchantCategory { Name = "Retail", Code = "5411" },
                    new MerchantCategory { Name = "Groceries", Code = "5421" },
                    new MerchantCategory { Name = "Airlines", Code = "4511" },
                    new MerchantCategory { Name = "Telecommunications", Code = "4814" },
                    new MerchantCategory { Name = "Utilities", Code = "4900" },
                    new MerchantCategory { Name = "Entertainment", Code = "7933" },
                    new MerchantCategory { Name = "Online Shopping", Code = "5398" }
                };

                await context.MerchantCategories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Seed demo organization and users if not already seeded
            if (!context.Organizations.Any())
            {
                // Create demo organization
                var demoOrg = new Organization
                {
                    Name = "Virtupay Corporation",
                    Industry = "Financial Technology",
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.Organizations.AddAsync(demoOrg);
                await context.SaveChangesAsync();

                // Create departments for the organization
                var departments = new List<Department>
                {
                    new Department 
                    { 
                        OrganizationId = demoOrg.Id,
                        Name = "Finance", 
                        Budget = 500000, 
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Department 
                    { 
                        OrganizationId = demoOrg.Id,
                        Name = "HR", 
                        Budget = 300000, 
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Department 
                    { 
                        OrganizationId = demoOrg.Id,
                        Name = "Operations", 
                        Budget = 400000, 
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Department 
                    { 
                        OrganizationId = demoOrg.Id,
                        Name = "Marketing", 
                        Budget = 350000, 
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Department 
                    { 
                        OrganizationId = demoOrg.Id,
                        Name = "Sales", 
                        Budget = 600000, 
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Department 
                    { 
                        OrganizationId = demoOrg.Id,
                        Name = "IT", 
                        Budget = 250000, 
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Department 
                    { 
                        OrganizationId = demoOrg.Id,
                        Name = "Compliance", 
                        Budget = 200000, 
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new Department 
                    { 
                        OrganizationId = demoOrg.Id,
                        Name = "Risk Management", 
                        Budget = 300000, 
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();

                var itDept = departments.FirstOrDefault(d => d.Name == "IT");
                var hrDept = departments.FirstOrDefault(d => d.Name == "HR");
                var complianceDept = departments.FirstOrDefault(d => d.Name == "Compliance");

                // Create demo users
                var demoUsers = new List<User>
                {
                    new User
                    {
                        Email = "ceo@virtupay.com",
                        PasswordHash = _passwordHasher.Hash("ceo123"),
                        Role = "Owner", // Global role for backward compatibility
                        FirstName = "Sarah",
                        LastName = "Johnson",
                        Status = "Active",
                        AccountNumber = GenerateAccountNumber(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "cfo@virtupay.com",
                        PasswordHash = _passwordHasher.Hash("cfo123"),
                        Role = "Admin",
                        FirstName = "Michael",
                        LastName = "Davis",
                        Status = "Active",
                        AccountNumber = GenerateAccountNumber(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "admin@virtupay.com",
                        PasswordHash = _passwordHasher.Hash("admin123"),
                        Role = "Admin",
                        FirstName = "John",
                        LastName = "Smith",
                        Status = "Active",
                        DepartmentId = itDept?.Id,
                        AccountNumber = GenerateAccountNumber(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "approver@virtupay.com",
                        PasswordHash = _passwordHasher.Hash("approver123"),
                        Role = "Approver",
                        FirstName = "Emily",
                        LastName = "Wilson",
                        Status = "Active",
                        DepartmentId = hrDept?.Id,
                        AccountNumber = GenerateAccountNumber(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "viewer@virtupay.com",
                        PasswordHash = _passwordHasher.Hash("viewer123"),
                        Role = "Viewer",
                        FirstName = "David",
                        LastName = "Martinez",
                        Status = "Active",
                        AccountNumber = GenerateAccountNumber(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new User
                    {
                        Email = "auditor@virtupay.com",
                        PasswordHash = _passwordHasher.Hash("auditor123"),
                        Role = "Auditor",
                        FirstName = "Robert",
                        LastName = "Brown",
                        Status = "Active",
                        DepartmentId = complianceDept?.Id,
                        AccountNumber = GenerateAccountNumber(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.Users.AddRangeAsync(demoUsers);
                await context.SaveChangesAsync();

                // Create organization memberships with appropriate roles
                var memberships = new List<OrganizationUser>
                {
                    new OrganizationUser
                    {
                        OrganizationId = demoOrg.Id,
                        UserId = demoUsers[0].Id, // CEO
                        OrgRole = "Owner",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new OrganizationUser
                    {
                        OrganizationId = demoOrg.Id,
                        UserId = demoUsers[1].Id, // CFO
                        OrgRole = "Admin",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new OrganizationUser
                    {
                        OrganizationId = demoOrg.Id,
                        UserId = demoUsers[2].Id, // Admin
                        OrgRole = "Admin",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new OrganizationUser
                    {
                        OrganizationId = demoOrg.Id,
                        UserId = demoUsers[3].Id, // Approver
                        OrgRole = "Approver",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new OrganizationUser
                    {
                        OrganizationId = demoOrg.Id,
                        UserId = demoUsers[4].Id, // Viewer
                        OrgRole = "Viewer",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new OrganizationUser
                    {
                        OrganizationId = demoOrg.Id,
                        UserId = demoUsers[5].Id, // Auditor
                        OrgRole = "Auditor",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                };

                await context.OrganizationUsers.AddRangeAsync(memberships);
                await context.SaveChangesAsync();

                // Create account balance for the organization
                var accountBalance = new AccountBalance
                {
                    OrganizationId = demoOrg.Id,
                    AvailableBalance = 1000000, // Initial balance for demo
                    TotalFunded = 1000000,
                    TotalWithdrawn = 0,
                    Currency = "NGN",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.AccountBalances.AddAsync(accountBalance);
                await context.SaveChangesAsync();
            }
        }

        private static string GenerateAccountNumber()
        {
            var random = new Random();
            long randomLong = random.NextInt64(1000000000, 10000000000);
            return randomLong.ToString();
        }
    }
}
