using Shared.Kernel.Helpers;

namespace Stock.Api.Model
{
    public class Product
    {
        public long Id { get; private set; }
        public string Sku { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string? ImageUrl { get; private set; }
        public decimal Price { get; private set; }
        public int Quantity { get; private set; }
        public string Category { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private Product()
        {
            Sku = null!;
            Name = null!;
            Description = null!;
            Category = null!;
        }

        public Product(long id, string sku, string name, string description, string category, decimal price, int quantity, string? imageUrl = null)
        {
            Id = id;
            Sku = sku;
            Name = name;
            Description = description;
            Category = category;
            Price = price;
            Quantity = quantity;
            ImageUrl = imageUrl;
            IsActive = true;
            CreatedAt = BrazilTime.Now;
        }

        public void Update(string name, string description, string category, decimal price, string? imageUrl)
        {
            Name = name;
            Description = description;
            Category = category;
            Price = price;
            ImageUrl = imageUrl;
            UpdatedAt = DateTime.UtcNow;;
        }

        public void Inactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;;
        }

        public void DecreaseStock(int quantityToDecrease)
        {
            if (Quantity < quantityToDecrease)
            {
                throw new InvalidOperationException("Estoque insuficiente.");
            }
            Quantity -= quantityToDecrease;
        }
    }
}