using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourcesMonitor.Models
{
    public class ServerOptionModel
    {
        public string Id { get; set; }
        public string HostName { get; set; }
        public List<string> WindowsServicesNames { get; set; }
        public List<string> DatabasesIgnoreList { get; set; }
        public string SqlServerLogin { get; set; }
        public string SqlServerPassword { get; set; }
        public bool IsNesseseryToCheckSqlBases { get; set; }
        public string LogicGroupId { get; set; }
    }

    public class ServerLogicGroupModel
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public bool IsSpesialBlockForWebsites { get; set; }
        public List<SitesModel> Sites { get; set; }

    }

    public class SitesModel
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string HtmlToControl { get; set; }
    }

    public class OptionsModel
    {
        public int CheckingIntervalMilliseconds { get; set; }
        public string SystemUserLogin { get; set; }
        public string SystemUserPassword { get; set; }
        public int MinFreeDiskSpaceInPercent { get; set; }
        public string TelegramRequest { get; set; }
        public ulong MinFreeDiskSpaceInMb { get; set; }
        public List<ServerLogicGroupModel> ServerLogicGroups { get; set; }
        public List<ServerOptionModel> ServerDatas { get; set; }
    }
}