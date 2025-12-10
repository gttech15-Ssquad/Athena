# Setup Instructions - Multi-Organization Platform

## 🚀 Quick Start (Development)

### Option 1: Fresh Start (Recommended for Testing)

Since you have an existing database with the old schema, the easiest approach is to start fresh:

1. **Delete the existing database file:**
   ```powershell
   # In PowerShell (current directory)
   Remove-Item virtupay-corporate.db -ErrorAction SilentlyContinue
   Remove-Item virtupay-corporate.db-shm -ErrorAction SilentlyContinue
   Remove-Item virtupay-corporate.db-wal -ErrorAction SilentlyContinue
   ```

2. **Run the application:**
   ```powershell
   dotnet run
   ```

   The application will:
   - Automatically create the new database schema with all tables
   - Run the database seeder to create demo organizations and users
   - Be ready to use immediately

3. **Test with demo credentials:**
   - **CEO/Owner**: ceo@virtupay.com / ceo123
   - **Admin**: admin@virtupay.com / admin123
   - **Approver**: approver@virtupay.com / approver123
   - **Viewer**: viewer@virtupay.com / viewer123

### Option 2: Keep Existing Data (Requires Migration)

If you have important data in the existing database, you'll need to:

1. **Create EF Core Migration:**
   ```powershell
   dotnet ef migrations add MultiOrganizationMigration
   ```

2. **Review the migration** (in `Migrations/` folder) to ensure it:
   - Creates Organizations and OrganizationUsers tables
   - Adds OrganizationId to existing tables
   - Adds new foreign key relationships

3. **Create a data migration script** to:
   - Create a default organization for each existing user
   - Create OrganizationUser memberships
   - Update existing records with organizationId

4. **Apply the migration:**
   ```powershell
   dotnet ef database update
   ```

## 📋 Step-by-Step Setup

### Step 1: Clean Database (If Starting Fresh)

```powershell
# Delete existing database
Remove-Item virtupay-corporate.db* -ErrorAction SilentlyContinue
```

### Step 2: Run the Application

```powershell
dotnet run
```

The application will:
- ✅ Create all database tables automatically
- ✅ Seed demo organization "Virtupay Corporation"
- ✅ Create 6 demo users with different roles
- ✅ Create 8 departments
- ✅ Create organization account balance (1,000,000 NGN)

### Step 3: Access the API

- **Swagger UI**: http://localhost:5001 (or check your launchSettings.json)
- **API Base URL**: http://localhost:5001/api

### Step 4: Test Registration

Try registering a new user:
```json
POST /api/auth/register
{
  "email": "newuser@example.com",
  "password": "password123",
  "role": "APP",
  "organizationName": "My New Company",
  "firstName": "John",
  "lastName": "Doe",
  "industry": "Technology"
}
```

This will:
- Create a new user
- Create a new organization "My New Company"
- Create an OrganizationUser membership with "Owner" role
- Return a JWT token with organization context

### Step 5: Test Login

```json
POST /api/auth/login
{
  "email": "ceo@virtupay.com",
  "password": "ceo123"
}
```

Response includes:
- JWT token with `orgId` and `membershipId` claims
- Organization context (OrganizationId, OrgRole, MembershipId)

## 🔄 Migration Strategy (If You Have Existing Data)

### For Production/Existing Data:

1. **Install EF Core Tools** (if not already installed):
   ```powershell
   dotnet tool install --global dotnet-ef
   ```

2. **Create Migration:**
   ```powershell
   dotnet ef migrations add AddMultiOrganizationSupport
   ```

