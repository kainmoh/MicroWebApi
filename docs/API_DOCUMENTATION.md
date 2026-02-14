# ?? API Documentation

Complete API reference for all microservices.

## Base URLs

| Service | Direct URL | Via Gateway |
|---------|-----------|-------------|
| Product Service | https://localhost:7001 | https://localhost:7100/gateway/products |
| Order Service | https://localhost:7000 | https://localhost:7100/gateway/orders |
| Payment Service | https://localhost:7002 | https://localhost:7100/gateway/payments |

## Standard Response Format

All endpoints return responses in this format:

```json
{
  "success": true,
  "message": "Operation successful",
  "data": { },
  "statusCode": 200,
  "errors": [],
  "timestamp": "2026-02-13T10:30:00Z"
}
```

---

## ??? Product Service API

### Get All Products
```http
GET /api/products
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Laptop",
      "description": "High-performance gaming laptop",
      "price": 1500.00,
      "quantity": 50,
      "category": "Electronics",
      "createdAt": "2026-02-13T10:00:00Z"
    }
  ]
}
```

### Get Product by ID
```http
GET /api/products/{id}
```

**Parameters:**
- `id` (path, required): Product ID

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "name": "Laptop",
    "description": "High-performance gaming laptop",
    "price": 1500.00,
    "quantity": 50,
    "category": "Electronics",
    "createdAt": "2026-02-13T10:00:00Z"
  }
}
```

**Error Response (404):**
```json
{
  "success": false,
  "message": "Product with ID 999 not found",
  "statusCode": 404,
  "errors": ["Product with ID 999 not found"]
}
```

### Create Product
```http
POST /api/products
```

**Request Body:**
```json
{
  "name": "Wireless Mouse",
  "description": "Ergonomic wireless mouse",
  "price": 25.00,
  "quantity": 200,
  "category": "Electronics"
}
```

**Response (201):**
```json
{
  "success": true,
  "message": "Product created successfully",
  "data": {
    "id": 6,
    "name": "Wireless Mouse",
    "description": "Ergonomic wireless mouse",
    "price": 25.00,
    "quantity": 200,
    "category": "Electronics",
    "createdAt": "2026-02-13T10:30:00Z"
  },
  "statusCode": 201
}
```

### Update Product
```http
PUT /api/products/{id}
```

**Parameters:**
- `id` (path, required): Product ID

**Request Body (all fields optional):**
```json
{
  "name": "Updated Laptop",
  "price": 1600.00,
  "quantity": 45
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Product updated successfully",
  "data": {
    "id": 1,
    "name": "Updated Laptop",
    "price": 1600.00,
    "quantity": 45,
    "description": "High-performance gaming laptop",
    "category": "Electronics",
    "createdAt": "2026-02-13T10:00:00Z"
  }
}
```

### Delete Product
```http
DELETE /api/products/{id}
```

**Parameters:**
- `id` (path, required): Product ID

**Response (200):**
```json
{
  "success": true,
  "message": "Product deleted successfully",
  "data": true
}
```

### Update Inventory (Internal Use)
```http
POST /api/products/update-inventory
```

**Request Body:**
```json
{
  "productId": 1,
  "quantity": 5
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Inventory updated successfully",
  "data": true
}
```

**Error Response - Insufficient Inventory (400):**
```json
{
  "success": false,
  "message": "Insufficient inventory for product Laptop. Available: 10, Required: 50",
  "statusCode": 400
}
```

### Rollback Inventory (Internal Use - Compensation)
```http
POST /api/products/rollback-inventory
```

**Request Body:**
```json
{
  "productId": 1,
  "quantity": 5
}
```

---

## ?? Order Service API

### Get All Orders
```http
GET /api/orders
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "productId": 1,
      "quantity": 2,
      "totalAmount": 3000.00,
      "status": "Completed",
      "createdAt": "2026-02-13T10:15:00Z",
      "completedAt": "2026-02-13T10:15:05Z"
    }
  ]
}
```

### Get Order by ID
```http
GET /api/orders/{id}
```

**Parameters:**
- `id` (path, required): Order ID

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "productId": 1,
    "quantity": 2,
    "totalAmount": 3000.00,
    "status": "Completed",
    "createdAt": "2026-02-13T10:15:00Z",
    "completedAt": "2026-02-13T10:15:05Z"
  }
}
```

### Create Order (Triggers Saga)
```http
POST /api/orders
```

**Request Body:**
```json
{
  "productId": 1,
  "quantity": 2
}
```

