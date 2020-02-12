using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Azure.Auth
{
    public struct Token
    {
        public const string JwtPropertyName = "jwt";
        [JsonProperty(PropertyName = JwtPropertyName)]
        public string jwt;

        public const string IssuerPropertyName = "issuer";
        [JsonProperty(PropertyName = IssuerPropertyName)]
        public string issuer;

        public const string ScopePropertyName = "scope";
        [JsonProperty(PropertyName = ScopePropertyName)]
        public string scope { get; set; }

        public const string ExpirationPropertyName = "expiration";
        [JsonProperty(PropertyName = ExpirationPropertyName)]
        public DateTime expiration;

        public const string SecretePropertyName = "secret";
        [JsonProperty(PropertyName = SecretePropertyName)]
        public string secret;

    }
}
