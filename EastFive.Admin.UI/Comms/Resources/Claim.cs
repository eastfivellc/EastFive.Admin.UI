using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Azure.Auth
{
    public struct Claim
    {
        public const string NamePropertyName = "name";
        [JsonProperty(PropertyName = NamePropertyName)]
        public string name;

        public const string TypePropertyName = "type";
        [JsonProperty(PropertyName = TypePropertyName)]
        public string type;

        public const string ValuePropertyName = "value";
        [JsonProperty(PropertyName = ValuePropertyName)]
        public string value;
    }
}
