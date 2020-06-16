using System;
using System.Collections.Generic;

namespace IoTSharp.App.View.ProductList
{
    public class ProductData
    {
        public static IEnumerable<Product> GetProducts()
        {
            yield return new Product
            {
                Id = Guid.NewGuid().ToString(),

                Title = "Dropbox",

                Description = "Dropbox is a file hosting service that offers cloud storage, file synchronization, a personal cloud.",

                ImageUrl = "./_content/IoTSharp.App.View/images/products/product_1.png",

                TotalDownloads = 594,

                CreatedAt = DateTime.Now.AddDays(-3),

                UpdatedAt = DateTime.Now.AddDays(-3)
            };

            yield return new Product
            {
                Id = Guid.NewGuid().ToString(),

                Title = "Medium Corporation",

                Description = "Medium is an online publishing platform developed by Evan Williams, and launched in August 2012.",

                ImageUrl = "./_content/IoTSharp.App.View/images/products/product_2.png",

                TotalDownloads = 625,

                CreatedAt = DateTime.Now.AddDays(-5),

                UpdatedAt = DateTime.Now.AddDays(-5)
            };

            yield return new Product
            {
                Id = Guid.NewGuid().ToString(),

                Title = "Slack",

                Description = "Slack is a cloud-based set of team collaboration tools and services, founded by Stewart Butterfield.",

                ImageUrl = "./_content/IoTSharp.App.View/images/products/product_3.png",

                TotalDownloads = 857,

                CreatedAt = DateTime.Now.AddDays(-5),

                UpdatedAt = DateTime.Now.AddDays(-5)
            };

            yield return new Product
            {
                Id = Guid.NewGuid().ToString(),

                Title = "Lyft",

                Description = "Lyft is an on-demand transportation company based in San Francisco, California.",

                ImageUrl = "./_content/IoTSharp.App.View/images/products/product_4.png",

                TotalDownloads = 406,

                CreatedAt = DateTime.Now.AddDays(-2),

                UpdatedAt = DateTime.Now.AddDays(-2)
            };

            yield return new Product
            {
                Id = Guid.NewGuid().ToString(),

                Title = "GitHub",

                Description = "GitHub is a web-based hosting service for version control of code using Git.",

                ImageUrl = "./_content/IoTSharp.App.View/images/products/product_5.png",

                TotalDownloads = 835,

                CreatedAt = DateTime.Now.AddDays(-3),

                UpdatedAt = DateTime.Now.AddDays(-3)
            };

            yield return new Product
            {
                Id = Guid.NewGuid().ToString(),

                Title = "Squarespace",

                Description = "Squarespace provides software as a service for website building and hosting. Headquartered in NYC.",

                ImageUrl = "./_content/IoTSharp.App.View/images/products/product_6.png",

                TotalDownloads = 835,

                CreatedAt = DateTime.Now.AddDays(-8),

                UpdatedAt = DateTime.Now.AddDays(-8)
            };
        }
    }
}
