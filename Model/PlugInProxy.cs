using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageManager.Model
{
    public class PlugInProxy
    {
        public string Name { get; set; }
        public Guid PlugInGuid { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
    }
}
