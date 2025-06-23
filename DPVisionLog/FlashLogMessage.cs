using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DPVisionLog
{
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        LogMove,
        LogVision,
        LogMes
    }
    /// <summary>
    /// log4net 日志等级类型枚举
    /// </summary>
    public enum Log4NetLevel
    {
        Warn = 1,
        Debug = 2,
        Info = 3,
        Fatal = 4,
        Error = 5
    }
    /// <summary>
    /// 日志内容
    /// </summary>
    public class FlashLogMessage
    {
        public Type type { get; set; }
        public string Time { get; set; }
        public string Message { get; set; }
        public Log4NetLevel Level { get; set; }
        public LogType logType { get; set; }
        public Exception Exception { get; set; }

    }
}
