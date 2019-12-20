using EastFive.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EastFive.Api.Resources
{
    public class Property
    {
        public Property()
        {
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public KeyValuePair<string, string>[] Options { get; set; }

        public string Type { get; set; }

        public bool IsTitle { get; set; }

        public bool IsIdentfier { get; set; }
    }
}
