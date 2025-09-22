namespace Order.Api.Model
{
    public class OrderItem
    {
        public long Id { get; private set; }
        public long ProductId { get; private set; }
        public int Quantity { get; private set; }
        public decimal UnitPrice { get; private set; }
        
        // Chave estrangeira e propriedade de navegação
        public long OrderId { get; private set; }
        public virtual Order Order { get; private set; } = null!;

        private OrderItem() { }

        public OrderItem(long id, long productId, int quantity, decimal unitPrice)
        {
            Id = id;
            ProductId = productId;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }
}