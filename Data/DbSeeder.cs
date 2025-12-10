using Microsoft.EntityFrameworkCore;
using virtupay_corporate.Models;

namespace virtupay_corporate.Data
{
    /// <summary>
    /// Database seeder for initial data setup.
    /// </summary>
    public static class DbSeeder
    {
        /// <summary>
        /// Seeds the database with initial data.
        /// </summary>
        public static async Task SeedAsync(CorporateDbContext context)
        {
            // Seed departments if not already seeded
            if (!context.Departments.Any())
            {
                var departments = new List<Department>
                {
                    new Department { Name = "Finance", Budget = 500000, Status = "Active" },
                    new Department { Name = "HR", Budget = 300000, Status = "Active" },
                    new Department { Name = "Operations", Budget = 400000, Status = "Active" },
                    new Department { Name = "Marketing", Budget = 350000, Status = "Active" },
                    new Department { Name = "Sales", Budget = 600000, Status = "Active" },
                    new Department { Name = "IT", Budget = 250000, Status = "Active" },
                    new Department { Name = "Compliance", Budget = 200000, Status = "Active" },
                    new Department { Name = "Risk Management", Budget = 300000, Status = "Active" }
                };

                await context.Departments.AddRangeAsync(departments);
                await context.SaveChangesAsync();
            }

            // Seed merchant categories if not already seeded
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

            // Seed demo users if not already seeded
            if (!context.Users.Any())
            {
                // Get departments for reference
                var departments = await context.Departments.ToListAsync();
                var itDept = departments.FirstOrDefault(d => d.Name == "IT");
                var hrDept = departments.FirstOrDefault(d => d.Name == "HR");
                var complianceDept = departments.FirstOrDefault(d => d.Name == "Compliance");

                // Assuming password hashing would be done in the actual application
                // For demo purposes, using placeholder hashes (would need proper hashing in production)
                var demoUsers = new List<User>
                {
                    new User
                    {
                        Email = "ceo@virtupay.com",
                        PasswordHash = "$2a$12$R9h/cIPz0gi.URNN.mJCOOVNxXG7wCWBHrLBXrHsT3gW5cWkMgO1O", // "ceo123" hashed
                        Role = "CEO",
                        FirstName = "Sarah",
                        LastName = "Johnson",
                        Status = "Active"
                    },
                    new User
                    {
                        Email = "cfo@virtupay.com",
                        PasswordHash = "$2a$12$pGpWFXwg9zDKPyJ3JqLkIeP7BxLKX5CqK5VJy9RbCRIlbQkV1tJwC", // "cfo123" hashed
                        Role = "CFO",
                        FirstName = "Michael",
                        LastName = "Davis",
                        Status = "Active"
                    },
                    new User
                    {
                        Email = "admin@virtupay.com",
                        PasswordHash = "$2a$12$KnW4/5QL8hB7mK3pL9sJQOVXyDJrU4mCh8GkF2qT1xJ5rS8nV0KhK", // "admin123" hashed
                        Role = "Admin",
                        FirstName = "John",
                        LastName = "Smith",
                        Status = "Active",
                        DepartmentId = itDept?.Id
                    },
                    new User
                    {
                        Email = "delegate@virtupay.com",
                        PasswordHash = "$2a$12$VbM2/7qL5hN8jK1pP3mRfOV4eF5gH2jI8yK9lR0mQ1aN2bS5cD4dP", // "delegate123" hashed
                        Role = "Delegate",
                        FirstName = "Emily",
                        LastName = "Wilson",
                        Status = "Active",
                        DepartmentId = hrDept?.Id
                    },
                    new User
                    {
                        Email = "auditor@virtupay.com",
                        PasswordHash = "$2a$12$QvA6/3zL2hK5jL7pM9rFaOP1eG4hI3jK0mL1nO2pP3qR4sT5uV6wX", // "auditor123" hashed
                        Role = "Auditor",
                        FirstName = "Robert",
                        LastName = "Brown",
                        Status = "Active",
                        DepartmentId = complianceDept?.Id
                    }
                };

                await context.Users.AddRangeAsync(demoUsers);
                await context.SaveChangesAsync();
            }
        }
    }
}
