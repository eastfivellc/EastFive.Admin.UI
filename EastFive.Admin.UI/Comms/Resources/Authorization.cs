using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Azure.Auth
{
    public struct Authorization : IReferenceable
    {
        #region Properties

        [JsonIgnore]
        public Guid id => authorizationRef.id;

        public const string AuthorizationIdPropertyName = "id";
        [JsonProperty(PropertyName = AuthorizationIdPropertyName)]
        public IRef<Authorization> authorizationRef;

        public const string MethodPropertyName = "method";
        [JsonProperty(PropertyName = MethodPropertyName)]
        public IRef<Method> Method { get; set; }

        public const string LocationAuthorizationPropertyName = "location_authentication";
        [JsonProperty(PropertyName = LocationAuthorizationPropertyName)]
        public Uri LocationAuthentication { get; set; }

        public const string LocationAuthorizationReturnPropertyName = "location_authentication_return";
        [JsonProperty(PropertyName = LocationAuthorizationReturnPropertyName)]
        public Uri LocationAuthenticationReturn { get; set; }

        public const string LocationLogoutPropertyName = "location_logout";
        [JsonProperty(PropertyName = LocationLogoutPropertyName)]
        public Uri LocationLogout { get; set; }

        public const string LocationLogoutReturnPropertyName = "location_logout_return";
        [JsonProperty(PropertyName = LocationLogoutReturnPropertyName)]
        public Uri LocationLogoutReturn { get; set; }

        #endregion
    }
}
