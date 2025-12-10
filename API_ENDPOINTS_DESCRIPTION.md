# Virtupay Corporate API - Complete Endpoint Descriptions

## Overview
This document contains short descriptions for all API endpoints as displayed in Swagger UI.

---

## ?? Authentication Endpoints

### POST `/api/auth/register`
**Register a new user account**

Creates a new user account with email and password. Automatically generates a unique 10-digit account number and creates an AccountBalance record. Returns a JWT token upon successful registration for immediate API access.

**Request**: Email, Password, Role (APP/VIEW), FirstName, LastName
**Response**: JWT Token, User Profile, Account Number
**Status**: 201 Created / 400 Bad Request

---

### POST `/api/auth/login`
**Login and obtain JWT token**

Authenticates a user with email and password, returning a JWT token for subsequent API calls. Token expires in 24 hours and must be included in the Authorization header for protected endpoints.

**Request**: Email, Password
**Response**: JWT Token, User Profile, Expiration Time
**Status**: 200 OK / 401 Unauthorized

---

### POST `/api/auth/change-password`
**Change user password**

Updates the authenticated user's password by verifying the old password and setting a new one. The user must be logged in and provide both old and new passwords.

**Requires**: JWT Authentication
**Request**: Old Password, New Password
**Response**: Success Message
**Status**: 200 OK / 400 Bad Request / 401 Unauthorized

---

### GET `/api/auth/profile`
**Get current user profile**

Retrieves the complete profile information of the currently authenticated user including email, role, name, status, and account creation date.

**Requires**: JWT Authentication
**Response**: User Email, Role, Account Number, First Name, Last Name, Status, Creation Date
**Status**: 200 OK / 401 Unauthorized

---

## ?? Card Management Endpoints

### POST `/api/cards`
**Create a new virtual card**

Generates a new virtual card for the authenticated user with assigned cardholder name and optional nickname. Returns complete card details including masked card number, expiry date, and initial status.

**Requires**: APP Role (Approver)
**Request**: Cardholder Name, Nickname (optional), Department ID (optional)
**Response**: Card ID, Masked Card Number, Expiry Date, Status
**Status**: 201 Created / 400 Bad Request / 403 Forbidden

---

### GET `/api/cards`
**Get all user virtual cards**

Retrieves paginated list of all virtual cards associated with the authenticated user. Returns card summaries with masked card numbers and status information.

**Query Parameters**: pageNumber (default: 1), pageSize (default: 20)
**Response**: Paginated list of cards with masked numbers
**Status**: 200 OK

---

### GET `/api/cards/{cardId}`
**Get card details by ID**

Retrieves complete information for a specific card including masked card number, status, metadata, creation date, and update history.

**Path**: cardId (Guid)
**Response**: Card details with masked number
**Status**: 200 OK / 404 Not Found

---

### GET `/api/cards/{cardId}/details`
**Get card details with balance and limits**

Comprehensive card information including current balance breakdown (available, reserved, used), active spending limits, merchant restrictions, and transaction history. Shows unmasked card number, CVV, and formatted expiry date.

**Path**: cardId (Guid)
**Response**: Full card details, balance, limits, merchant restrictions, recent transactions
**Status**: 200 OK / 404 Not Found

---

### PUT `/api/cards/{cardId}`
**Update card settings and preferences**

Modifies card properties such as nickname and international transaction settings for an existing card. Any authenticated user can update their own cards.

**Path**: cardId (Guid)
**Request**: Nickname (optional), Allow International (optional)
**Response**: Updated card details
**Status**: 200 OK / 404 Not Found

---

### POST `/api/cards/{cardId}/freeze`
**Freeze a virtual card**

Temporarily disables a virtual card for transactions while maintaining its data and allowing future reactivation. Only Approvers (APP role) can freeze cards.

**Requires**: APP Role (Approver)
**Path**: cardId (Guid)
**Request**: Freeze Reason
**Response**: Success message
**Status**: 200 OK / 403 Forbidden / 404 Not Found

---

### POST `/api/cards/{cardId}/unfreeze`
**Unfreeze a virtual card**

Reactivates a previously frozen virtual card, allowing normal transaction processing to resume. Only Approvers (APP role) can unfreeze cards.

**Requires**: APP Role (Approver)
**Path**: cardId (Guid)
**Response**: Success message
**Status**: 200 OK / 403 Forbidden / 404 Not Found

---

### DELETE `/api/cards/{cardId}`
**Cancel/Delete a virtual card**

Permanently deactivates and removes a virtual card from the system. This action is irreversible. Only Approvers (APP role) can delete cards.

**Requires**: APP Role (Approver)
**Path**: cardId (Guid)
**Status**: 204 No Content / 403 Forbidden / 404 Not Found

---

