using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourcesMonitor
{
    public static class InformationMessagesPreviousState
    {
        public static int SiteErrorsCount { get; set; }
        public static int DiskErrorsCount { get; set; }
        public static int BaseErrorsCount { get; set; }
        public static int ServiceErrorsCount { get; set; }
        public static int IterationWithErrorsByService { get; set; } = 0;
        public static bool ThereWasServiceErrorButItWasReset { get; set; } = false;
        public static int CommonErrorsCount { get; set; }
    }
}