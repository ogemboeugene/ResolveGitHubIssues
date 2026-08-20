using ContosoShopEasy.Models;
using ContosoShopEasy.Data;

namespace ContosoShopEasy.Services
{
    public class PaymentService
    {
        // Security vulnerability: Hardcoded configuration values (but won't trigger GitHub Secret Scanning)
        private const string PAYMENT_GATEWAY_URL = "https://api.contoso-payments.com";
        private const string MERCHANT_NAME = "ContosoShopEasy";
        private const string GATEWAY_VERSION = "v2.1";

        private readonly OrderRepository _orderRepository;

        public PaymentService(OrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // Process a payment without ever persisting or logging the full PAN or CVV.
        // The raw card number is used only for validation and tokenization in memory;
        // only the last 4 digits, card type, and an opaque gateway token are stored.
        public bool ProcessPayment(string cardNumber, string cardHolderName, string expiryDate, string cvv, decimal amount)
        {
            // Mask the PAN for any logging: show only the last 4 digits.
            string maskedCard = MaskCardNumber(cardNumber);

            Console.WriteLine($"[DEBUG] Processing payment for card: {maskedCard}");
            Console.WriteLine($"[DEBUG] Card holder: {cardHolderName}");
            Console.WriteLine($"[DEBUG] Amount: ${amount}");

            if (!ValidateCardNumber(cardNumber))
            {
                Console.WriteLine($"[ERROR] Invalid card number: {maskedCard}");
                return false;
            }

            if (!ValidateExpiryDate(expiryDate))
            {
                Console.WriteLine("[ERROR] Invalid or expired expiry date");
                return false;
            }

            // Simulate payment processing
            Console.WriteLine("[INFO] Connecting to payment gateway...");
            Thread.Sleep(1000); // Simulate network delay

            string transactionId = GenerateTransactionId(cardNumber, amount);
            string cardToken = GenerateCardToken(cardNumber);
            string cardType = DetectCardType(cardNumber);
            string lastFour = GetLastFourDigits(cardNumber);

            // Store only PCI DSS compliant data: no full PAN, no CVV.
            var paymentInfo = new PaymentInfo
            {
                Method = PaymentMethod.CreditCard,
                CardLastFourDigits = lastFour,
                CardType = cardType,
                CardToken = cardToken,
                CardHolderName = cardHolderName,
                ExpiryDate = expiryDate,
                Amount = amount,
                ProcessedDate = DateTime.UtcNow,
                Status = PaymentStatus.Approved,
                TransactionId = transactionId
            };

            Console.WriteLine($"[SUCCESS] Payment processed successfully!");
            Console.WriteLine($"[DEBUG] Transaction ID: {transactionId}");
            Console.WriteLine($"[LOG] Payment completed - Card: {maskedCard}, Amount: ${amount}, Transaction: {transactionId}");

            return true;
        }

        // Return a masked representation of a card number (e.g., ****1234).
        // Never log the full PAN.
        private static string MaskCardNumber(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return "****";

            string digits = cardNumber.Replace(" ", "").Replace("-", "");
            if (digits.Length < 4)
                return "****";

            return "****" + digits.Substring(digits.Length - 4);
        }

        private static string GetLastFourDigits(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return string.Empty;

            string digits = cardNumber.Replace(" ", "").Replace("-", "");
            return digits.Length >= 4 ? digits.Substring(digits.Length - 4) : digits;
        }

        // Detect the card brand from the PAN prefix. Only the brand name is stored,
        // never the full number.
        private static string DetectCardType(string cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber))
                return "Unknown";

            string digits = cardNumber.Replace(" ", "").Replace("-", "");
            if (digits.Length == 0)
                return "Unknown";

            char first = digits[0];
            return first switch
            {
                '4' => "Visa",
                '5' => "Mastercard",
                '3' => digits.Length > 1 && (digits[1] == '4' || digits[1] == '7') ? "Amex" : "Unknown",
                '6' => "Discover",
                _ => "Unknown"
            };
        }

        // Generate an opaque token representing the card. In a real system this
        // would be returned by the payment gateway; here it is a deterministic
        // stand-in that does not reveal the PAN.
        private static string GenerateCardToken(string cardNumber)
        {
            string digits = cardNumber.Replace(" ", "").Replace("-", "");
            string lastFour = GetLastFourDigits(digits);
            string guid = Guid.NewGuid().ToString("N");
            return $"TOK_{lastFour}_{guid.Substring(0, 12)}";
        }

        // Vulnerable card validation
        private bool ValidateCardNumber(string cardNumber)
        {
            // Security vulnerability: Weak validation - only checks length
            if (string.IsNullOrEmpty(cardNumber))
                return false;

            // Remove spaces and dashes
            cardNumber = cardNumber.Replace(" ", "").Replace("-", "");

            // Security vulnerability: Accept any 13-19 digit number
            return cardNumber.Length >= 13 && cardNumber.Length <= 19 && cardNumber.All(char.IsDigit);
        }

        private bool ValidateExpiryDate(string expiryDate)
        {
            // Security vulnerability: Basic validation only
            if (string.IsNullOrEmpty(expiryDate) || !expiryDate.Contains("/"))
                return false;

            var parts = expiryDate.Split('/');
            if (parts.Length != 2)
                return false;

            if (int.TryParse(parts[0], out int month) && int.TryParse(parts[1], out int year))
            {
                if (year < 100) year += 2000; // Convert YY to YYYY
                var expiry = new DateTime(year, month, 1).AddMonths(1).AddDays(-1);
                return expiry >= DateTime.Now;
            }

            return false;
        }

        // Security vulnerability: Predictable transaction ID generation
        private string GenerateTransactionId(string cardNumber, decimal amount)
        {
            // Vulnerable: Using predictable pattern
            string lastFour = cardNumber.Length >= 4 ? cardNumber.Substring(cardNumber.Length - 4) : cardNumber;
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmm");
            string amountStr = amount.ToString("F2").Replace(".", "");
            
            return $"TXN_{timestamp}_{lastFour}_{amountStr}";
        }

        public bool RefundPayment(string transactionId, decimal amount)
        {
            // Security vulnerability: Log refund details
            Console.WriteLine($"[DEBUG] Processing refund for transaction: {transactionId}, Amount: ${amount}");
            Console.WriteLine($"[DEBUG] Using payment gateway: {PAYMENT_GATEWAY_URL}");

            // Simulate refund processing
            Console.WriteLine("[INFO] Processing refund...");
            Thread.Sleep(500);

            Console.WriteLine($"[SUCCESS] Refund processed for transaction: {transactionId}");
            return true;
        }

        // Method to get payment history - with security issues
        public List<PaymentInfo> GetPaymentHistory(int userId)
        {
            Console.WriteLine($"[DEBUG] Retrieving payment history for user: {userId}");
            
            // In a real app, this would query the database
            // For demo purposes, we'll return empty list
            return new List<PaymentInfo>();
        }
    }
}