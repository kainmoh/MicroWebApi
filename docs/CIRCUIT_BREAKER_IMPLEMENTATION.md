# ?? Circuit Breaker Implementation Guide

## ?? Overview

Circuit breakers are implemented in **TWO locations** in this microservices architecture:

1. **Order Service** - Using Polly library for HTTP client calls
2. **API Gateway** - Using Ocelot's built-in QoS (Quality of Service) options

---

## ?? Location #1: Order Service (Polly Implementation)

### **File Location**
```
src/Services/OrderService/Program.cs (Lines 57-102)
```

### **Purpose**
Protects **Order Service** when making HTTP calls to:
- **Product Service** (Port 7001)
- **Payment Service** (Port 7002)

### **Implementation Details**

#### **1. Circuit Breaker Policy**
```csharp
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()  // Handles 5xx errors and network failures
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.RequestTimeout)
    .CircuitBreakerAsync(
        handledEventsAllowedBeforeBreaking: 5,  // Open after 5 failures
        durationOfBreak: TimeSpan.FromSeconds(30),  // Stay open for 30 seconds
        onBreak: (outcome, timespan) =>
        {
            Log.Warning("Circuit breaker opened for {Duration}s due to: {Exception}",
                timespan.TotalSeconds, outcome.Exception?.Message ?? "Policy result");
        },
        onReset: () =>
        {
            Log.Information("Circuit breaker reset");
        },
        onHalfOpen: () =>
        {
            Log.Information("Circuit breaker is half-open");
        }
    );
```

#### **Configuration Parameters**

| Parameter | Value | Description |
|-----------|-------|-------------|
| **Failure Threshold** | 5 failures | Circuit opens after 5 consecutive failures |
| **Break Duration** | 30 seconds | Circuit stays open for 30 seconds |
| **Handled Errors** | 5xx, Network timeouts, Request timeouts | Types of errors that count toward failures |

#### **States Explanation**

```
???????????????????????????????????????????????????????????????????
?                   CIRCUIT BREAKER STATES                        ?
???????????????????????????????????????????????????????????????????

[1] CLOSED (Normal Operation)
    ?
    All requests pass through
    Failures are counted
    ?
    IF 5 failures occur
    ?
[2] OPEN (Circuit Breaker Triggered)
    ?
    ? All requests FAIL IMMEDIATELY (no actual call made)
    ? Returns ServiceCommunicationException
    ?? Wait 30 seconds
    ?
[3] HALF-OPEN (Testing)
    ?
    ? Allow ONE test request through
    ?
    IF successful
        ? Circuit moves to CLOSED ?
    IF fails
        ? Circuit moves back to OPEN ? (wait another 30s)
```

#### **2. Retry Policy**
```csharp
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(
        retryCount: 3,  // Retry 3 times
        sleepDurationProvider: retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),  // Exponential backoff
        onRetry: (outcome, timespan, retryCount, context) =>
        {
            Log.Warning("Retry {RetryCount} after {Delay}s due to: {Exception}",
                retryCount, timespan.TotalSeconds, outcome.Exception?.Message ?? "Policy result");
        }
    );
```

#### **Retry Configuration**

| Attempt | Delay | Total Time |
|---------|-------|------------|
| 1st retry | 2 seconds | 2s |
| 2nd retry | 4 seconds | 6s |
| 3rd retry | 8 seconds | 14s |
| **After 3 failures** | **Circuit may open** | - |

#### **3. HTTP Client Configuration**

```csharp
// Product Service HTTP Client
builder.Services.AddHttpClient("ProductService")
    .AddPolicyHandler(retryPolicy)           // Applied FIRST
    .AddPolicyHandler(circuitBreakerPolicy)  // Applied SECOND
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

// Payment Service HTTP Client  
builder.Services.AddHttpClient("PaymentService")
    .AddPolicyHandler(retryPolicy)           // Applied FIRST
    .AddPolicyHandler(circuitBreakerPolicy)  // Applied SECOND
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));
```

#### **Policy Execution Order**

```
Request Flow:
    ?
[1] Retry Policy (Outer)
    ?
[2] Circuit Breaker Policy (Inner)
    ?
[3] HTTP Request to Service
    ?
[4] Response/Error
    ?
IF error:
    ? Circuit Breaker counts it
    ? Retry Policy may retry
    ? After max retries, error propagates
```

### **Usage in OrderSagaOrchestrator**

#### **Example: Calling Product Service**
```csharp
private async Task<ProductDto> GetProductDetailsAsync(int productId)
{
    var httpClient = _httpClientFactory.CreateClient("ProductService");
    
    // This call is protected by:
    // 1. Retry policy (3 retries with exponential backoff)
    // 2. Circuit breaker (opens after 5 failures)
    var response = await httpClient.GetAsync($"/api/products/{productId}");
    
    response.EnsureSuccessStatusCode();
    // ... rest of code
}
```

