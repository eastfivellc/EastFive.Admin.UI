using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EastFive.Admin.UI
{
    public class EntityType
    {
        public string Name { get; set; }

        public IDataType [] Properties { get; set; }
    }
}
