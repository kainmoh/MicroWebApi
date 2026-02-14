namespace Shared.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    
    public NotFoundException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown for bad request scenarios
/// </summary>
public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
    
    public BadRequestException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when service-to-service communication fails
/// </summary>
public class ServiceCommunicationException : Exception
{
    public ServiceCommunicationException(string message, Exception? innerException = null) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when there is insufficient inventory for an operation
/// </summary>
public class InsufficientInventoryException : Exception
{
    public InsufficientInventoryException(string message) : base(message) { }
    
    public InsufficientInventoryException(string message, Exception innerException) 
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when payment processing fails
/// </summary>
public class PaymentFailedException : Exception
{
    public PaymentFailedException(string message) : base(message) { }
    
    public PaymentFailedException(string message, Exception innerException) 
        : base(message, innerException) { }
}
