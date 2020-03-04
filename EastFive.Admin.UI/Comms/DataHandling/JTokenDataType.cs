using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace EastFive.Admin.UI
{
    public class JTokenDataType
    {
        public JToken token;
        public int index;

        public JTokenDataType(JToken token, int index)
        {
            this.token = token;
            this.index = index;
        }
    }
}