#### **What Happens on Failure**

```
Scenario 1: Transient Error (503, Network timeout)
    ?
[1] 1st Attempt fails
    ? Retry after 2 seconds
    ?
[2] 2nd Attempt fails
    ? Retry after 4 seconds
    ?
[3] 3rd Attempt fails
    ? Retry after 8 seconds
    ?
[4] 4th Attempt fails (max retries reached)
    ? Failure counted by Circuit Breaker (1/5)
    ? Exception thrown to caller
    ?
[Saga] Compensation triggered (rollback inventory)

Scenario 2: Circuit Breaker Opens
    ?
After 5 total failures across multiple requests:
    ?
[Circuit OPENS]
    ?
All subsequent requests to that service:
    ? Fail immediately (no network call made)
    ? Throw BrokenCircuitException
    ?
Wait 30 seconds
    ?
[Circuit HALF-OPEN]
    ?
Next request is allowed through as test
    ?
IF successful ? Circuit CLOSED ?
IF fails ? Circuit OPEN again ?
```

### **Logging Output Examples**

```log
# Retry in action
[2024-02-15 10:15:32] WARN: Retry 1 after 2s due to: Connection refused
[2024-02-15 10:15:36] WARN: Retry 2 after 4s due to: Connection refused
[2024-02-15 10:15:44] WARN: Retry 3 after 8s due to: Connection refused

# Circuit breaker opens
[2024-02-15 10:16:00] WARN: Circuit breaker opened for 30s due to: Connection refused

# Circuit breaker half-open
[2024-02-15 10:16:30] INFO: Circuit breaker is half-open

# Circuit breaker reset
[2024-02-15 10:16:35] INFO: Circuit breaker reset
```

---

## ?? Location #2: API Gateway (Ocelot QoS)

### **File Location**
```
src/ApiGateway/ocelot.json (Lines 14-18, 37-41, etc.)
```

### **Purpose**
Protects **API Gateway** when routing requests to downstream services:
- Product Service
- Order Service  
- Payment Service

### **Implementation Details**

#### **QoS Configuration for Each Route**

```json
{
  "QoSOptions": {
    "ExceptionsAllowedBeforeBreaking": 3,  // Open after 3 failures
    "DurationOfBreak": 30000,              // Stay open for 30 seconds (ms)
    "TimeoutValue": 10000                  // Request timeout: 10 seconds (ms)
  }
}
```

#### **Configuration Parameters**

| Parameter | Value | Description |
|-----------|-------|-------------|
| **Failure Threshold** | 3 failures | Circuit opens after 3 consecutive failures |
| **Break Duration** | 30,000 ms (30s) | Circuit stays open for 30 seconds |
| **Request Timeout** | 10,000 ms (10s) | Maximum time to wait for response |

#### **Example Routes with Circuit Breaker**

##### **1. Product Service Routes**
```json
{
  "DownstreamPathTemplate": "/api/products",
  "DownstreamScheme": "https",
  "DownstreamHostAndPorts": [
    { "Host": "localhost", "Port": 7001 }
  ],
  "UpstreamPathTemplate": "/gateway/products",
  "UpstreamHttpMethod": [ "Get", "Post" ],
  "QoSOptions": {
    "ExceptionsAllowedBeforeBreaking": 3,
    "DurationOfBreak": 30000,
    "TimeoutValue": 10000
  },
  "RateLimitOptions": {
    "EnableRateLimiting": true,
    "Period": "1s",
    "Limit": 10  // Max 10 requests/second
  }
}
```

##### **2. Order Service Routes**
```json
{
  "DownstreamPathTemplate": "/api/orders",
  "DownstreamScheme": "https",
  "DownstreamHostAndPorts": [
    { "Host": "localhost", "Port": 7000 }
  ],
  "UpstreamPathTemplate": "/gateway/orders",
  "UpstreamHttpMethod": [ "Get", "Post" ],
  "QoSOptions": {
    "ExceptionsAllowedBeforeBreaking": 3,
    "DurationOfBreak": 30000,
    "TimeoutValue": 10000
  },
  "RateLimitOptions": {
    "EnableRateLimiting": true,
    "Period": "1s",
    "Limit": 5  // Max 5 requests/second
  }
}
```

##### **3. Payment Service Routes**
```json
{
  "DownstreamPathTemplate": "/api/payments",
  "DownstreamScheme": "https",
  "DownstreamHostAndPorts": [
    { "Host": "localhost", "Port": 7002 }
  ],
  "UpstreamPathTemplate": "/gateway/payments",
  "UpstreamHttpMethod": [ "Get", "Post" ],
  "QoSOptions": {
    "ExceptionsAllowedBeforeBreaking": 3,
    "DurationOfBreak": 30000,
    "TimeoutValue": 10000
  }
}
```

