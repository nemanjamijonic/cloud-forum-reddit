using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class HealthCheck : TableEntity
    {
        public DateTime CheckedAt { get; set; }
        public string ServiceUrl { get; set; }
        public string Status { get; set; }

        public HealthCheck()
        {
            RowKey = Guid.NewGuid().ToString();
            PartitionKey = "HealthCheck";
        }
    }
}
