using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourcesMonitor
{
    public static class Constants
    {
        public const string DOMAIN_NAME = "YOUR_DOMAIN";
        public static class ContentTypes
        {
            public const string X_WWW_FORM = "application/x-www-form-urlencoded";
            public const string JSON = "application/json";

        }

        public static class InfoAccauntData
        {
            public const string HOST = "post.DOMEN.ru";
            public const string MAIL = "EmailFrom@Domen.com";
            public const string PASSWORD = "Password";
            public const int SMTP_PORT = 225;
            public const int POP3_PORT = 110;

        }

        public static class ResultCodes
        {
            public const int RESULT_OK = 0;
            public const int RESULT_CANCEL = 1;
            public const int RESULT_COMMON_ERROR = -1;
            public const int RESULT_COMMON_HANDLED_ERROR = -2;
        }

        public enum ServiceState
        {
            Running         = 0,
            Paused          = 1,
            StartPending    = 2,  // (start pending, Starting);
            PausePending    = 3,  // (pause pending, Pausing);
            ContinuePending = 4,  // after pause(continue pending, Starting_after_pause);
            StopPending     = 5,  // (stop pending, Stopping);
            Stopped         = 6,
            Unknown         = 7
        }

        public static class DatabaseStates
        {
            public const string SIMPLE_STATUS_ONLINE    = "ONLINE";
            public const string AUTOCLOSE               = "autoclose";
            public const string UNKNOWN_CODE_2          = "2 not sure";
            public const string SELECT_INTO_OR_BULKCOPY = "select into/bulkcopy";
            public const string TRUNCATE_LOG_ON_CHKPT   = "trunc.log on chkpt";
            public const string TORN_PAGE_DETECTION     = "torn page detection";
            public const string LOADING              = "loading";
            public const string PRE_RECOVERY         = "pre recovery";
            public const string RECOVERING           = "recovering";
            public const string NOT_RECOVERED        = "not recovered";
            public const string OFFLINE              = "offline";
            public const string READ_ONLY            = "read only";
            public const string DBO_USE_ONLY         = "dbo use only";
            public const string SINGLE_USER          = "single user";
            public const string UNKNOWN_CODE_8192    = "8192 not sure";
            public const string UNKNOWN_CODE_16384   = "16384 not sure";
            public const string EMERGENCY_MODE       = "emergency mode";
            public const string ONLINE               = "online";
            public const string UNKNOWN_CODE_131072  = "131072 not sure";
            public const string UNKNOWN_CODE_262144  = "262144 not sure";
            public const string UNKNOWN_CODE_524288  = "524288 not sure";
            public const string UNKNOWN_CODE_1048576 = "1048576 not sure";
            public const string UNKNOWN_CODE_2097152 = "2097152 not sure";
            public const string AUTOSHRINK           = "autoshrink";
            public const string CLEANLY_SHUTDOWN     = "cleanly shutdown";
        }
    }
}