**Successful Response (201):**
```json
{
  "success": true,
  "message": "Order created and processed successfully",
  "data": {
    "id": 2,
    "productId": 1,
    "quantity": 2,
    "totalAmount": 3000.00,
    "status": "Completed",
    "createdAt": "2026-02-13T10:20:00Z",
    "completedAt": "2026-02-13T10:20:05Z"
  },
  "statusCode": 201
}
```

**Error Response - Payment Failed (503):**
```json
{
  "success": false,
  "message": "Order saga failed: Payment processing failed",
  "statusCode": 503,
  "errors": ["Payment processing failed"]
}
```

**Saga Flow Diagram:**
```
POST /api/orders
    ?
[Step 1] Fetch product details & calculate total
    ?
[Step 2] Create order (Status: Pending)
    ?
[Step 3] Update inventory ? Call Product Service
    ? (Success)
[Step 4] Update order status (Status: Ordered)
    ?
[Step 5] Process payment ? Call Payment Service
    ? (Success)
[Step 6] Update order status (Status: PaymentProcessed)
    ?
[Step 7] Complete order (Status: Completed)
    ?
? Return completed order

----- OR ON FAILURE -----

[Any Step Fails]
    ?
[Compensation] Rollback inventory (if updated)
    ?
[Compensation] Mark order as Failed
    ?
? Throw ServiceCommunicationException
```

### Update Order Status
```http
PUT /api/orders/{id}/status
```

**Parameters:**
- `id` (path, required): Order ID

**Request Body:**
```json
{
  "orderId": 1,
  "status": "Cancelled"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Order status updated successfully",
  "data": {
    "id": 1,
    "productId": 1,
    "quantity": 2,
    "totalAmount": 3000.00,
    "status": "Cancelled",
    "createdAt": "2026-02-13T10:15:00Z",
    "completedAt": null
  }
}
```

**Valid Status Values:**
- `Pending`
- `Ordered`
- `PaymentProcessed`
- `Completed`
- `Failed`
- `Cancelled`

### Delete Order
```http
DELETE /api/orders/{id}
```

**Parameters:**
- `id` (path, required): Order ID

**Response (200):**
```json
{
  "success": true,
  "message": "Order deleted successfully",
  "data": true
}
```

---

## ?? Payment Service API

### Get All Payments
```http
GET /api/payments
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "orderId": 1,
      "amount": 3000.00,
      "paymentMethod": "CreditCard",
      "status": "PaymentDone",
      "transactionId": "TXN-20260213102015-A1B2C3D4",
      "createdAt": "2026-02-13T10:20:15Z",
      "processedAt": "2026-02-13T10:20:15Z"
    }
  ]
}
```

### Get Payment by ID
```http
GET /api/payments/{id}
```

**Parameters:**
- `id` (path, required): Payment ID

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "orderId": 1,
    "amount": 3000.00,
    "paymentMethod": "CreditCard",
    "status": "PaymentDone",
    "transactionId": "TXN-20260213102015-A1B2C3D4",
    "createdAt": "2026-02-13T10:20:15Z",
    "processedAt": "2026-02-13T10:20:15Z"
  }
}
```

### Get Payments by Order ID
```http
GET /api/payments/order/{orderId}
```

**Parameters:**
- `orderId` (path, required): Order ID

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": 1,
      "orderId": 1,
      "amount": 3000.00,
      "paymentMethod": "CreditCard",
      "status": "PaymentDone",
      "transactionId": "TXN-20260213102015-A1B2C3D4",
      "createdAt": "2026-02-13T10:20:15Z",
      "processedAt": "2026-02-13T10:20:15Z"
    }
  ]
}
```

### Create Payment (Without Processing)
```http
POST /api/payments
```

**Request Body:**
```json
{
  "orderId": 1,
  "amount": 3000.00,
  "paymentMethod": "CreditCard"
}
```

**Response (201):**
```json
{
  "success": true,
  "message": "Payment created successfully",
  "data": {
    "id": 2,
    "orderId": 1,
    "amount": 3000.00,
    "paymentMethod": "CreditCard",
    "status": "Pending",
    "transactionId": "TXN-20260213103000-E5F6G7H8",
    "createdAt": "2026-02-13T10:30:00Z",
    "processedAt": null
  },
  "statusCode": 201
}
```

### Process Payment (Internal Use - Called by Order Service)
```http
POST /api/payments/process
```

**Request Body:**
```json
{
  "orderId": 1,
  "amount": 3000.00,
  "paymentMethod": "CreditCard"
}
```