### **Behavior**

```
Client ? API Gateway ? Downstream Service
    ?
[1] Request arrives at gateway
    ?
[2] Gateway checks circuit breaker state
    ?
IF circuit is CLOSED:
    ? Forward request to downstream service
    ? Wait max 10 seconds for response
    ? IF timeout or error: count failure (x/3)
    ?
IF circuit is OPEN:
    ? Return 503 Service Unavailable immediately
    ? Log: "Quality of Service circuit breaker tripped"
    ?
IF circuit is HALF-OPEN:
    ? Allow ONE test request
    ? IF success: close circuit
    ? IF failure: open circuit again
```

### **Error Response**

When circuit is open at gateway level:

```json
{
  "errors": [
    "Quality of service circuit breaker tripped"
  ],
  "status": 503
}
```

---

## ?? Complete Circuit Breaker Flow

### **Scenario: Order Creation with Multiple Circuit Breakers**

```
????????????????????????????????????????????????????????????????????????
?                        CLIENT APPLICATION                            ?
????????????????????????????????????????????????????????????????????????
                                 ?
                                 ? POST /gateway/orders
                                 ?
????????????????????????????????????????????????????????????????????????
?                    API GATEWAY (Ocelot QoS)                          ?
?  ?????????????????????????????????????????????????????????????????? ?
?  ? Circuit Breaker #1 (Gateway Level)                             ? ?
?  ?   - Threshold: 3 failures                                      ? ?
?  ?   - Break: 30 seconds                                          ? ?
?  ?   - Timeout: 10 seconds                                        ? ?
?  ?????????????????????????????????????????????????????????????????? ?
????????????????????????????????????????????????????????????????????????
                                 ?
                                 ? Routes to OrderService
                                 ?
????????????????????????????????????????????????????????????????????????
?                      ORDER SERVICE                                   ?
?  ?????????????????????????????????????????????????????????????????? ?
?  ? OrderSagaOrchestrator                                          ? ?
?  ?????????????????????????????????????????????????????????????????? ?
?                                 ?                                    ?
?        ???????????????????????????????????????????????????          ?
?        ?                        ?                        ?          ?
?        ?                        ?                        ?          ?
?  ?????????????????      ?????????????????      ?????????????????  ?
?  ? HTTP Call to  ?      ? HTTP Call to  ?      ? HTTP Call to  ?  ?
?  ? Product Svc   ?      ? Product Svc   ?      ? Payment Svc   ?  ?
?  ? (Get Details) ?      ? (Update Inv)  ?      ? (Process)     ?  ?
?  ?????????????????      ?????????????????      ?????????????????  ?
?          ?                      ?                      ?          ?
?  ???????????????????????????????????????????????????????????????  ?
?  ? Circuit Breaker #2 (Polly - ProductService)                 ?  ?
?  ?   - Threshold: 5 failures                                   ?  ?
?  ?   - Break: 30 seconds                                       ?  ?
?  ?   - Retry: 3 times with exponential backoff                 ?  ?
?  ???????????????????????????????????????????????????????????????  ?
?                                                                    ?
?  ???????????????????????????????????????????????????????????????  ?
?  ? Circuit Breaker #3 (Polly - PaymentService)                 ?  ?
?  ?   - Threshold: 5 failures                                   ?  ?
?  ?   - Break: 30 seconds                                       ?  ?
?  ?   - Retry: 3 times with exponential backoff                 ?  ?
?  ???????????????????????????????????????????????????????????????  ?
????????????????????????????????????????????????????????????????????????
```

### **Layered Protection**

| Layer | Location | Threshold | Duration | Protects |
|-------|----------|-----------|----------|----------|
| **Layer 1** | API Gateway | 3 failures | 30s | Gateway from overloading services |
| **Layer 2** | OrderService ? ProductService | 5 failures | 30s | OrderService from Product failures |
| **Layer 3** | OrderService ? PaymentService | 5 failures | 30s | OrderService from Payment failures |

---

## ?? Comparison: Gateway vs Polly Circuit Breakers

| Feature | API Gateway (Ocelot) | Order Service (Polly) |
|---------|---------------------|----------------------|
| **Scope** | Per-route (gateway level) | Per-HTTP-client (service level) |
| **Failure Threshold** | 3 failures | 5 failures |
| **Break Duration** | 30 seconds | 30 seconds |
| **Retry Policy** | ? No built-in retry | ? 3 retries with backoff |
| **Timeout** | 10 seconds | No explicit timeout (relies on HttpClient) |
| **Rate Limiting** | ? Built-in (10/s products, 5/s orders) | ? Not implemented |
| **Logging** | Basic Ocelot logs | Detailed Serilog logs |
| **When Opens** | 3 consecutive failures from client to gateway to service | 5 consecutive failures from OrderService to downstream |
| **Impact** | Client gets 503 immediately | Saga fails, triggers compensation |

