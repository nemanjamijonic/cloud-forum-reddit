using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class SentMail : TableEntity
    {
        public DateTime SentAt { get; set; }
        public string CommentID { get; set; }
        public int NumberOfSentMails { get; set; }

        public SentMail()
        {
            PartitionKey = "SentMail";
            RowKey = Guid.NewGuid().ToString();
        }

    }
}
