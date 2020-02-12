using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Azure.Functions
{
    public class InvocationMessage : IReferenceable
    {
        //public InvocationMessage()
        //{
        //    invocationRef = default;
        //    lastModified = default;
        //    requestUri = default;
        //    method = default;
        //    content = new byte[] { };
        //    lastExecuted = default;
        //}

        [JsonIgnore]
        public Guid id => this.invocationRef.id;

        public const string IdPropertyName = "id";
        [JsonProperty(PropertyName = IdPropertyName)]
        public IRef<InvocationMessage> invocationRef;

        [JsonProperty]
        public DateTimeOffset lastModified;

        [JsonProperty]
        public Uri requestUri;

        [JsonProperty]
        public string method;

        [JsonProperty]
        public IDictionary<string, string> headers;

        [JsonProperty]
        public byte[] content;

        public const string LastExecutedPropertyName = "last_executed";
        [JsonProperty(PropertyName = LastExecutedPropertyName)]
        public DateTime? lastExecuted;
    }
}
