# Payage – Payment Processing API

Technical assignment implementation for a RESTful payment processing system.

The API supports:
- Authorization
- Capture (partial & full)
- Void
- Refund (partial & full)
- Get transaction by id
- List transactions (with optional query filtering)

Built with:
- ASP.NET Core (.NET 8)
- PostgreSQL
- Dapper
- Vertical Slice Architecture
- Global exception handling middleware

---

## Architecture Overview

The solution follows **Vertical Slice Architecture**.

Each feature (Authorize, Capture, Void, Refund, etc.) contains:
- Request / Response models  
- Validator  
- Handler
- Repository  

No ORMs are used – only **raw SQL via Dapper**.
Database constraints act as a second safety layer in addition to application validation.

---

## Requirements

Local PostgreSQL
- PostgreSQL
- A database created manually

---

## Database Setup (Local)

1. Create database: 'payage'
2. Execute script:
	Database/Migrations/001_init.sql
   
---

## Configuration

Connection strings are stored in:
- appsettings.json
- appsettings.Development.json

Populate credentials from your database. Example:

"ConnectionStrings": {
  "Default": "Host=localhost;Port=5432;Database=payage;Username=postgres;Password=postgres"
}

---

## Run the application

1. Restore dependencies:
   dotnet restore

2. Start the API:
   dotnet run

3. Open Swagger UI in browser:
   https://localhost:5245/swagger

> Note: The port is defined in launchSettings.json.


## API Endpoints

Swagger UI is the primary way to explore and test the API.

- POST /api/v1/payments/authorize – Authorize a payment
- POST /api/v1/payments/{id}/capture – Capture a payment (partial/full)
- POST /api/v1/payments/{id}/void – Void an authorized payment
- POST /api/v1/payments/{id}/refund – Refund a captured payment
- GET /api/v1/payments/{id} – Get a payment with specified id
- GET /api/v1/payments?page={page}&pageSize={pageSize}&status={status}&orderReference={orderReference} - Get a list of payments with specified filters


## API Usage Examples

### Authorize
POST /api/v1/payments/authorize

Request example:
{
  "amount": 155,
  "currency": "EUR",
  "cardNumber": "4111111111111111",
  "cardholderName": "John",
  "expirationMonth": 5,
  "expirationYear": 2027,
  "cvv": "123",
  "orderReference": "1"
}

Response example:
{
  "id": "95e074e3-dd62-4d24-9a6a-6dd68e354b3a",
  "status": "AUTHORIZED",
  "amount": 155,
  "currency": "EUR",
  "maskedCardNumber": "411111******1111",
  "createdAt": "2026-02-09T09:07:16.5187785+00:00"
}

### Capture
POST /api/v1/payments/95e074e3-dd62-4d24-9a6a-6dd68e354b3a/capture

Request example:
{
  "amount": 155.00
}

Response example:
{
  "id": "95e074e3-dd62-4d24-9a6a-6dd68e354b3a",
  "status": "CAPTURED",
  "amount": 155,
  "currency": "EUR",
  "capturedAmount": 155,
  "updatedAt": "2026-02-09T09:11:56.205677+00:00"
}

### Void
POST /api/v1/payments/c2498046-15c0-4977-b5a7-c4613311ae79/void – Void an authorized payment

```
c2498046-15c0-4977-b5a7-c4613311ae79 is example id of the transaction with AUTHORIZED status
```

Response example:
{
  "id": "c2498046-15c0-4977-b5a7-c4613311ae79",
  "status": "VOIDED",
  "amount": 155,
  "currency": "EUR",
  "updatedAt": "2026-02-09T09:16:48.094669+00:00"
}

### Refund

POST /api/v1/payments/95e074e3-dd62-4d24-9a6a-6dd68e354b3a/refund

Request example:
{
  "amount": 155.00
}
	
Response body
{
  "id": "95e074e3-dd62-4d24-9a6a-6dd68e354b3a",
  "status": "REFUNDED",
  "amount": 155,
  "currency": "EUR",
  "capturedAmount": 155,
  "refundedAmount": 155,
  "updatedAt": "2026-02-09T09:22:28.0881+00:00"
}

### GetById
GET /api/v1/payments/{id} 

Request example:
/api/v1/payments/95e074e3-dd62-4d24-9a6a-6dd68e354b3a

Response example:
{
  "id": "95e074e3-dd62-4d24-9a6a-6dd68e354b3a",
  "status": "REFUNDED",
  "amount": 155,
  "currency": "EUR",
  "capturedAmount": 100,
  "refundedAmount": 100,
  "maskedCardNumber": "411111******1111",
  "cardholderName": "John",
  "createdAt": "2026-02-09T09:07:16.518778+00:00",
  "updatedAt": "2026-02-09T09:22:28.0881+00:00"
}


### List Transactions
GET /api/v1/payments

Request example:
/api/v1/payments?page=1&pageSize=10&status=CAPTURED

Response example:

"items": [
    {
      "id": "95e074e3-dd62-4d24-9a6a-6dd68e354b3a",
      "status": "CAPTURED",
      "amount": 155,
      "currency": "EUR",
      "orderReference": "1",
      "capturedAmount": 155,
      "refundedAmount": 0,
      "createdAt": "2026-02-09T09:07:16.518778+00:00",
      "updatedAt": "2026-02-09T09:11:56.205677+00:00"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 1,
  "hasNext": false
}


---

## Business Rules & Assumptions

The system models a simplified card payment lifecycle.

### Transaction states
- **AUTHORIZED** – funds are reserved but not yet captured.
- **CAPTURED** – funds have been collected.
- **VOIDED** – authorization was cancelled before capture.
- **REFUNDED** – captured funds have been fully returned.

### Allowed transitions
- AUTHORIZED → CAPTURED
- AUTHORIZED → VOIDED
- CAPTURED → REFUNDED

No other transitions are permitted.

---

## Error Handling

The API uses centralized exception handling implemented in middleware.

This ensures:
- consistent error responses  
- no duplication of error handling in controllers

### Error codes
400 - Validation error or invalid state transition
404 - Transaction not found
409 - Conflict (e.g., duplicate order reference)
500 - Unexpected system error