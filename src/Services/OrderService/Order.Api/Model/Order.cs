namespace Order.Api.Model
{
    public class Order
    {
        public long Id { get; private set; }
        public long CustomerId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public decimal TotalPrice { get; private set; }
        public List<OrderItem> Items { get; private set; } = new List<OrderItem>();
        public OrderStatus Status { get; private set; }


        // Construtor vazio para o EF Core
        private Order()
        { 
            Status = OrderStatus.Pending;
        }

        public Order(long id, long customerId, List<OrderItem> items)
        {
            Id = id;
            CustomerId = customerId;
            OrderDate = DateTime.UtcNow;
            Items = items;
            CalculateTotalPrice();
            Status = OrderStatus.Pending;
        }

        private void CalculateTotalPrice()
        {
            TotalPrice = Items.Sum(item => item.UnitPrice * item.Quantity);
        }

        public void ConfirmOrder()
        {
            if (Status == OrderStatus.Pending)
            {
                Status = OrderStatus.Confirmed;
            }
        }
    }

    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }
}