---

## ?? Testing Circuit Breakers

### **Test 1: Gateway Circuit Breaker**

```bash
# Stop ProductService to simulate failure
# Then make repeated requests through gateway

for i in {1..5}; do
  curl -X GET https://localhost:7100/gateway/products
  echo ""
  sleep 1
done
```

**Expected Behavior:**
- First 3 requests: Timeout after 10s (503 Service Unavailable)
- After 3rd failure: Circuit opens
- Subsequent requests: Fail immediately with "Quality of service circuit breaker tripped"
- After 30 seconds: Circuit half-opens, next request is test request

### **Test 2: Polly Circuit Breaker (OrderService)**

```bash
# Stop PaymentService
# Then create an order

curl -X POST https://localhost:7000/api/orders \
  -H "Content-Type: application/json" \
  -d '{"productId": 1, "quantity": 2}'
```

**Expected Log Output:**
```log
[10:15:00] WARN: Retry 1 after 2s due to: Connection refused (PaymentService)
[10:15:04] WARN: Retry 2 after 4s due to: Connection refused (PaymentService)
[10:15:12] WARN: Retry 3 after 8s due to: Connection refused (PaymentService)
[10:15:20] ERROR: Order saga failed: Failed to process payment
[10:15:20] INFO: Compensating transaction: Rolling back inventory for product 1
```

**After 5 such requests:**
```log
[10:18:00] WARN: Circuit breaker opened for 30s due to: Connection refused
[10:18:30] INFO: Circuit breaker is half-open
```

---

## ?? Configuration Tuning

### **When to Adjust Thresholds**

#### **Increase Failure Threshold** (More tolerant)
- High-latency networks
- Known occasional transient failures
- Non-critical services

```csharp
// More tolerant: 10 failures before opening
handledEventsAllowedBeforeBreaking: 10,
durationOfBreak: TimeSpan.FromSeconds(60)
```

#### **Decrease Failure Threshold** (More aggressive)
- Critical services
- Fast-fail requirements
- Production environments with SLA requirements

```csharp
// More aggressive: 2 failures before opening
handledEventsAllowedBeforeBreaking: 2,
durationOfBreak: TimeSpan.FromSeconds(15)
```

### **Recommended Settings by Environment**

| Environment | Gateway Threshold | Polly Threshold | Break Duration |
|-------------|------------------|-----------------|----------------|
| **Development** | 5 | 10 | 15s |
| **Staging** | 3 | 5 | 30s |
| **Production** | 3 | 3 | 60s |

---

## ?? Monitoring Circuit Breakers

### **Key Metrics to Track**

1. **Circuit Breaker Opens** - How often circuits open
2. **Break Duration Used** - Actual time circuits stay open
3. **Half-Open Test Results** - Success rate of test requests
4. **Retry Attempts** - How many retries before success/failure
5. **Failed Requests During Open** - Requests rejected while circuit open

### **Log Analysis Queries**

```bash
# Count circuit breaker opens today
grep "Circuit breaker opened" logs/order-service-*.txt | wc -l

# Find services causing most failures
grep "Circuit breaker opened" logs/order-service-*.txt | grep -o "ProductService\|PaymentService" | sort | uniq -c

# Average time before circuit closes
grep "Circuit breaker reset" logs/order-service-*.txt
```

---

## ?? Benefits Achieved

### **1. Fault Isolation**
? Failures in PaymentService don't cascade to ProductService  
? OrderService protected from downstream failures

### **2. Fast Fail**
? Clients get immediate errors instead of waiting for timeouts  
? Resources not wasted on failing calls

### **3. Self-Healing**
? Circuits automatically reset when services recover  
? No manual intervention needed

### **4. Graceful Degradation**
? System continues operating with limited functionality  
? Compensation transactions rollback partial work

---

## ?? References

- **Polly Documentation**: https://github.com/App-vNext/Polly
- **Ocelot QoS**: https://ocelot.readthedocs.io/en/latest/features/qualityofservice.html
- **Circuit Breaker Pattern**: https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker

---

## ? Summary

### **Circuit Breaker Locations:**
1. ? **API Gateway** (ocelot.json) - All routes
2. ? **OrderService** (Program.cs) - ProductService & PaymentService HTTP clients

### **Key Features:**
- ? Retry with exponential backoff (OrderService only)
- ? Automatic circuit opening on repeated failures
- ? Self-healing with half-open state
- ? Comprehensive logging
- ? Layered protection (Gateway + Service level)

---

**Last Updated:** 2024-02-15  
**Version:** 1.0
