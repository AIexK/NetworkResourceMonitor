using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Text.RegularExpressions;

namespace ResourcesMonitor.Utils
{
    public static class CommonUtils
    {
        public static string MakeResultJson(int errorCode, string message, dynamic data = null, string serverId = null)
        {
            dynamic resultObject = new { ErrorCode = errorCode, Message = message, Data = data, ServerId = serverId };
            var result = JsonConvert.SerializeObject(resultObject);
            return result;
        }

        public static string GetIPAddressFromMachineName(string machinename)
        {
            if (string.IsNullOrEmpty(machinename))
            {
                return string.Empty;
            }
            string ipAddress = string.Empty;
            System.Net.IPAddress ip = System.Net.Dns.GetHostEntry(machinename).AddressList.Where(o => o.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).First();
            ipAddress = ip.ToString();
            
            return ipAddress;
        }

        public static string GetMachineNameFromIPAddress(string ipAdress)
        {
            string machineName = string.Empty;
            try
            {
                System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(ipAdress);
                machineName = hostEntry.HostName;
            }
            catch (Exception ex)
            {
                Logger.LogError($"GetMachineNameFromIPAddress: {ex.ToString()}");
            }
            return machineName;
        }
    }
}