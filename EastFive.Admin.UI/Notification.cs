using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EastFive.Admin.UI.Resources
{
    public class Notification
    {
        public const string IdPropertyName = "id";
        [JsonProperty(PropertyName = IdPropertyName)]
        public Guid id;

        public const string TitlePropertyName = "title";
        [JsonProperty(PropertyName = TitlePropertyName)]
        public string title;

        public const string BodyPropertyName = "body";
        [JsonProperty(PropertyName = BodyPropertyName)]
        public string body;

        public const string LinkPropertyName = "link";
        [JsonProperty(PropertyName = LinkPropertyName)]
        public string link;

        public const string LastDayPropertyName = "last_day";
        [JsonProperty(PropertyName = LastDayPropertyName)]
        public DateTime lastDay;
    }
}
