using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Comment : TableEntity
    {
        public string Description { get; set; }
        public Guid UserID { get; set; }
        public string Username { get; set; }
        public string PostID { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public int NumberOfLikes { get; set; }
        public int NumberOfDislikes { get; set; }
        public List<string> Likes { get; set; }
        public List<string> Dislikes { get; set; }


        public Comment()
        {

            PartitionKey = "Comment"; 
            RowKey = Guid.NewGuid().ToString();
        }
    }
}
