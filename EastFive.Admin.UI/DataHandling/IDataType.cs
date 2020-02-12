using Newtonsoft.Json;
using SharpDom.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EastFive.Admin.UI
{
    public interface IDataType
    {
        TagDiv Render(TagDiv rowEntry);

        string Edit();

        void Update(string editResults);

        object GetValue();
    }
}
