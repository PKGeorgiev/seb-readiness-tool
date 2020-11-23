using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Security.Permissions;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SEBReadinessTool
{
    public class SEBUtils
    {
        public SEBUtils()
        {

        }

        public ServiceStatus GetServiceStatus(string name)
        {
            ServiceStatus ss = new ServiceStatus();

            try
            {
                var query = String.Format(
                    @"SELECT State, StartMode FROM Win32_Service WHERE Name = '{0}'",
                    name);

                var querySearch = new ManagementObjectSearcher(query);

                var services = querySearch.Get();

                foreach (var service in services.Cast<ManagementObject>())
                {
                    ss.State = Convert.ToString(service.GetPropertyValue("State"));
                    ss.StartMode = Convert.ToString(service.GetPropertyValue("StartMode"));
                    ss.Success = true;
                    
                    break;
                };
            }
            catch { }

            return ss;
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        [PrincipalPermission(SecurityAction.Demand, Role = Constants.Roles.Administrators)]
        public void EnableService(string name)
        {
            Process sc = Process.Start("sc.exe", string.Format("config {0} start= auto", name));
            sc.WaitForExit();
        }

        [PrincipalPermission(SecurityAction.Demand, Role = Constants.Roles.Administrators)]
        public void StartService(string name)
        {
            ServiceStatus ss = GetServiceStatus(name);
            ServiceController sc = new ServiceController(name);

            if (!sc.ServiceHandle.IsInvalid)
            {
                sc.Start();

                sc.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMilliseconds(15000));
            }
        }

        [PrincipalPermission(SecurityAction.Demand, Role = Constants.Roles.Administrators)]
        public void FixService(string name)
        {
            ServiceStatus svc = GetServiceStatus(name);

            if (svc.Success && svc.State != Constants.Services.Status.Running)
            {
                if (svc.StartMode != Constants.Services.StartMode.Auto)
                {
                    EnableService(name);
                }

                StartService(name);
            }

        }
    }
}
