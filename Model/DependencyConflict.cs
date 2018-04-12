using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageManager.Model
{
    public class DependencyConflict
    {
        public string ParentAssy { get; set; }
        public Guid ParentAssyGuid { get; set; }
        public string DependencyName { get; set; }
        public Version Version { get; set; }
        public Version ConflictingVersion { get; set; }
    }
}
