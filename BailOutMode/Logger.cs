using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BailOutMode
{
    public enum LogLevel
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error
    }

    public static class Logger
    {
        private static readonly string LoggerName = Plugin.PluginName;
        public static LogLevel LogLevel = LogLevel.Info;
        private static readonly ConsoleColor DefaultFgColor = ConsoleColor.Gray;
        public static IPA.Logging.Logger ipaLogger;
        /* IPA Log Levels by severity
         * None, Debug, Info, Notice, Warning, Error, Critical
         */

        private static void ResetForegroundColor()
        {
            Console.ForegroundColor = DefaultFgColor;
        }

        public static void Trace(string format, params object[] args)
        {
            if (LogLevel > LogLevel.Trace)
            {
                return;
            }
            /*
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("[" + LoggerName + " @ " + DateTime.Now.ToString("HH:mm") + " - Trace] " + String.Format(format, args));
            ResetForegroundColor();
            */
            IPA.Logging.StandardLogger.PrintFilter = IPA.Logging.Logger.LogLevel.All;
            ipaLogger.Debug(String.Format(format, args));
        }

        public static void Debug(string format, params object[] args)
        {
            if (LogLevel > LogLevel.Debug)
            {
                IPA.Logging.StandardLogger.PrintFilter = IPA.Logging.Logger.LogLevel.InfoUp;
                ipaLogger.Debug(String.Format(format, args));
                return;
            }

            //Console.ForegroundColor = ConsoleColor.Magenta;
            //Console.WriteLine("[" + LoggerName + " @ " + DateTime.Now.ToString("HH:mm") + " - Debug] " + String.Format(format, args));
            //ResetForegroundColor();
            IPA.Logging.StandardLogger.PrintFilter = IPA.Logging.Logger.LogLevel.All;
            ipaLogger.Debug(String.Format(format, args));
            //IPA.Logging.StandardLogger.PrintFilter = prevLevel;
        }

        public static void Info(string format, params object[] args)
        {
            if (LogLevel > LogLevel.Info)
            {
                return;
            }

            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine("[" + LoggerName + " @ " + DateTime.Now.ToString("HH:mm") + " - Info] " + String.Format(format, args));
            //ResetForegroundColor();
            ipaLogger.Info(String.Format(format, args));
        }

        public static void Warning(string format, params object[] args)
        {
            if (LogLevel > LogLevel.Warn)
            {
                return;
            }

            //Console.ForegroundColor = ConsoleColor.Blue;
            //Console.WriteLine("[" + LoggerName + " @ " + DateTime.Now.ToString("HH:mm") + " - Warning] " + String.Format(format, args));
            //ResetForegroundColor();
            ipaLogger.Warn(String.Format(format, args));
        }

        public static void Error(string format, params object[] args)
        {
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //Console.WriteLine("[" + LoggerName + " @ " + DateTime.Now.ToString("HH:mm") + " - Error] " + String.Format(format, args));
            //ResetForegroundColor();
            ipaLogger.Error(String.Format(format, args));
        }

        public static void Exception(string message, Exception e)
        {
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine("[" + LoggerName + " @ " + DateTime.Now.ToString("HH:mm") + "] " + String.Format("{0}-{1}-{2}\n{3}", message, e.GetType().FullName, e.Message, e.StackTrace));
            //ResetForegroundColor();
            ipaLogger.Critical(message);
            ipaLogger.Critical(e);
        }
    }
}