3. **Create Data Migration Script:**

   You'll need to manually create a script to:
   ```sql
   -- For each existing user:
   -- 1. Create an organization
   INSERT INTO Organizations (Id, Name, Status, CreatedAt, UpdatedAt)
   VALUES (NEWID(), 'Default Organization for [UserEmail]', 'Active', GETUTCDATE(), GETUTCDATE());
   
   -- 2. Create membership
   INSERT INTO OrganizationUsers (Id, OrganizationId, UserId, OrgRole, Status, CreatedAt, UpdatedAt)
   VALUES (NEWID(), @OrgId, @UserId, 'Owner', 'Active', GETUTCDATE(), GETUTCDATE());
   
   -- 3. Update existing records
   UPDATE Departments SET OrganizationId = @OrgId WHERE ...
   UPDATE VirtualCards SET OrganizationId = @OrgId, OwnerMembershipId = @MembershipId WHERE ...
   UPDATE AccountBalances SET OrganizationId = @OrgId WHERE ...
   ```

4. **Apply Migration:**
   ```powershell
   dotnet ef database update
   ```

## ✅ What Happens Automatically

When you run `dotnet run` in **Development mode**:

1. **Database Creation**: `EnsureCreatedAsync()` creates all tables from your DbContext models
2. **Data Seeding**: `DbSeeder.SeedAsync()` runs automatically and creates:
   - 1 demo organization
   - 6 demo users
   - 6 organization memberships
   - 8 departments (scoped to organization)
   - 10 merchant categories
   - 1 organization account balance

## 🧪 Testing the Multi-Organization Features

### 1. Test Organization Switching

```bash
# Login first
POST /api/auth/login
# Save the token

# Get user's organizations
GET /api/auth/organizations
Authorization: Bearer {token}

# Switch to a different organization (if user belongs to multiple)
POST /api/auth/switch-organization
Authorization: Bearer {token}
{
  "organizationId": "{org-id}"
}
```

### 2. Test Card Creation (Requires Approval)

```bash
# Create a card (as Approver role)
POST /api/cards
Authorization: Bearer {token}
{
  "cardholderName": "John Doe",
  "nickname": "My Card",
  "departmentId": "{dept-id}"
}

# Card will be created with status "PENDING"
# An approval request will be created automatically
```

### 3. Test Approval Workflow

```bash
# Get pending approvals (as Admin/Owner)
GET /api/approvals/pending
Authorization: Bearer {token}

# Approve a card creation
PUT /api/approvals/{approvalId}/approve
Authorization: Bearer {token}
{
  "comment": "Approved for business use"
}
```

## 📝 Important Notes

### Development Mode (Current Setup)
- Uses `EnsureCreatedAsync()` - **recreates database on schema changes**
- **Will delete existing data** if schema changes
- Perfect for development and testing
- No migration scripts needed

### Production Mode
- Uses `MigrateAsync()` - **applies migrations incrementally**
- **Preserves existing data**
- Requires EF Core migrations
- Must create and apply migrations before deployment

## 🎯 Recommended Next Steps

1. **For Development/Testing:**
   - ✅ Delete existing database
   - ✅ Run `dotnet run`
   - ✅ Test with demo credentials
   - ✅ No migration needed!

2. **For Production:**
   - ⚠️ Create EF Core migrations
   - ⚠️ Create data migration script
   - ⚠️ Test migration on staging first
   - ⚠️ Apply migration to production

## 🔍 Verify Setup

After running the application, check:

1. **Database file exists**: `virtupay-corporate.db`
2. **Swagger UI loads**: http://localhost:5001
3. **Can login**: Use ceo@virtupay.com / ceo123
4. **Token contains orgId**: Check JWT token claims
5. **Can list organizations**: GET /api/auth/organizations

## ❓ Troubleshooting

### Issue: "Table already exists" errors
**Solution**: Delete the database file and restart

### Issue: "Foreign key constraint" errors
**Solution**: Ensure DbSeeder runs after database creation (it does automatically)

### Issue: Can't login with demo credentials
**Solution**: Check that DbSeeder ran successfully (check logs)

### Issue: Missing organization context in token
**Solution**: Ensure user has an active OrganizationUser membership

## 📚 Additional Resources

- See `MULTI_ORGANIZATION_IMPLEMENTATION.md` for full architecture details
- See `API_ENDPOINTS_DESCRIPTION.md` for API documentation
- Check application logs in `logs/` folder

