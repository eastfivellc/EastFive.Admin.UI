using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Azure.Auth
{
    public struct Session : IReferenceable
    {
        [JsonIgnore]
        public Guid id => sessionId.id;

        public const string SessionIdPropertyName = "id";
        [JsonProperty(PropertyName = SessionIdPropertyName)]
        public IRef<Session> sessionId;

        public const string AuthorizationPropertyName = "authorization";
        [JsonProperty(PropertyName = AuthorizationPropertyName)]
        public IRefOptional<Authorization> authorization { get; set; }

        public const string AccountPropertyName = "account";
        [JsonProperty(PropertyName = AccountPropertyName)]
        public Guid? account { get; set; }

        /// <summary>
        /// Determines if the session is authorized.
        /// </summary>
        /// <remarks>
        ///   The presence of .authorization is insufficent for this
        ///   determination because the referenced authorization may
        ///   not have been executed.
        /// </remarks>
        public const string AuthorizedTokenPropertyName = "authorized";
        [JsonProperty(PropertyName = AuthorizedTokenPropertyName)]
        public bool authorized;

        public const string HeaderNamePropertyName = "header_name";
        [JsonProperty(PropertyName = HeaderNamePropertyName)]
        public string HeaderName { get; set; }

        public const string TokenPropertyName = "token";
        [JsonProperty(PropertyName = TokenPropertyName)]
        public string token;

        public const string RefreshTokenPropertyName = "refresh_token";
        [JsonProperty(PropertyName = RefreshTokenPropertyName)]
        public string refreshToken;
    }
}
