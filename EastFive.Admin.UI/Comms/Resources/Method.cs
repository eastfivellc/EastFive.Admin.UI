using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Azure.Auth
{
    public class Method : IReferenceable
    {
        [JsonIgnore]
        public Guid id => authenticationId.id;

        public const string AuthenticationIdPropertyName = "id";
        [JsonProperty(PropertyName = AuthenticationIdPropertyName)]
        public IRef<Method> authenticationId;

        public const string NamePropertyName = "name";
        [JsonProperty(PropertyName = NamePropertyName)]
        public string name;
    }
}
