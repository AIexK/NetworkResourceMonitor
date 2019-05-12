using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourcesMonitor.Models
{
    public class DataBaseInfoModel
    {
        public string Name { get; set; }
        public List<string> Statuses { get; set; }
        public string StatusesOneLine {
            get
            {
                if (this.Statuses == null || this.Statuses.Count == 0)
                {
                    return string.Empty;
                }
                return string.Join(", ", this.Statuses);
            }
        }         
    }

    public class SqlServerCheckResult
    {
        public List<DataBaseInfoModel> DataBaseInfoList { get; set; } = new List<DataBaseInfoModel>();
        public string ErrorMessage { get; set; }
    }
}