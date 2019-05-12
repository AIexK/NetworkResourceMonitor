using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ResourcesMonitor
{
    public enum MessageType
    {
        Success,
        Info,
        Warning,
        Error
    }
    public static class Logger
    {
        public static void Log(string text)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Debug(text);
        }
        public static void LogError(string text,
                                    bool sendToTelegram = false)
        {
            var logger = LogManager.GetLogger("ErrorLogger");
            logger.Trace(text);
            if (sendToTelegram)
            {
                const int MAX_MESSAGE_LENGHT = 1000;
                if (text.Length > MAX_MESSAGE_LENGHT)
                {
                    text = text.Substring(0, MAX_MESSAGE_LENGHT);
                }
                MessagesHelper.SendTelegramMessage($"ResourcesMonitor: {text}");
            }

        }
    }
}
