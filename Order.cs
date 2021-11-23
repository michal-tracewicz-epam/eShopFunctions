using System.Collections.Generic;

namespace EShop.Orders
{
    class Order
    {
        public string id {get;set;}
        public string BuyerId { get; set; }
        public Addess ShipToAddress { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public decimal Total { get; set; }
    }
}