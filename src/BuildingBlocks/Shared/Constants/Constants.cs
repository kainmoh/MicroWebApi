namespace Shared.Constants;

/// <summary>
/// Order status constants
/// </summary>
public static class OrderStatus
{
    public const string Pending = "Pending";
    public const string Ordered = "Ordered";
    public const string PaymentProcessed = "PaymentProcessed";
    public const string Completed = "Completed";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
}

/// <summary>
/// Payment status constants
/// </summary>
public static class PaymentStatus
{
    public const string Pending = "Pending";
    public const string PaymentDone = "PaymentDone";
    public const string Failed = "Failed";
    public const string Refunded = "Refunded";
}

/// <summary>
/// Payment method constants
/// </summary>
public static class PaymentMethod
{
    public const string CreditCard = "CreditCard";
    public const string DebitCard = "DebitCard";
    public const string PayPal = "PayPal";
    public const string BankTransfer = "BankTransfer";
}
