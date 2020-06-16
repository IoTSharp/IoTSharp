using System;

namespace IoTSharp.App.View.UserList
{
    public class Address
    {
        public string Country { set; get; }

        public string State { set; get; }

        public string City { set; get; }

        public string Street { set; get; }
    }

    public class User
    {
        public string Id { set; get; }

        public string Name { set; get; }

        public string Email { set; get; }

        public string Phone { set; get; }

        public string Avatar { set; get; }

        public DateTime CreatedAt { set; get; }

        public Address Address { set; get; }
    }
}
