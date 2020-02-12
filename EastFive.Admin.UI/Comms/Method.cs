using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EastFive.Api.Resources
{
    public class Method
    {
        public string HttpMethod { get; set; }

        public string Name { get; set; }

        public Uri Path { get; set; }

        public string Description { get; set; }

        public Parameter[] Parameters { get; set; }

        public Response[] Responses { get; set; }
    }
}
