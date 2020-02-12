using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Azure.Auth
{
    public struct Whoami
    {
        public const string SessionPropertyName = "session";
        [JsonProperty(PropertyName = SessionPropertyName)]
        public IRef<Session> session;

        public const string NamePropertyName = "name";
        [JsonProperty(PropertyName = NamePropertyName)]
        public string name { get; set; }

        public const string AccountPropertyName = "account";
        [JsonProperty(PropertyName = AccountPropertyName)]
        public Guid? account { get; set; }
    }
}