**Successful Response (200) - 95% chance:**
```json
{
  "success": true,
  "message": "Payment processed successfully",
  "data": {
    "id": 1,
    "orderId": 1,
    "amount": 3000.00,
    "paymentMethod": "CreditCard",
    "status": "PaymentDone",
    "transactionId": "TXN-20260213102015-A1B2C3D4",
    "createdAt": "2026-02-13T10:20:15Z",
    "processedAt": "2026-02-13T10:20:15Z"
  }
}
```

**Failed Response (402) - 5% chance:**
```json
{
  "success": false,
  "message": "Payment processing failed: Card declined",
  "statusCode": 402,
  "errors": ["Payment processing failed: Card declined"]
}
```

**Possible Failure Reasons (simulated):**
- Insufficient funds
- Card declined
- Invalid card number
- Card expired
- Gateway timeout

**Valid Payment Methods:**
- `CreditCard`
- `DebitCard`
- `PayPal`
- `BankTransfer`

**Valid Payment Status:**
- `Pending`
- `PaymentDone`
- `Failed`
- `Refunded`

### Delete Payment
```http
DELETE /api/payments/{id}
```

**Parameters:**
- `id` (path, required): Payment ID

**Response (200):**
```json
{
  "success": true,
  "message": "Payment deleted successfully",
  "data": true
}
```

---

## ?? API Gateway Routes

All endpoints can be accessed through the API Gateway:

### Product Endpoints
- `GET /gateway/products` ? `Product Service: GET /api/products`
- `GET /gateway/products/{id}` ? `Product Service: GET /api/products/{id}`
- `POST /gateway/products` ? `Product Service: POST /api/products`
- `PUT /gateway/products/{id}` ? `Product Service: PUT /api/products/{id}`
- `DELETE /gateway/products/{id}` ? `Product Service: DELETE /api/products/{id}`

### Order Endpoints
- `GET /gateway/orders` ? `Order Service: GET /api/orders`
- `GET /gateway/orders/{id}` ? `Order Service: GET /api/orders/{id}`
- `POST /gateway/orders` ? `Order Service: POST /api/orders`
- `PUT /gateway/orders/{id}/status` ? `Order Service: PUT /api/orders/{id}/status`
- `DELETE /gateway/orders/{id}` ? `Order Service: DELETE /api/orders/{id}`

### Payment Endpoints
- `GET /gateway/payments` ? `Payment Service: GET /api/payments`
- `GET /gateway/payments/{id}` ? `Payment Service: GET /api/payments/{id}`
- `GET /gateway/payments/order/{orderId}` ? `Payment Service: GET /api/payments/order/{orderId}`
- `POST /gateway/payments` ? `Payment Service: POST /api/payments`
- `DELETE /gateway/payments/{id}` ? `Payment Service: DELETE /api/payments/{id}`

### Gateway Features
- **Circuit Breaker**: 3 failures open circuit for 30 seconds
- **Timeout**: 10 seconds per request
- **Rate Limiting**:
  - Products: 10 requests/second
  - Orders: 5 requests/second
  - Payments: No limit

---

## ?? Error Codes

| Status Code | Meaning | Example |
|-------------|---------|---------|
| 200 | Success | Resource retrieved/updated |
| 201 | Created | Resource created |
| 400 | Bad Request | Invalid input, insufficient inventory |
| 402 | Payment Required | Payment failed |
| 404 | Not Found | Resource doesn't exist |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Unexpected error |
| 503 | Service Unavailable | Service communication failed, circuit open |

---

## ?? Authentication (Future Enhancement)

Currently, no authentication is required. In production, add:

```http
Authorization: Bearer {jwt_token}
```

---

## ?? Examples

### Complete Order Flow Example

**Step 1: Check available products**
```bash
curl https://localhost:7100/gateway/products
```

**Step 2: Create order**
```bash
curl -X POST https://localhost:7100/gateway/orders \
  -H "Content-Type: application/json" \
  -d '{"productId": 1, "quantity": 2}'
```

**Step 3: Verify order**
```bash
curl https://localhost:7100/gateway/orders/1
```

**Step 4: Check payment**
```bash
curl https://localhost:7100/gateway/payments/order/1
```

### Test Failure Scenario

**Attempt to order more than available:**
```bash
curl -X POST https://localhost:7100/gateway/orders \
  -H "Content-Type: application/json" \
  -d '{"productId": 1, "quantity": 1000}'
```

**Expected Response (503):**
```json
{
  "success": false,
  "message": "Order saga failed: Failed to update inventory: Insufficient inventory",
  "statusCode": 503
}
```

---

For more examples, import the Postman collection from the repository root.
