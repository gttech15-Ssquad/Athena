# Transaction API - NGN Currency Implementation

## Summary
All transaction APIs in the Virtupay Corporate API have been updated to use **NGN (Nigerian Naira)** as the default currency across all transaction operations.

## Changes Made

### 1. **TransactionDTOs.cs Updates**

#### CreateTransactionRequest
```csharp
public string Currency { get; set; } = "NGN";
```
- Default currency is now NGN
- Users can optionally specify a different currency if needed

#### TransactionSummaryResponse
```csharp
public string Currency { get; set; } = "NGN";
```
- Added `Currency` property defaulting to NGN
- All transaction summaries now include the currency code

### 2. **TransactionsController.cs Updates**

#### CreateTransaction Endpoint
- **Error Messages**: Updated to show currency as NGN with Naira symbol (?)
  ```
"Insufficient account balance. Required: ?1000.00, Available: ?500.00"
  ```
- **Audit Logs**: Transaction amounts logged as `?{Amount:N2}` format
- **Default**: All created transactions use NGN currency

#### CompleteTransaction Endpoint
- **Audit Logs**: Updated to log completion with NGN format
  - `"Amount: ?5000.00, Merchant: Nike Store"`

#### ReverseTransaction Endpoint
- **Audit Logs**: Updated to log reversals with NGN format
  - `"Amount: ?5000.00, Reason: Customer request"`

#### GetTransactionSummary Endpoint
- **Response**: Now explicitly includes `"Currency": "NGN"` in the response
- All amount fields (CompletedAmount, PendingAmount, etc.) are in NGN

### 3. **API Response Examples**

#### Create Transaction Response (201 Created)
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "cardId": "550e8400-e29b-41d4-a716-446655440001",
  "amount": 15000.00,
  "merchant": "Dangote Refinery",
  "merchantCategoryCode": "5411",
  "status": "COMPLETED",
  "currency": "NGN",
  "referenceId": "TXN-001",
  "createdAt": "2024-01-15T10:30:00Z",
  "completedAt": "2024-01-15T10:35:00Z"
}
```

#### Transaction Summary Response (GET /api/transactions/card/{cardId}/summary)
```json
{
  "currency": "NGN",
  "completedAmount": 150000.00,
  "pendingAmount": 25000.00,
  "reversedAmount": 5000.00,
  "failedAmount": 0.00,
  "transactionsByMerchant": {
    "Nike Store": 50000.00,
    "Dangote": 75000.00,
    "MTN": 25000.00
  }
}
```

### 4. **Error Messages (NGN Format)**

All error responses now display amounts in NGN with the Naira symbol:
- ? `"Required: ?1000.00"` (Good)
- ? ~~`"Required: $1000.00"`~~ (Old)

## API Endpoints Overview

| Method | Endpoint | Default Currency | Status |
|--------|----------|------------------|--------|
| POST | `/api/transactions/card/{cardId}` | NGN | ? Updated |
| GET | `/api/transactions/card/{cardId}` | NGN | ? (paginated list) |
| GET | `/api/transactions/{transactionId}` | NGN | ? Single transaction |
| POST | `/api/transactions/{transactionId}/complete` | NGN | ? Updated |
| POST | `/api/transactions/{transactionId}/reverse` | NGN | ? Updated |
| POST | `/api/transactions/{transactionId}/dispute` | NGN | ? Default |
| GET | `/api/transactions/card/{cardId}/summary` | NGN | ? Updated |

## Database Impact
- Existing transactions: No migration needed - currency stored as string in database
- New transactions: Automatically set to NGN if not explicitly provided

## Implementation Details

### Currency Symbol
- **Symbol**: ? (U+20A6)
- **Format**: `?{amount:N2}` (e.g., ?15,000.00)
- **Locale**: Nigerian Naira formatting with thousand separators

### Audit Trail
All transaction-related audit logs now include:
```csharp
$"Amount: ?{amount:N2}, Merchant: {merchant}"
```

Example audit entries:
- `TRANSACTION_CREATED: Amount: ?15000.00, Merchant: Nike Store`
- `TRANSACTION_COMPLETED: Amount: ?15000.00, Merchant: Nike Store`
- `TRANSACTION_REVERSED: Amount: ?15000.00, Reason: Customer request`

## Testing in Swagger

When testing transaction endpoints:
1. **Create Transaction**: Submit with `Currency: "NGN"` (or leave blank for default)
2. **View Response**: Currency will show as `"NGN"`
3. **View Summary**: Will include `"currency": "NGN"` in response

## Future Enhancements

To support multi-currency transactions:
1. Accept currency parameter in request: `CreateTransactionRequest { Currency: "USD" }`
2. Add currency conversion service
3. Store exchange rates at transaction time
4. Display converted amounts in summaries

## Build Status
? All builds successful after NGN implementation
