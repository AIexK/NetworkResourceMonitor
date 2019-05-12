using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ResourcesMonitor;
using ResourcesMonitor.Utils;
using System.Net.NetworkInformation;

namespace ResourcesMonitor.Models
{
    public class DiskDataModel
    {
        public string Name { get; set; }
        public ulong FreeSpace { get; set; }

        public bool IsOverflow
        {
            get
            {
                if (this.FreeSpaceInMb <= OptionsHelper.Options.MinFreeDiskSpaceInMb &&
                    this.FreeSpaceInPercent <= OptionsHelper.Options.MinFreeDiskSpaceInPercent)
                {
                    return true;
                }          
                return false;
            }
        }
        public string FreeSpaceToDisplay
        {
            get
            {
                return this.BytesToString(this.FreeSpace);
            }
        }
        public ulong Size { get; set; }        
        public ulong FreeSpaceInMb
        {
            get
            {
                uint result = 0;
                result = (uint)Math.Round((double)this.FreeSpace / (double)1024 / (double)1024);
                return result;
            }
        }
        public string BytesToString(ulong byteCount)
        {
            var size = Convert.ToInt64(byteCount);
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(size);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(size) * num).ToString() + suf[place];
        }

        public int UsedSpaceInPercent
        {
            get
            {
                return 100 - this.FreeSpaceInPercent;
            }
        }

        public int FreeSpaceInPercent
        {
            get
            {
                int result = 0;
                if (this.Size == 0)
                {
                    // To prevent division on zero
                    return 0;
                }
                result = (int)Math.Round((double)this.FreeSpace / (double)this.Size * (double)100);
                return result;
            }
        }
    }

    public class ServicesStatusModel
    {
        public string Name { get; set; }
        public Constants.ServiceState State { get; set; }
    }

    public class SqlBasesStatusModels
    {
        public bool IsAvailable { get; set; }
        public string BaseName { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class ServerStatusModel
    {
        public bool IsNesseseryToCheckSqlBases { get; set; }
        public List<string> ErrorMessages { get; set; } = new List<string>();
        public string ErrorMessagesOneLine {
            get
            {
                return string.Join("<br>", this.ErrorMessages.ToArray());
            }
        }

        private void AddErrorMessage(Exception exception, string textInfo)
        { 
            const int MAX_NUMBER_ERROR_RECORDS = 100;
            if (this.ErrorMessages.Count <= MAX_NUMBER_ERROR_RECORDS &&
                exception != null)
            {
                this.ErrorMessages.Add($"{textInfo} {exception.Message}");
            }
            else
            if (exception != null)
            {
                Logger.LogError(exception.ToString());
            }
            if (exception == null)
            {
                this.ErrorMessages.Add($"##{textInfo}##");
                Logger.LogError($"##{textInfo}##");
            }
        }
        public string Id { get; set; }
        public string HostName { get; set; }
        public string LogicGroupId { get; set; }
        public string Description { get; set; }

        private string _ip;
        public string Ip
        {
            get
            {
                return this._ip;
            }
        }

        private long _pingMsec;
        public long PingMsec
        {
            get
            {
                return this._pingMsec;
            }
        }
        public int StateId { get; set; }
        public List<ServicesStatusModel> WindowsServicesStatus { get; set; } = new List<ServicesStatusModel>();
        public List<DiskDataModel> DisksData { get; set; } = new List<DiskDataModel>();
        public SqlServerCheckResult CheckingDatabaseResult { get; set; }
        public Result MakeChecking()
        {
            ErrorMessages.Clear();
            var result = new Result()
            {
                ResultCode = ResultCode.Ok
            };

            // Ping
            this._pingMsec = 0;
            try
            {
                using (Ping ping = new Ping())
                {
                    this._pingMsec = ping.Send(this.HostName).RoundtripTime;
                }
            }
            catch (Exception ex)
            {
                this._pingMsec = (int)ResultCode.CommonError;
                this.AddErrorMessage(ex, $"Ping of {this.HostName}");
            }

            // Get IP
            try
            {
                this._ip = CommonUtils.GetIPAddressFromMachineName(this.HostName);
            }
            catch (Exception ex)
            {
                this.AddErrorMessage(ex, $"Getting IP of {this.HostName}");
            }

            // Check Windows Services
            try
            {
                var servicesNames = OptionsHelper.GetWindowsServiceNamesToControl(this.HostName);
                if (servicesNames.Count != 0)
                {
                    this.WindowsServicesStatus = ServicesControl.MakeChecking(this.HostName, servicesNames);
                }
            }
            catch (Exception ex)
            {
                this.AddErrorMessage(ex, $"Checking Windows Services of {this.HostName}");
            }

            // Check disks status
            var disksInfo = new List<DiskDataModel>();
            try
            {
                this.DisksData = DiskUtils.GetDrivesListOfRemoteMachine(this.HostName);
            }
            catch (Exception ex)
            {
                this.AddErrorMessage(ex, $"Checking disks status of {this.HostName} error");
            }

            // Check Databases status
            this.IsNesseseryToCheckSqlBases = OptionsHelper.IsNessesaryToCheckDatabase(this.HostName);
            try
            {
                if (this.IsNesseseryToCheckSqlBases)
                {
                    this.CheckingDatabaseResult = DataBaseUtils.CheckBases(this.HostName);
                    if (!string.IsNullOrEmpty(this.CheckingDatabaseResult.ErrorMessage))
                    {
                        this.AddErrorMessage(null, this.CheckingDatabaseResult.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                this.AddErrorMessage(ex, $"Checking databases status of {this.HostName} error");
            }

            return result;
        }
    }

    public class SiteStatusModel
    {
        public string SiteId { get; set; }
        public string ErrorMessage { get; set; }
        public string Url { get; set; }        
        public bool IsAvailable { get; set; }
    }

    public class ResourcesStatusModel
    {
        public bool IsFirstIntervalChanging { get; set; } = true;
        public List<ServerStatusModel> ServersStatus { get; set; } = new List<ServerStatusModel>();
        public List<SiteStatusModel> SitesStatus { get; set; } = new List<SiteStatusModel>();
        public Result Initialize()
        {
            //this.ServersStatus = new List<ServerStatusModel>();
            var result = new Result()
            {
                ResultCode = ResultCode.Ok
            };
            try
            {
                var options = OptionsHelper.Options;
                foreach (var optionsServerData in options.ServerDatas)
                {
                    if (string.IsNullOrEmpty(optionsServerData.HostName))
                    {
                        continue;
                    }
                    var serverStatus = new ServerStatusModel();
                    serverStatus.Id = optionsServerData.Id;
                    serverStatus.LogicGroupId = optionsServerData.LogicGroupId;
                    serverStatus.HostName = optionsServerData.HostName;                    
                    serverStatus.WindowsServicesStatus = new List<ServicesStatusModel>();
                    this.ServersStatus.Add(serverStatus);
                }
            }
            catch (Exception ex)
            {
                result.ResultCode = ResultCode.CommonError;
                result.Message = ex.ToString();
            }
            return result;           
        }       
    }
}