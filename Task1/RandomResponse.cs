using Azure;
using Azure.Data.Tables;
using System;

namespace Task1
{
    public class RandomResponse : ITableEntity
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public bool Success { get; set; }

        public ETag ETag { get; set; }

        public RandomResponse()
        {
        }
    }
}
