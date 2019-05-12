using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Management;
using ResourcesMonitor.Models;

namespace ResourcesMonitor.Utils
{
    public class ServicesControl
    {
        private static Constants.ServiceState GetServiceStateByStateName(string stateName)
        {
            Constants.ServiceState result = Constants.ServiceState.Unknown;
            switch (stateName)
            {
                case "Running":
                    result = Constants.ServiceState.Running;
                    break;
                case "Stopped":
                    result = Constants.ServiceState.Stopped;
                    break;
                case "Paused":
                    result = Constants.ServiceState.Paused;
                    break;
                case "Start Pending":
                    result = Constants.ServiceState.StartPending;
                    break;
                case "Stop Pending":
                    result = Constants.ServiceState.StopPending;
                    break;
                case "Continue Pending":
                    result = Constants.ServiceState.ContinuePending;
                    break;
                case "Pause Pending":
                    result = Constants.ServiceState.PausePending;
                    break;
            }
            return result;
        }

        public static List<ServicesStatusModel> MakeChecking(string hostName, List<string> servicesNames)
        {
            var servicesStatuses = new List<ServicesStatusModel>();
            if (servicesNames.Count == 0)
            {
                return servicesStatuses;
            }

            var mainOptions = OptionsHelper.Options;

            var options = new ConnectionOptions();
            if (Environment.MachineName.ToLower() != hostName.ToLower())
            {
                options.Username = mainOptions.SystemUserLogin;
                options.Password = mainOptions.SystemUserPassword;
                options.EnablePrivileges = true;
                options.Impersonation = ImpersonationLevel.Impersonate;
            }

            var scope = new ManagementScope($"\\\\{hostName}\\root\\CIMV2", options);
            scope.Connect();
            ManagementPath path = new ManagementPath("Win32_Service");
            ManagementClass services;
            services = new ManagementClass(scope, path, null);

            foreach (ManagementObject service in services.GetInstances())
            {
                var currentServiceName = service.GetPropertyValue("Name").ToString().ToLower();
                // https://www.codeproject.com/Articles/28161/Using-WMI-to-manipulate-services-Install-Uninstall
                foreach (var serviceName in servicesNames)
                {
                    if (currentServiceName == serviceName.ToLower())
                    {
                        var currentServiceStateName = service.GetPropertyValue("State").ToString();
                        var currentServiceState = GetServiceStateByStateName(currentServiceStateName);
                        servicesStatuses.Add(
                            new ServicesStatusModel()
                            {
                                Name = currentServiceName,
                                State = currentServiceState
                            });
                    }
                }
            }
            return servicesStatuses;
        }
    }
}