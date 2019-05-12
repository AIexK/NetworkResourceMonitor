using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourcesMonitor.Models
{
    public enum ResultCode
    {
        Ok = 0,
        Cancel = 1,
        CommonError = -1
    }

    public class Result
    {
        public string Message { get; set; }
        public ResultCode ResultCode { get; set; }
    }
}