using ContosoShopEasy.Models;
using ContosoShopEasy.Data;

namespace ContosoShopEasy.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;

        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAllProducts();
        }

        public Product? GetProductById(int id)
        {
            return _productRepository.GetProductById(id);
        }

        public List<Product> GetProductsByCategory(int categoryId)
        {
            return _productRepository.GetProductsByCategory(categoryId);
        }

        // Maximum length allowed for a search term. Keeps queries bounded and
        // limits the surface area for abuse.
        private const int MaxSearchTermLength = 100;

        // Search products using a sanitized, length-bounded search term.
        // User input is never concatenated into a SQL query; the underlying
        // repository uses parameterized LINQ predicates, and the service layer
        // rejects input containing SQL control characters before forwarding it.
        public List<Product> SearchProducts(string searchTerm)
        {
            if (!TrySanitizeSearchTerm(searchTerm, out string sanitizedTerm))
            {
                Console.WriteLine("[WARNING] Search term rejected by input validation");
                return new List<Product>();
            }

            return _productRepository.SearchProducts(sanitizedTerm);
        }

        // Validate and sanitize a search term without concatenating it into SQL.
        // Returns false (rejecting the input) when the term is empty, too long,
        // or contains characters commonly used in SQL injection / control flow.
        private static bool TrySanitizeSearchTerm(string searchTerm, out string sanitizedTerm)
        {
            sanitizedTerm = string.Empty;

            if (string.IsNullOrWhiteSpace(searchTerm))
                return false;

            string trimmed = searchTerm.Trim();
            if (trimmed.Length == 0 || trimmed.Length > MaxSearchTermLength)
                return false;

            // Reject SQL control characters and common injection markers.
            // The repository already uses parameterized predicates, so this is
            // defense in depth rather than the primary mitigation.
            char[] dangerousChars = { '\'', '"', ';', '-', '*', '/', '%', '_', '[', ']', '\0' };
            if (trimmed.IndexOfAny(dangerousChars) >= 0)
                return false;

            // Reject control characters and angle brackets used in XSS payloads.
            if (trimmed.Any(c => char.IsControl(c) || c == '<' || c == '>'))
                return false;

            sanitizedTerm = trimmed;
            return true;
        }

        public List<Product> GetTopRatedProducts(int count = 10)
        {
            return _productRepository.GetAllProducts()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Rating)
                .Take(count)
                .ToList();
        }

        public List<Product> GetFeaturedProducts(int count = 5)
        {
            return _productRepository.GetAllProducts()
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .OrderByDescending(p => p.ReviewCount)
                .Take(count)
                .ToList();
        }

        public bool IsProductInStock(int productId, int quantity = 1)
        {
            var product = _productRepository.GetProductById(productId);
            return product != null && product.StockQuantity >= quantity;
        }

        public bool UpdateStock(int productId, int quantityChange)
        {
            var product = _productRepository.GetProductById(productId);
            if (product != null)
            {
                product.StockQuantity += quantityChange;
                product.LastModified = DateTime.UtcNow;
                return true;
            }
            return false;
        }
    }
}