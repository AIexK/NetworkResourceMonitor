using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Management;
using ResourcesMonitor.Models;

namespace ResourcesMonitor.Utils
{
    public static class DiskUtils
    {
        public static List<DiskDataModel> GetDrivesListOfRemoteMachine(string hostName)
        {
            var result = new List<DiskDataModel>();
            var mainOptions = OptionsHelper.Options;

            var options = new ConnectionOptions();
            if (Environment.MachineName.ToLower() != hostName.ToLower())
            {
                options.Username = mainOptions.SystemUserLogin;
                options.Password = mainOptions.SystemUserPassword;
                options.EnablePrivileges = true;
                options.Impersonation = ImpersonationLevel.Impersonate;
            }

            var scope = new ManagementScope(@"\\" + hostName + @"\root\cimv2", options);
            var queryString = "select Name, Size, FreeSpace from Win32_LogicalDisk where DriveType=3";
            var query = new ObjectQuery(queryString);
            var worker = new ManagementObjectSearcher(scope, query);
            var disksInfoResults = worker.Get();
            foreach (ManagementObject item in disksInfoResults)
            {
                var name = (string)item["Name"];
                var freeSpace = (ulong)item["FreeSpace"];

                var size = (ulong)item["Size"];
                var localDiskInfo = new DiskDataModel()
                {
                    Name = name,
                    FreeSpace = freeSpace,
                    Size = size
                };
                result.Add(localDiskInfo);
            }
            worker.Dispose();
            return result;
        }
    }
}

