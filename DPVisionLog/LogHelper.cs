using log4net;
using System;

namespace DPVisionLog
{
    /// <summary>
    /// 日志帮助类
    /// </summary>
    public class LogHelper
    {
        public static void Fatal(Type type, object message, Exception exception = null)
        {
            ILog log = LogManager.GetLogger(type);
             if (exception == null)
                 log.Fatal(message);
            else
                 log.Fatal(message, exception);
        }

         public static void Error(Type type, object message, Exception exception = null)
         {
             ILog log = LogManager.GetLogger(type);
             if (exception == null)
                 log.Error(message);
             else
                log.Error(message, exception);
         }

         public static void Warn(Type type, object message, Exception exception = null)
        {
            ILog log = LogManager.GetLogger(type);
             if (exception == null)
                 log.Warn(message);
             else
                 log.Warn(message, exception);
         }

         public static void Info(Type type, object message, Exception exception = null)
         {
             ILog log = LogManager.GetLogger(type);
             if (exception == null)
                 log.Info(message);
             else
                 log.Info(message, exception);
         }

         public static void Debug(Type type, object message, Exception exception = null)
         {
             ILog log = LogManager.GetLogger(type);
             if (exception == null)
                 log.Debug(message);
             else
                 log.Debug(message, exception);
         }
    }
}
