using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EastFive.Api.Resources
{
    public class Parameter
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public bool Default { get; set; }
        public string Where { get; set; }
        public string Type { get; set; }
    }
}
