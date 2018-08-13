using log4net;
using log4net.Config;
using System;
using System.IO;
using System.Reflection;

namespace DeviceControl.Core
{
    public enum LogLevel
    {
        Info,
        Error
    }

    public static class Logger
    {
        public static void Configure(string configLocation)
        {
            var entryAssembly = Assembly.GetExecutingAssembly();
            var assemblyFolder = Path.GetDirectoryName(entryAssembly.Location);
            var logRepository = LogManager.GetRepository(entryAssembly);

            XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(assemblyFolder, configLocation)));
        }

        public static void Log(object type, LogLevel level, Exception ex)
        {
            Log(type, level, null, ex);
        }

        public static void Log(object type, LogLevel level, string message, Exception ex)
        {
            var logger = GetLogger(type);
            switch (level)
            {
                case LogLevel.Error:
                    logger.Error(message, ex);
                    break;
                case LogLevel.Info:
                    logger.Info(message, ex);
                    break;
                default:
                    throw new NotImplementedException($"Not implemented level: {level}");
            }
        }

        public static void Log(object type, LogLevel level, string message)
        {
            var logger = GetLogger(type);
            switch (level)
            {
                case LogLevel.Error:
                    logger.Error(message);
                    break;
                case LogLevel.Info:
                    logger.Info(message);
                    break;
                default:
                    throw new NotImplementedException($"Not implemented level: {level}");
            }
        }

        private static ILog GetLogger(object type)
        {
            var resolvedType = type is Type typeAsType ? typeAsType : type.GetType();
            return LogManager.GetLogger(resolvedType);
        }
    }
}
