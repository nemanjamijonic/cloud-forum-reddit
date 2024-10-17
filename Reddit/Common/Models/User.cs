using Common.Enums;
using Common.Helper;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class User : TableEntity
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; } 
        public string LastName { get; set; } 
        public string Address { get; set; } 
        public string City { get; set; } 
        public string Country { get; set; } 
        public string PhoneNumber { get; set; } 
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public List<string> LikedPosts { get; set; }
        public List<string> DislikedPosts { get; set; }
        public User()
        {
            PartitionKey = "User";
            RowKey = Guid.NewGuid().ToString();
        }
    }

}
