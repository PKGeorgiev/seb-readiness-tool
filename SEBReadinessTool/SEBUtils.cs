using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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

            // Checking service's status with WMI does not require admin priviledges
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

        private string GetUsername()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (identity.Name.IndexOf("\\") > 0)
            {
                return identity.Name.Split('\\')[1];
            }
            else if (identity.Name.IndexOf("@") > 0)
            {
                return identity.Name.Split('@')[0];
            }

            return identity.Name;
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

        private List<FileInfo> GetFiles(string path, string pattern = "*.*")
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);

                return dir.GetFiles(pattern).ToList();
            }

            return new List<FileInfo>();
        }

        public string ArchiveLogs()
        {
            var path = Environment.ExpandEnvironmentVariables(Constants.Log.Folder);
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var computerName = Environment.GetEnvironmentVariable("computername");
            var zipName = string.Format("SEB Logs_{0}_{1}_{2}.zip", computerName, GetUsername(), DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss"));
            var zipPath = Path.Combine(documentsPath, zipName);

            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }

            var files = GetFiles(path, "*.log")
                .OrderByDescending(ob => ob.CreationTime)
                .Take(9);

            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update))
            {
                foreach (var file in files)
                {

                    archive.CreateEntryFromFile(file.FullName, file.Name);
                }
            }

            return zipPath;
        }

        // https://stackoverflow.com/a/58017167
        public static bool IsSoftwareInstalled(string softwareName)
        {
            var registryUninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            var registryUninstallPathFor32BitOn64Bit = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

            if (Is32BitWindows())
                return GetSoftwareEntryInternal(softwareName, RegistryView.Registry32, registryUninstallPath) != null;

            var is64BitSoftwareInstalled = GetSoftwareEntryInternal(softwareName, RegistryView.Registry64, registryUninstallPath) != null;
            var is32BitSoftwareInstalled = GetSoftwareEntryInternal(softwareName, RegistryView.Registry64, registryUninstallPathFor32BitOn64Bit) != null;
            return is64BitSoftwareInstalled || is32BitSoftwareInstalled;
        }

        public static SoftwareEntry GetSoftwareEntry(string softwareName)
        {
            var registryUninstallPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            var registryUninstallPathFor32BitOn64Bit = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";

            if (Is32BitWindows())
                return GetSoftwareEntryInternal(softwareName, RegistryView.Registry32, registryUninstallPath);

            var is64BitSoftwareInstalled = GetSoftwareEntryInternal(softwareName, RegistryView.Registry64, registryUninstallPath);
            var is32BitSoftwareInstalled = GetSoftwareEntryInternal(softwareName, RegistryView.Registry64, registryUninstallPathFor32BitOn64Bit);

            if (is64BitSoftwareInstalled != null)
                return is64BitSoftwareInstalled;
            else
                return is32BitSoftwareInstalled;
        }

        private static bool Is32BitWindows() => Environment.Is64BitOperatingSystem == false;

        private static SoftwareEntry GetSoftwareEntryInternal(string softwareName, RegistryView registryView, string installedProgrammsPath)
        {
            var uninstallKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView)
                                                  .OpenSubKey(installedProgrammsPath);

            if (uninstallKey == null)
                return null;

            var uk = uninstallKey.GetSubKeyNames()
                               .Select(installedSoftwareString => uninstallKey.OpenSubKey(installedSoftwareString))
                               .Select(installedSoftwareKey => new SoftwareEntry()
                               {
                                   DisplayName = installedSoftwareKey.GetValue("DisplayName") as string,
                                   DisplayVersion = installedSoftwareKey.GetValue("DisplayVersion") as string,
                                   UninstallString = installedSoftwareKey.GetValue("UninstallString") as string
                               })
                               .Where(installedSoftwareName => installedSoftwareName.DisplayName != null && installedSoftwareName.DisplayName.Contains(softwareName))
                               .FirstOrDefault();

            Version version;
            if (uk != null && Version.TryParse(uk.DisplayVersion, out version))
            {
                uk.Version = version;
            }

            return uk;
        }
    }
}