### POST `/api/cards/{cardId}/limits`
**Set spending limits on a card**

Configures spending limits with specified type (daily, monthly, transaction), amount, and threshold for alerts. Any authenticated user can set limits on their cards.

**Path**: cardId (Guid)
**Request**: Limit Type, Amount, Period, Threshold Percentage
**Response**: Created limit details
**Status**: 201 Created / 400 Bad Request

---

### GET `/api/cards/{cardId}/limits`
**Get all spending limits for a card**

Retrieves all active and inactive spending limits configured for a specific virtual card.

**Path**: cardId (Guid)
**Response**: List of card limits with amounts and thresholds
**Status**: 200 OK

---

### GET `/api/cards/{cardId}/balance`
**Get card balance**

Retrieves current balance information including available, reserved, and used amounts in the specified currency (NGN).

**Path**: cardId (Guid)
**Response**: Available Balance, Reserved Balance, Used Balance, Currency, Last Updated
**Status**: 200 OK / 404 Not Found

---

### POST `/api/cards/{cardId}/balance/fund`
**Fund/Add balance to a virtual card**

Adds funds to the available balance of a virtual card. This increases the cardholder's available funds and can be used for initial setup, periodic top-ups, or corrections. Each funding transaction is logged and tracked for audit purposes.

**Path**: cardId (Guid)
**Request**: Amount, Reason, Reference ID (optional)
**Response**: Updated card balance
**Status**: 200 OK / 400 Bad Request / 404 Not Found

---

### PUT `/api/cards/{cardId}/international`
**Enable/Disable international transactions**

Controls whether the virtual card can be used for international transactions based on compliance and business requirements.

**Path**: cardId (Guid)
**Request**: Allow International (boolean)
**Response**: Success message
**Status**: 200 OK / 404 Not Found

---

## ?? Transaction Endpoints

### POST `/api/transactions/card/{cardId}`
**Create a new transaction for a virtual card**

Initiates a transaction on a specific virtual card. Creates a new transaction record in PENDING status that can be completed, reversed, or disputed later. The amount is automatically deducted from the user's main account balance.

**Path**: cardId (Guid)
**Request**: Amount (?), Merchant Name, Merchant Category Code (optional), Reference ID (optional)
**Response**: Transaction ID, Status, Amount in NGN, Merchant
**Status**: 201 Created / 400 Bad Request / 404 Not Found

---

### GET `/api/transactions/card/{cardId}`
**Get transaction history for a virtual card**

Retrieves all transactions for a specific card with pagination support. Returns transactions in chronological order with status, amount, merchant, and dates.

**Path**: cardId (Guid)
**Query Parameters**: pageNumber (default: 1), pageSize (default: 20, max: 100)
**Response**: Paginated list of transactions
**Status**: 200 OK

---

### GET `/api/transactions/{transactionId}`
**Get a specific transaction by ID**

Retrieves detailed information about a single transaction including amount, merchant, status, dispute information, and timestamps.

**Path**: transactionId (Guid)
**Response**: Complete transaction details
**Status**: 200 OK / 404 Not Found

---

### POST `/api/transactions/{transactionId}/complete`
**Complete a pending transaction**

Finalizes a pending transaction, moving it to COMPLETED status. This confirms the transaction has been processed and the account balance remains deducted.

**Path**: transactionId (Guid)
**Request**: Status (optional)
**Response**: Success message
**Status**: 200 OK / 404 Not Found

---

### POST `/api/transactions/{transactionId}/reverse`
**Reverse a transaction**

Cancels a transaction and reverses its amount back to the account. This removes the transaction from the balance calculation and returns funds to the main account balance.

**Path**: transactionId (Guid)
**Request**: Reversal Reason
**Response**: Success message with refund confirmation
**Status**: 200 OK / 404 Not Found

---

### POST `/api/transactions/{transactionId}/dispute`
**Dispute a transaction**

Files a dispute against a transaction (e.g., fraudulent activity, merchant error, or unauthorized use). Puts the transaction in DISPUTED status pending investigation. The dispute reason is recorded for investigation records.

**Path**: transactionId (Guid)
**Request**: Dispute Reason, Details (optional)
**Response**: Success message
**Status**: 200 OK / 404 Not Found

---

### GET `/api/transactions/card/{cardId}/summary`
**Get transaction summary and analytics for a card**

Provides a high-level overview of transactions for a card within a date range. Includes total amounts by status (completed, pending, reversed, failed) and breakdown by merchant. Useful for financial reporting and spending analysis. Defaults to last 30 days if dates not specified.

**Path**: cardId (Guid)
**Query Parameters**: startDate (optional), endDate (optional)
**Response**: Totals by status, Transaction count, Merchant breakdown, Currency
**Status**: 200 OK

---

## ?? Account Balance Endpoints

