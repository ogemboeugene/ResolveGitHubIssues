namespace ContosoShopEasy.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TotalAmount { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public Address? ShippingAddress { get; set; }
        public Address? BillingAddress { get; set; }
        public PaymentInfo? PaymentInfo { get; set; }
        public string? Notes { get; set; }
        public DateTime? ShippedDate { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public string? TrackingNumber { get; set; }

        public Order()
        {
            OrderNumber = string.Empty;
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Pending;
            OrderItems = new List<OrderItem>();
        }

        public Order(int id, int userId, string orderNumber)
        {
            Id = id;
            UserId = userId;
            OrderNumber = orderNumber;
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Pending;
            OrderItems = new List<OrderItem>();
        }
    }

    public enum OrderStatus
    {
        Pending = 1,
        Processing = 2,
        Shipped = 3,
        Delivered = 4,
        Cancelled = 5,
        Returned = 6
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }

        public OrderItem()
        {
        }

        public OrderItem(int id, int orderId, int productId, int quantity, decimal unitPrice)
        {
            Id = id;
            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
            TotalPrice = quantity * unitPrice;
        }
    }

    public class PaymentInfo
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public PaymentMethod Method { get; set; }

        // PCI DSS compliant fields: never store the full PAN or CVV.
        // CardLastFourDigits holds only the last 4 digits for display.
        public string CardLastFourDigits { get; set; }

        // CardType identifies the card brand (Visa, Mastercard, etc.) for display.
        public string CardType { get; set; }

        // CardToken is the opaque token returned by the payment gateway in place
        // of the real PAN. The raw card number is never persisted.
        public string CardToken { get; set; }

        public string CardHolderName { get; set; }
        public string ExpiryDate { get; set; }
        public decimal Amount { get; set; }
        public DateTime ProcessedDate { get; set; }
        public PaymentStatus Status { get; set; }
        public string? TransactionId { get; set; }

        public PaymentInfo()
        {
            CardLastFourDigits = string.Empty;
            CardType = string.Empty;
            CardToken = string.Empty;
            CardHolderName = string.Empty;
            ExpiryDate = string.Empty;
            ProcessedDate = DateTime.UtcNow;
            Status = PaymentStatus.Pending;
        }
    }

    public enum PaymentMethod
    {
        CreditCard = 1,
        DebitCard = 2,
        PayPal = 3,
        BankTransfer = 4
    }

    public enum PaymentStatus
    {
        Pending = 1,
        Approved = 2,
        Declined = 3,
        Refunded = 4
    }
}