using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Post : TableEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
        public Guid UserId { get; set; }
        public string PostedByUser { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsClosed { get; set; }
        public string ImageUrl { get; set; }
        public bool IsDeleted { get; set; }
        public int CommentsNumber { get; set; }
        public List<Comment> Comments { get; set; }

        public Post()
        {
            PartitionKey = "Post";
            RowKey = Guid.NewGuid().ToString();
            Comments = new List<Comment>();
        }
    }
}
