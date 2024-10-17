using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class CommentReactions : TableEntity
    {
        public Guid CommentId { get; set; }
        public Guid UserId { get; set; }
        public bool Liked { get; set; }
        public bool Disliked { get; set; }

        public CommentReactions()
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = "CommentReactions";
        }
    }
}
