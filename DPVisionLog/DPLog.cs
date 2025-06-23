#define ISDOCK


using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

using log4net;
using log4net.Config;


namespace DPVisionLog
{
    public class DPLog
    {  /// <summary>
       /// 记录消息Queue
       /// </summary>
        private readonly ConcurrentQueue<FlashLogMessage> _que;

        /// <summary>
        /// 信号
        /// </summary>
        private readonly ManualResetEvent _mre;

        /// <summary>
        /// 日志
        /// </summary>
       // private readonly ILog _log;

        Thread t;
        /// <summary>
        /// 另一个线程记录日志，只在程序初始化时调用一次
        /// </summary>
        public void StartWriteLog()
        {
            t = new Thread(new ThreadStart(WriteLog));
            t.IsBackground = true;
            t.Start();
        }

        public void ExitThread()
        {
            if (t != null)
            {
                t.Abort();
                t = null;
            }

        }
        public int tip_time_ms = 0xfa0;
        public void Reset()
        {
            _mre.Close();
            _mre.Dispose();
        }
        /// <summary>
        /// 从队列中写日志至磁盘
        /// </summary>
        private void WriteLog()
        {
            while (true)
            {
                
                // 等待信号通知
                _mre.WaitOne();

                FlashLogMessage msg;
                // 判断是否有内容需要如磁盘 从列队中获取内容，并删除列队中的内容
                while (_que.Count > 0 && _que.TryDequeue(out msg))
                {
                    // 判断日志等级，然后写日志
                    switch (msg.Level)
                    {
                        case Log4NetLevel.Debug:

                            LogHelper.Debug(msg.type,msg.Message, msg.Exception);
              
                          
                                break;
                        case Log4NetLevel.Info:

                            LogHelper.Info(msg.type, msg.Message, msg.Exception);
                          
                            break;
                        case Log4NetLevel.Error:
                          
                            LogHelper.Error(msg.type, msg.Message, msg.Exception);

                            //this.form.AddLogInfo(msg.Message, msg.Time, msg.logType, msg.Level);
                            break;
                        case Log4NetLevel.Warn:

                            LogHelper.Warn(msg.type, msg.Message, msg.Exception);
                          
                            break;
                        case Log4NetLevel.Fatal:

                            LogHelper.Fatal(msg.type, msg.Message, msg.Exception);
                           
                            break;
                    }
                }

                // 重新设置信号
                _mre.Reset();
                Thread.Sleep(1);
            }
        }


        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="message">日志文本</param>
        /// <param name="level">等级</param>
        /// <param name="ex">Exception</param>
        public void EnqueueMessage(Type type,string message, Log4NetLevel level,LogType logtype, Exception ex = null)
        {


            _que.Enqueue(new FlashLogMessage
            { 
                type = type,
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"),
                Message = message,
                Level = level,
                logType = logtype,
                Exception = ex
            });

                // 通知线程往磁盘中写日志
                _mre.Set();
            
        }

        public void Debug(Type type,string msg,Exception ex = null)
        {
            Instance.EnqueueMessage(type,msg, Log4NetLevel.Debug, LogType.LogMove, ex);
        }

        public void Error(Type type, string msg,  Exception ex = null)
        {
            Instance.EnqueueMessage(type, msg, Log4NetLevel.Error, LogType.LogMove, ex);
        }

        public void Fatal(Type type, string msg, Exception ex = null)
        {
            Instance.EnqueueMessage(type, msg, Log4NetLevel.Fatal, LogType.LogMove, ex);
        }

        public void Info(Type type, string msg, Exception ex = null)
        {
            Instance.EnqueueMessage(type, msg, Log4NetLevel.Info, LogType.LogMove, ex);
        }

        public void Warn(Type type, string msg,Exception ex = null)
        {
            Instance.EnqueueMessage(type, msg, Log4NetLevel.Warn, LogType.LogMove, ex);
        }

        private static DPLog log=new DPLog();
      
      
        public static DPLog Instance
        {
            get
            {
                return DPLog.log;
            }
        }

        public int nLogMaxCount = 1000;
        //{
        //    get
        //    {
        //        return this.form.nLogCount;
        //    }
        //    set
        //    {
        //        this.form.nLogCount = value;
        //    }
        //}

        private DPLog()
        {
         
           
            //// 设置日志配置文件路径
            //XmlConfigurator.Configure(configFile);

            _que = new ConcurrentQueue<FlashLogMessage>();
            _mre = new ManualResetEvent(false);
           // loggerTrace = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
          
           // loggerTrace = LogManager.GetLogger("LogTrace");
           
         //   Register();
        }
     
     

    }

    public class TxtModel
    {
        public int Id { get; set; }

        public string Content { get; set; }
    }

    public class ExcelHeaderModel
    {
        public string name { get; set; }
    }

    /// <summary>
    /// excel导出
    /// </summary>
    public class ExcelModel
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 报警码
        /// </summary>
        public string WarnCode { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }
    }
}