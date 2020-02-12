using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace EastFive.Api.Resources
{
    public class Response
    {
        public Response(ParameterInfo paramInfo)
        {
            this.Name = paramInfo.Name;
            this.StatusCode = System.Net.HttpStatusCode.OK;
            //this.Example = "TODO: JSON serialize response type";
            this.Headers = new KeyValuePair<string, string>[] { };
        }

        public Response()
        {
        }

        public string Name { get; set; }

        public System.Net.HttpStatusCode StatusCode { get; set; }

        public string Example { get; set; }

        public KeyValuePair<string, string>[] Headers { get; set; }
    }
}
