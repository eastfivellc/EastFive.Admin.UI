using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Azure.Storage
{
    public class StorageTable
    {
        public string name;

        public StorageProperty[] properties;
    }

    public class StorageProperty
    {
        public string name;

        public string type;
    }

    public class StorageRow
    {
        #region Properties

        [JsonProperty]
        public string rowKey;

        [JsonProperty]
        public string partitionKey;

        [JsonProperty]
        public IDictionary<string, object> properties;

        #endregion
    }

    public class TableInformation
    {
        public long total;
        public long mismatchedRowKeys;
        public long mismatchedPartitionKeys;
        public IDictionary<string, IDictionary<object, long>> properties;
        public IDictionary<string, PartitionSummary> partitions;
    }

    public class PartitionSummary
    {
        public long total;
        public IDictionary<string, IDictionary<object, long>> properties;
    }
}
