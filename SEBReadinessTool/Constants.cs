using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEBReadinessTool
{
    public static class Constants
    {
        public static class Log
        {
            public static readonly string Folder = @"%localappdata%\SafeExamBrowser\Logs";
        }

        public static class Services
        { 
            public static readonly string SafeExamBrowser = nameof(SafeExamBrowser);
            public static readonly string WuauServ = nameof(WuauServ);

            public static class Status
            {
                public static readonly string Running = nameof(Running);
            }

            public static class StartMode
            {
                public static readonly string Auto = nameof(Auto);
            }
        }

        public static class Roles
        {
            public const string Administrators = @"BUILTIN\Administrators";
        }
    }
}
