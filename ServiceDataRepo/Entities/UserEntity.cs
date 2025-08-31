using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceDataRepo.Entities
{
    public class UserEntity : TableEntity
    {
        public UserEntity() { }

        public UserEntity(string username)
        {
            PartitionKey = "User";
            RowKey = username;
            Username = username;
            CreatedAt = DateTime.UtcNow;
        }

        public string Name { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Gender { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
        public string ProfileImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
