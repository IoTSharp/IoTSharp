using System;
using System.Collections.Generic;

namespace IoTSharp.App.View.UserList
{
    public class UserData
    {
        public static IEnumerable<User> GetUsers()
        {
            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Ekaterina Tankova",

                Email = "ekaterina.tankova@devias.io",

                Phone = "304-428-3097",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_3.png",

                CreatedAt = DateTime.Now.AddDays(-3),

                Address = new Address
                {
                    Country = "USA",

                    State = "West Virginia",

                    City = "Parkersburg",

                    Street = "2849 Fulton Street"
                }
            };

            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Cao Yu",

                Email = "cao.yu@devias.io",

                Phone = "712-351-5711",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_4.png",

                CreatedAt = DateTime.Now.AddDays(-5),

                Address = new Address
                {
                    Country = "USA",

                    State = "Bristow",

                    City = "Iowa",

                    Street = "1865  Pleasant Hill Road"
                }
            };

            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Alexa Richardson",

                Email = "alexa.richardson@devias.io",

                Phone = "770-635-2682",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_2.png",

                CreatedAt = DateTime.Now.AddDays(5),

                Address = new Address
                {
                    Country = "USA",

                    State = "Georgia",

                    City = "Atlanta",

                    Street = "4894  Lakeland Park Drive"
                }
            };

            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Anje Keizer",

                Email = "anje.keizer@devias.io",

                Phone = "908-691-3242",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_5.png",

                CreatedAt = DateTime.Now.AddDays(-2),

                Address = new Address
                {
                    Country = "USA",

                    State = "Ohio",

                    City = "Dover",

                    Street = "4158  Hedge Street"
                }
            };

            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Clarke Gillebert",

                CreatedAt = DateTime.Now.AddDays(-3),

                Address = new Address
                {
                    Country = "USA",

                    State = "Texas",

                    City = "Dallas",

                    Street = "75247"
                },

                Email = "clarke.gillebert@devias.io",

                Phone = "972-333-4106",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_6.png"
            };

            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Adam Denisov",

                CreatedAt = DateTime.Now.AddDays(-8),

                Address = new Address
                {
                    Country = "USA",

                    State = "California",

                    City = "Bakerfield",

                    Street = "317 Angus Road"
                },

                Email = "adam.denisov@devias.io",

                Phone = "858-602-3409",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_1.png"
            };

            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Ava Gregoraci",

                CreatedAt = DateTime.Now.AddDays(-1),

                Address = new Address
                {
                    Country = "USA",

                    State = "California",

                    City = "Redondo Beach",

                    Street = "2188  Armbrester Drive"
                },

                Email = "ava.gregoraci@devias.io",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_7.png",

                Phone = "415-907-2647"
            };

            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Emilee Simchenko",

                CreatedAt = DateTime.Now.AddDays(-6),

                Address = new Address
                {
                    Country = "USA",

                    State = "Nevada",

                    City = "Las Vegas",

                    Street = "1798  Hickory Ridge Drive"
                },

                Email = "emilee.simchenko@devias.io",

                Phone = "702-661-1654",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_8.png"
            };

            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Kwak Seong-Min",

                CreatedAt = DateTime.Now.AddDays(-10),

                Address = new Address
                {
                    Country = "USA",

                    State = "Michigan",

                    City = "Detroit",

                    Street = "3934  Wildrose Lane"
                },

                Email = "kwak.seong.min@devias.io",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_9.png",

                Phone = "313-812-8947"
            };

            yield return new User
            {
                Id = Guid.NewGuid().ToString(),

                Name = "Merrile Burgett",

                CreatedAt = DateTime.Now.AddDays(-20),

                Address = new Address
                {
                    Country = "USA",

                    State = "Utah",

                    City = "Salt Lake City",

                    Street = "368 Lamberts Branch Road"
                },

                Email = "merrile.burgett@devias.io",

                Phone = "801-301-7894",

                Avatar = "./_content/IoTSharp.App.View/images/avatars/avatar_10.png"
            };
        }
    }
}