### GET `/api/accountbalance`
**Get the current account balance**

Retrieves the main account balance for the authenticated user including available funds, total funded amount, and total withdrawn amount. Returns the complete balance snapshot.

**Requires**: JWT Authentication
**Response**: Available Balance, Total Funded, Total Withdrawn, Currency, Creation Date
**Status**: 200 OK / 404 Not Found

---

### GET `/api/accountbalance/summary`
**Get account balance summary for dashboard**

Retrieves a quick summary of the account including available balance, total funded, total withdrawn, number of cards funded, and active transactions. Perfect for dashboard widgets.

**Requires**: JWT Authentication
**Response**: Available Balance, Total Funded, Total Withdrawn, Cards Funded Count, Active Transactions Count
**Status**: 200 OK

---

### POST `/api/accountbalance/fund`
**Fund the main account balance**

Adds funds to the main account balance. This is how money enters the system and becomes available for allocation to virtual cards. All funding transactions are logged and audited.

**Requires**: JWT Authentication
**Request**: Amount (?), Reason, Reference ID (optional)
**Response**: Updated account balance
**Status**: 200 OK / 400 Bad Request / 404 Not Found

---

### GET `/api/accountbalance/transactions`
**Get account transaction history**

Retrieves paginated transaction history for the account including all funding and withdrawal transactions. Can filter by transaction type (FUNDING, WITHDRAWAL). Shows balance before/after each transaction.

**Requires**: JWT Authentication
**Query Parameters**: pageNumber (default: 1), pageSize (default: 20), transactionType (optional: FUNDING/WITHDRAWAL)
**Response**: Paginated list of account transactions with balance snapshots
**Status**: 200 OK

---

### GET `/api/accountbalance/transactions/date-range`
**Get transaction history for a date range**

Retrieves all account transactions within a specified date range. Useful for reporting, compliance, and period-based analysis.

**Requires**: JWT Authentication
**Query Parameters**: startDate (ISO format: yyyy-MM-dd), endDate (ISO format: yyyy-MM-dd)
**Response**: List of transactions within date range
**Status**: 200 OK

---

## ? Approval Workflow Endpoints

### POST `/api/approvals`
**Request approval for a sensitive action**

Initiates an approval workflow for sensitive card operations (freeze, delete, change limits, etc). The approval is routed to the appropriate manager based on action type and role hierarchy. Approvals expire after 48 hours if not resolved.

**Path**: cardId (Guid)
**Request**: Action Type (FREEZE_CARD, DELETE_CARD, CHANGE_LIMITS, etc), Action Data (optional)
**Response**: Approval ID, Status (PENDING), Expiration Time
**Status**: 201 Created / 400 Bad Request

---

### GET `/api/approvals/pending`
**Get all pending approvals**

Retrieves all approval requests that are waiting for action. Only accessible by managers (CEO, CFO, Admin). Shows approvals from all cards and team members. Sorted by creation date, with oldest requests shown first (FIFO processing).

**Requires**: CEO, CFO, or Admin Role
**Response**: List of pending approval requests
**Status**: 200 OK

---

### GET `/api/approvals/{approvalId}`
**Get a specific approval request by ID**

Retrieves detailed information about a single approval request including action type, status, requested by, approval comments, and decision timestamps.

**Path**: approvalId (Guid)
**Response**: Complete approval request details
**Status**: 200 OK / 404 Not Found

---

### PUT `/api/approvals/{approvalId}/approve`
**Approve an approval request**

Authorizes a pending approval request, allowing the requested action to proceed. Only authorized managers can approve based on action type. The approver's comment is recorded for the audit trail.

**Requires**: CEO, CFO, or Admin Role
**Path**: approvalId (Guid)
**Request**: Approval Comment (optional)
**Response**: Success message
**Status**: 200 OK / 404 Not Found

---

### PUT `/api/approvals/{approvalId}/reject`
**Reject an approval request**

Denies a pending approval request, preventing the requested action from proceeding. The rejection reason is recorded for audit and to inform the requester why their action was not approved.

**Requires**: CEO, CFO, or Admin Role
**Path**: approvalId (Guid)
**Request**: Rejection Reason
**Response**: Success message
**Status**: 200 OK / 404 Not Found

---

### GET `/api/approvals/card/{cardId}/history`
**Get approval history for a specific card**

Retrieves all approval requests (past and present) associated with a particular card. Includes approved, rejected, and pending requests. Useful for compliance audits and understanding card action history.

**Path**: cardId (Guid)
**Response**: Complete approval history for the card
**Status**: 200 OK

---

### GET `/api/approvals/requirements/{actionType}`
**Get approval requirements for an action type**

Shows what role is required to approve a specific action and whether approval is needed at all. Use this to inform users what approval level is needed before submitting a request.

