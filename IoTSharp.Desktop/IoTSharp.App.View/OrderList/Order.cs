using System;

namespace IoTSharp.App.View.OrderList
{
    public class Order
    {
        public string Id { set; get; }

        public string Ref { set; get; }

        public string Customer { set; get; }

        public decimal Amount { set; get; }

        public string Status { set; get; }

        public DateTime CreatedAt { set; get; }
    }
}
