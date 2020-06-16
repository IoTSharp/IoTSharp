using System;
using System.Collections.Generic;

namespace IoTSharp.App.View.OrderList
{
    public class OrderData
    {
        public static IEnumerable<Order> GetOrders()
        {
            yield return new Order
            {
                Id = Guid.NewGuid().ToString(),

                Ref = "CDD1049",

                Customer = "Ekaterina Tankova",

                Status = "pending",

                Amount = 30.5M,

                CreatedAt = DateTime.Now.AddDays(-12)
            };

            yield return new Order
            {
                Id = Guid.NewGuid().ToString(),

                Ref = "CDD1048",

                Customer = "Cao Yu",

                Status = "delivered",

                Amount = 25.1M,

                CreatedAt = DateTime.Now.AddDays(-3)
            };

            yield return new Order
            {
                Id = Guid.NewGuid().ToString(),

                Ref = "CDD1047",

                Customer = "Alexa Richardson",

                Status = "refunded",

                Amount = 10.99M,

                CreatedAt = DateTime.Now.AddDays(-1)
            };

            yield return new Order
            {
                Id = Guid.NewGuid().ToString(),

                Ref = "CDD1046",

                Customer = "Anje Keizer",

                Status = "pending",

                Amount = 96.43M,

                CreatedAt = DateTime.Now.AddDays(-6)
            };

            yield return new Order
            {
                Id = Guid.NewGuid().ToString(),

                Ref = "CDD1045",

                Customer = "Clarke Gillebert",

                Status = "delivered",

                Amount = 32.54M,

                CreatedAt = DateTime.Now.AddDays(-4)
            };

            yield return new Order
            {
                Id = Guid.NewGuid().ToString(),

                Ref = "CDD1044",

                Customer = "Adam Denisov",

                Status = "delivered",

                Amount = 16.76M,

                CreatedAt = DateTime.Now.AddDays(-8)
            };
        }
    }
}
