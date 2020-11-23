using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEBReadinessTool
{
    public class GitTag
    {
        public Version Version { get; set; }
        public RepositoryTag Tag { get; set; }
    }
}
