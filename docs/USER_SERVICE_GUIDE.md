# User Service - Authentication & Authorization Guide

## Overview
The User Service provides JWT-based authentication and authorization for the microservices architecture.

## Features
- ✅ JWT Token Authentication
- ✅ Role-Based Authorization (Admin, User)
- ✅ User Registration
- ✅ User Login
- ✅ Password Hashing with BCrypt
- ✅ Token Validation
- ✅ User Management (Admin only)

## API Endpoints

### Public Endpoints (No Authentication Required)

#### 1. **Login**
```http
POST https://localhost:7003/api/auth/login
POST https://localhost:7100/gateway/auth/login (via Gateway)
```

**Request Body:**
```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "userId": 1,
    "username": "admin",
    "email": "admin@microservices.com",
    "firstName": "System",
    "lastName": "Administrator",
    "role": "Admin",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2026-02-15T17:00:00Z"
  }
}
```

#### 2. **Register**
```http
POST https://localhost:7003/api/auth/register
POST https://localhost:7100/gateway/auth/register (via Gateway)
```

**Request Body:**
```json
{
  "username": "newuser",
  "email": "newuser@example.com",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "firstName": "John",
  "lastName": "Smith"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "userId": 4,
    "username": "newuser",
    "email": "newuser@example.com",
    "firstName": "John",
    "lastName": "Smith",
    "role": "User",
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2026-02-15T17:00:00Z"
  }
}
```

### Protected Endpoints (Authentication Required)

#### 3. **Get Profile**
```http
GET https://localhost:7003/api/auth/profile
GET https://localhost:7100/gateway/auth/profile (via Gateway)
```

**Headers:**
```
Authorization: Bearer {your-token-here}
```

**Response:**
```json
{
  "success": true,
  "message": "Profile retrieved successfully",
  "data": {
    "id": 1,
    "username": "admin",
    "email": "admin@microservices.com",
    "firstName": "System",
    "lastName": "Administrator",
    "role": "Admin",
    "isActive": true,
    "createdAt": "2026-02-14T12:00:00Z",
    "lastLoginAt": "2026-02-14T16:30:00Z"
  }
}
```

### Admin Only Endpoints

#### 4. **Get All Users**
```http
GET https://localhost:7003/api/users
GET https://localhost:7100/gateway/users (via Gateway)
```

**Headers:**
```
Authorization: Bearer {admin-token-here}
```

**Response:**
```json
{
  "success": true,
  "message": "Retrieved 3 users",
  "data": [
    {
      "id": 1,
      "username": "admin",
      "email": "admin@microservices.com",
      "firstName": "System",
      "lastName": "Administrator",
      "role": "Admin",
      "isActive": true,
      "createdAt": "2026-02-14T12:00:00Z",
      "lastLoginAt": "2026-02-14T16:30:00Z"
    }
  ]
}
```

#### 5. **Get User by ID**
```http
GET https://localhost:7003/api/users/1
GET https://localhost:7100/gateway/users/1 (via Gateway)
```

#### 6. **Deactivate User**
```http
PUT https://localhost:7003/api/users/2/deactivate
PUT https://localhost:7100/gateway/users/2/deactivate (via Gateway)
```

#### 7. **Activate User**
```http
PUT https://localhost:7003/api/users/2/activate
PUT https://localhost:7100/gateway/users/2/activate (via Gateway)
```

## Default Users

### Admin User
```
Username: admin
Password: Admin@123
Role: Admin
```

### Test Users
```
Username: john.doe
Password: User@123
Role: User

Username: jane.smith
Password: User@123
Role: User
```

## Using JWT Token

1. **Login** to get your JWT token
2. **Copy the token** from the response
3. **Add to Headers** in subsequent requests:
   ```
   Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
   ```

## Token Configuration

**JWT Settings** (appsettings.json):
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForJWTTokenGeneration12345!@#$%^&*()",
    "Issuer": "MicroservicesDemo",
    "Audience": "MicroservicesDemo",
    "ExpirationHours": 24
  }
}
```

## Swagger UI with Authentication

1. Navigate to: `https://localhost:7003/swagger`
2. Click on **Authorize** button (top right)
3. Enter: `Bearer {your-token}` (include the word "Bearer")
4. Click **Authorize**
5. Now you can test protected endpoints

## Testing with Postman

### Step 1: Login
```http
POST https://localhost:7100/gateway/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

### Step 2: Save Token
Copy the `token` value from the response.

### Step 3: Use Token
Add header to requests:
```
Authorization: Bearer {paste-token-here}
```

### Step 4: Test Protected Endpoint
```http
GET https://localhost:7100/gateway/auth/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## Integration with Other Services

You can add JWT authentication to ProductService, OrderService, and PaymentService by:

1. **Install Package:**
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

2. **Configure in Program.cs:**
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = "MicroservicesDemo",
            ValidateAudience = true,
            ValidAudience = "MicroservicesDemo",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("YourSuperSecretKeyForJWTTokenGeneration12345!@#$%^&*()")
            )
        };
    });

app.UseAuthentication();
app.UseAuthorization();
```

3. **Add to Controllers:**
```csharp
[Authorize] // Requires any authenticated user
[Authorize(Roles = "Admin")] // Requires Admin role
```

## Error Responses

### 401 Unauthorized
```json
{
  "success": false,
  "message": "Invalid username or password",
  "statusCode": 401
}
```

### 400 Bad Request
```json
{
  "success": false,
  "message": "Username or email already exists",
  "statusCode": 400
}
```

### 403 Forbidden
```json
{
  "success": false,
  "message": "Access denied. Admin role required.",
  "statusCode": 403
}
```

## Database

**Database Name:** `UserServiceDb`  
**Table:** `Users`

**Schema:**
- Id (int, primary key)
- Username (varchar(100), unique, indexed)
- Email (varchar(100), unique, indexed)
- PasswordHash (varchar(500))
- Role (varchar(50), indexed)
- FirstName (varchar(100))
- LastName (varchar(100))
- IsActive (bit)
- CreatedAt (datetime)
- LastLoginAt (datetime, nullable)

## Security Best Practices

✅ Passwords are hashed using BCrypt  
✅ JWT tokens expire after 24 hours  
✅ HTTPS is enforced  
✅ Rate limiting on login endpoint (5 attempts per minute)  
✅ Role-based access control  
✅ Token validation on protected endpoints  

## Troubleshooting

### Cannot login
- Verify username and password are correct
- Check if user is active (IsActive = true)
- Ensure UserService is running on port 7003

### Token not working
- Verify token format: `Bearer {token}`
- Check token expiration
- Ensure JWT secret key matches across services

### 403 Forbidden
- Verify user role (Admin vs User)
- Check [Authorize] attribute requirements
- Ensure token includes role claim

## Next Steps

1. ✅ Start all services: `.\start-all-services.ps1`
2. ✅ Login to get JWT token
3. ✅ Test protected endpoints with token
4. ✅ Add authentication to other services (optional)
5. ✅ Build React frontend with login page

# Migrations

## Applying Migrations to All Services

# UserService
cd src\Services\UserService
dotnet ef database update

# ProductService
cd src\Services\ProductService
dotnet ef database update

# OrderService
cd src\Services\OrderService
dotnet ef database update

# PaymentService
cd src\Services\PaymentService
dotnet ef database update
