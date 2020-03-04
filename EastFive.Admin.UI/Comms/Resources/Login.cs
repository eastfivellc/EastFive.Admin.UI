using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Azure.Auth
{
    public struct Login
    {
        public const string ActorPropertyName = "actor";
        [JsonProperty(PropertyName = ActorPropertyName)]
        public Guid actorId;

        public const string MethodPropertyName = "method";
        [JsonProperty(PropertyName = MethodPropertyName)]
        public string method;

        public const string NamePropertyName = "name";
        [JsonProperty(PropertyName = NamePropertyName)]
        public string name;

        public const string AuthorizationPropertyName = "authorization";
        [JsonProperty(PropertyName = AuthorizationPropertyName)]
        public IRef<Authorization> authorization;

        public const string WhenPropertyName = "when";
        [JsonProperty(PropertyName = WhenPropertyName)]
        public DateTime when;
    }
}
