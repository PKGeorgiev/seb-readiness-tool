using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEBReadinessTool
{
    public class SoftwareEntry
    {
        public string DisplayName { get; set; }

        public string DisplayVersion { get; set; }

        public string UninstallString { get; set; }

        public Version Version { get; set; }
    }
}
