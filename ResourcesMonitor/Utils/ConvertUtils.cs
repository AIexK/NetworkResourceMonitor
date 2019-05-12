using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ResourcesMonitor.Utils
{
    public static class ConvertUtils
    {
        private static DateTime _defaultDateTime = DateTime.MinValue;
        public static class ConvertSafe
        {
            public static Int32 ToInt32(object value, Int32 defaultValue = 0)
            {
                try
                {
                    return Convert.ToInt32(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            public static Int64 ToInt64(object value, Int64 defaultValue = 0)
            {
                try
                {
                    return Convert.ToInt64(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            public static double ToDouble(object value, double defaultValue = 0d)
            {
                try
                {
                    return Convert.ToDouble(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            public static double? ToNullDouble(object value, double? defaultValue = null)
            {
                try
                {
                    return Convert.ToDouble(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            public static string ToString(object value, string defaultValue = "")
            {
                try
                {
                    return Convert.ToString(value);
                }
                catch
                {
                    return defaultValue;
                }
            }

            public static DateTime ToDateTime(object value)
            {
                try
                {
                    return Convert.ToDateTime(value);
                }
                catch
                {
                    return DateTime.MinValue;
                }
            }
        }
    }
}