using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class PostReactions : TableEntity
    {
        public Guid PostId { get; set; }
        public Guid UserId { get; set; }
        public bool Liked { get; set; }
        public bool Disliked { get; set; }

        public PostReactions()
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = "PostReactions";
        }
    }
}
