using SharpDom.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Admin.UI
{
    public class StringDataType : IDataType
    {
        public string Name { get; set; }

        public string entityId;

        public string entityName;

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
            var span = new TagSpan
            {
            };
            var xSpan = span["TEst"];
            var div = new TagDiv()
            {
                Id = $"{entityName}_{entityId}",
                Class = "rTableCell",
            };
            div.Children.Add(xSpan);
            rowEntry.Children.Add(div);
            return rowEntry;
        }

        public void Update(string editResults)
        {
            throw new NotImplementedException();
        }
    }
}
