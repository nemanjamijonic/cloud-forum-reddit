using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class Notification : TableEntity
    {
        public Guid UserId { get; set; }
        public Guid PostId { get; set; }
        public bool IsNotificationOn { get; set; }

        public Notification()
        {
            PartitionKey = "Notification";
            RowKey = Guid.NewGuid().ToString();
        }
    }
}
