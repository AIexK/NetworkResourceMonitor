using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourcesMonitor.Models
{
    public class DataBaseOptionsModel
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public bool IsNesseseryToCheckSqlBases { get; set; }
    }
}