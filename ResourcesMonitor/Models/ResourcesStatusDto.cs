using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourcesMonitor.Models
{
    public class DiskSpaceDto
    {
        public string Letter { get; set; }
        public int FullnessPercent { get; set; }
        public string FreeSpaceToDisplay { get; set; }
        public bool IsOverflow { get; set; }
    }

    public class WindowsServicesStatusDto
    {
        public string Name { get; set; }
        public string StateName { get; set; }
    }

    public class ServerStatusDto
    {
        public string Id { get; set; }
        public string LogicGroupId { get; set; }
        public string HostName { get; set; }
        public string Ip { get; set; }
        public long Ping { get; set; }
        public List<DiskSpaceDto> DiskSpace { get; set; } = new List<DiskSpaceDto>();
        public bool IsSqlBaseUnderMonitoring { get; set; }
        public List<DatabaseInfoDto> SqlBasesWithProblems { get; set; } = new List<DatabaseInfoDto>();
        public List<WindowsServicesStatusDto> WindowsServicesStatuses { get; set; } = new List<WindowsServicesStatusDto>();
        public string ErrorMessage { get; set; }
    }

    public class WebsiteStateDto
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public bool IsAvailable { get; set; }        
    }

    public class ServersLogicGroupDto
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public List<WebsiteStateDto> WebsiteStatesDto { get; set; }
    }

    public class ResourcesStatusDto
    {
        public List<ServersLogicGroupDto> ServersLogicGroupsDto { get; set; } = new List<ServersLogicGroupDto>();
        public List<ServerStatusDto> ServersStatusDto { get; set; } = new List<ServerStatusDto>();
    }

}