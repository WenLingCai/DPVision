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
       /// ��¼��ϢQueue
       /// </summary>
        private readonly ConcurrentQueue<FlashLogMessage> _que;

        /// <summary>
        /// �ź�
        /// </summary>
        private readonly ManualResetEvent _mre;

        /// <summary>
        /// ��־
        /// </summary>
       // private readonly ILog _log;

        Thread t;
        /// <summary>
        /// ��һ���̼߳�¼��־��ֻ�ڳ����ʼ��ʱ����һ��
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
        /// �Ӷ�����д��־������
        /// </summary>
        private void WriteLog()
        {
            while (true)
            {
                
                // �ȴ��ź�֪ͨ
                _mre.WaitOne();

                FlashLogMessage msg;
                // �ж��Ƿ���������Ҫ����� ���ж��л�ȡ���ݣ���ɾ���ж��е�����
                while (_que.Count > 0 && _que.TryDequeue(out msg))
                {
                    // �ж���־�ȼ���Ȼ��д��־
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

                // ���������ź�
                _mre.Reset();
                Thread.Sleep(1);
            }
        }


        /// <summary>
        /// д��־
        /// </summary>
        /// <param name="message">��־�ı�</param>
        /// <param name="level">�ȼ�</param>
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

                // ֪ͨ�߳���������д��־
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
         
           
            //// ������־�����ļ�·��
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
    /// excel����
    /// </summary>
    public class ExcelModel
    {
        /// <summary>
        /// ���
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public string WarnCode { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// ����
        /// </summary>
        public string Content { get; set; }
    }
}