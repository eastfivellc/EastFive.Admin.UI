using Newtonsoft.Json.Linq;
using SharpDom.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Admin.UI
{
    public class JTokenDataType : IDataType
    {
        JToken token;
        int index;

        public JTokenDataType(JToken token, int index)
        {
            this.token = token;
            this.index = index;
        }

        public string Edit()
        {
            throw new NotImplementedException();
        }

        public object GetValue()
        {
            throw new NotImplementedException();
        }

        public TagDiv Render(TagDiv rowEntry)
        {
            if (token.Type == JTokenType.String)
            {
                var jString = token.Value<string>();
                var span = new TagSpan
                {
                };
                var xSpan = span[jString];
                var div = new TagDiv()
                {
                    //Id = $"{entityName}_{entityId}",
                    Class = "rTableCell",
                };
                div.Children.Add(xSpan);
                rowEntry.Children.Add(div);
            }
            if (token.Type == JTokenType.Property)
            {
                var jProp = token as JProperty;
                var jString = jProp.Value.Value<string>();
                var span = new TagSpan
                {
                };
                var xSpan = span[jString];
                var div = new TagDiv()
                {
                    Id = $"{jProp.Name}_{index}",
                    Class = "rTableCell",
                };
                div.Children.Add(xSpan);
                rowEntry.Children.Add(div);
            }
            return rowEntry;
        }

        public void Update(string editResults)
        {
            throw new NotImplementedException();
        }
    }
}