**Path**: actionType (string: FREEZE_CARD, DELETE_CARD, CHANGE_LIMITS, etc)
**Response**: Required Role, Is Required, Description, Max Duration
**Status**: 200 OK

---

## ?? Audit & Compliance Endpoints

### GET `/api/audit`
**Get all audit logs with filtering and pagination**

Retrieves audit logs for all system operations with support for filtering by action, resource, and date range. Useful for compliance audits and security investigations. Returns paginated results.

**Requires**: CEO, CFO, Admin, or Auditor Role
**Query Parameters**: pageNumber, pageSize, action (filter), resource (filter), startDate, endDate
**Response**: Paginated list of audit logs
**Status**: 200 OK

---

### GET `/api/audit/user/{userId}`
**Get audit logs for a specific user**

Retrieves all audit log entries related to actions performed by or affecting a specific user. Useful for user activity tracking and compliance verification.

**Requires**: CEO, CFO, Admin, or Auditor Role
**Path**: userId (Guid)
**Response**: List of audit logs for the specified user
**Status**: 200 OK

---

### GET `/api/audit/export`
**Export audit logs in specified format**

Exports filtered audit logs to CSV or JSON format for external processing, reporting, or archival. Defaults to last 30 days if date range not specified.

**Requires**: CEO, CFO, Admin, or Auditor Role
**Query Parameters**: format (csv/json), startDate (optional), endDate (optional)
**Response**: File download in requested format
**Status**: 200 OK (file download)

---

### POST `/api/audit/departments`
**Create a new department**

Establishes a new corporate department with assigned name, budget allocation, and manager. Only CEO can create departments. Returns department details upon successful creation.

**Requires**: CEO Role
**Request**: Department Name, Budget, Manager ID (optional)
**Response**: Created department details with ID
**Status**: 201 Created / 400 Bad Request

---

### GET `/api/audit`
**Get all departments with pagination**

Retrieves paginated list of all corporate departments with their details and user counts. Useful for organizational overview and department-level reporting.

**Query Parameters**: pageNumber (default: 1), pageSize (default: 20)
**Response**: Paginated list of departments
**Status**: 200 OK

---

### GET `/api/audit/{departmentId}`
**Get a specific department by ID**

Retrieves detailed information for a single department including budget, manager, and user count.

**Path**: departmentId (Guid)
**Response**: Department details
**Status**: 200 OK / 404 Not Found

---

### PUT `/api/audit/{departmentId}`
**Update a department**

Modifies department information such as name, budget allocation, manager assignment, and status. Only CEO can update departments. Partial updates are supported.

**Requires**: CEO Role
**Path**: departmentId (Guid)
**Request**: Name (optional), Budget (optional), Manager ID (optional), Status (optional)
**Response**: Updated department details
**Status**: 200 OK / 404 Not Found

---

### DELETE `/api/audit/{departmentId}`
**Delete a department**

Permanently removes a department from the system. Only CEO can delete departments. This action is irreversible and may affect associated users and cards.

**Requires**: CEO Role
**Path**: departmentId (Guid)
**Status**: 204 No Content / 404 Not Found

---

## ?? Response Format

All API responses follow a consistent JSON structure:

### Success Response
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "amount": 15000.00,
  "currency": "NGN",
  "status": "COMPLETED",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### Error Response
```json
{
  "code": "INSUFFICIENT_FUNDS",
  "message": "Insufficient account balance. Required: ?1000.00, Available: ?500.00"
}
```

### Paginated Response
```json
{
  "items": [
    { "id": "...", "amount": 15000, ... }
  ],
  "total": 150,
  "pageNumber": 1,
  "pageSize": 20
}
```

---

## ?? Authentication

All endpoints except `/api/auth/register` and `/api/auth/login` require:

```
Authorization: Bearer {JWT_TOKEN}
```

Tokens expire in 24 hours (1440 minutes). Request a new token using the login endpoint.

---

## ?? Currency

All monetary amounts are in **NGN (Nigerian Naira)** by default.

- **Symbol**: ?
- **Format**: ?15,000.00 (with thousand separators)
- **Display**: All API responses show amounts with currency code "NGN"

---

## ?? Timestamps

All timestamps are in **UTC** format (ISO 8601):
- Format: `2024-01-15T10:30:00Z`
- Use these for date filtering in queries

---

## ??? Role-Based Access

### Roles
- **APP** (Approver) - Can create and manage cards, approve workflows
- **VIEW** (Viewer) - Read-only access to most endpoints
- **CEO** - Full system access including approvals and department management
- **CFO** - Financial operations and approval authority
- **Admin** - System administration and compliance
- **Auditor** - Audit log access and reporting

---

**Last Updated**: 2024
**API Version**: 1.0
**Status**: ? Production Ready
