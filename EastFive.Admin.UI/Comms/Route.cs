using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Net.Http;
using EastFive.Extensions;
using EastFive.Linq;
using EastFive.Reflection;
using Newtonsoft.Json;

namespace EastFive.Api.Resources
{
    public class Route
    {
        public bool IsEntryPoint { get; set; }

        public string Name { get; set; }

        public Method[] Methods { get; set; }

        public Property[] Properties { get; set; }
    }
}
