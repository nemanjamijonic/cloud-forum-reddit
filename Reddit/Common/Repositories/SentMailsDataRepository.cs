using Common.Models;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class SentMailsDataRepository
    {
        private CloudStorageAccount _storageAccount;
        private CloudTable _table;

        public SentMailsDataRepository()
        {
            _storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("SentMailDataConnectionString"));
            CloudTableClient tableClient = new CloudTableClient(new Uri(_storageAccount.TableEndpoint.AbsoluteUri), _storageAccount.Credentials);
            _table = tableClient.GetTableReference("SentMailTable"); _table.CreateIfNotExists();
        }

        public async Task CreateNewNotificationGroup(SentMail newMail)
        {
            TableOperation insertOperation = TableOperation.Insert(newMail);
            await _table.ExecuteAsync(insertOperation);
        }

    }
}
