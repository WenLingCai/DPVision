using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DPVision.Common
{
    public static class JsonHelper
    {
        public static bool SaveFile(string sFilePath, JObject obj)
        {
            bool flag = obj == null || sFilePath == string.Empty;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool flag2 = !File.Exists(sFilePath);
                if (flag2)
                {
                    File.Create(sFilePath).Close();
                }
                using (FileStream fs = new FileStream(sFilePath, FileMode.Open, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        JsonWriter jw = new JsonTextWriter(sw);
                        jw.WriteRaw(obj.ToString());
                        jw.Close();
                        sw.Close();
                    }
                    fs.Close();
                }
                result = true;
            }
            return result;
        }

        public static JObject LoadFile(string sFilePath)
        {
            bool flag = !File.Exists(sFilePath);
            JObject result;
            if (flag)
            {
                result = null;
            }
            else
            {
                JObject obj = null;
                using (StreamReader sr = new StreamReader(sFilePath))
                {
                    using (JsonTextReader jtr = new JsonTextReader(sr))
                    {
                        obj = (JObject)JToken.ReadFrom(jtr);
                        jtr.Close();
                    }
                    sr.Close();
                }
                result = obj;
            }
            return result;
        }


   
        /// <summary>
        /// 读取JSON文件
        /// </summary>
        /// <param name="key">JSON文件中的最后一级的key值</param>
        /// <param name="key2">JSON文件中第一级key值</param>
        /// <returns>JSON文件中的value值</returns>
        public static string ReadJsonValue(string sFilePath, string key, string key1, string key2)
        {
            string value = "";
            bool flag = !File.Exists(sFilePath);
            if (flag)
            {
                return value;
            }
            else
            {
                using (System.IO.StreamReader file = System.IO.File.OpenText(sFilePath))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o = (JObject)JToken.ReadFrom(reader);

                        {
                            if (o != null)
                            {
                                JObject obj = o[key2] as JObject;
                                if (obj != null)
                                {
                                    JObject obj1 = obj[key1] as JObject;
                                    if (obj1 != null)
                                    {
                                        value = obj[key].ToString();
                                    }
                                }
                            }
                        }
                    }
                    file.Close();

                }
            }
            return value;
        }
        /// <summary>
        /// 读取JSON文件
        /// </summary>
        /// <param name="key">JSON文件中的最后一级的key值</param>
        /// <param name="key1">JSON文件中第一级key值</param>
        /// <returns>JSON文件中的value值</returns>
        public static string ReadJsonValue(string sFilePath, string key, string key1)
        {
            string value = "";
            bool flag = !File.Exists(sFilePath);
            if (flag)
            {
                return value;
            }
            else
            {
                using (System.IO.StreamReader file = System.IO.File.OpenText(sFilePath))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o = (JObject)JToken.ReadFrom(reader);

                        {
                            JObject obj = o[key1] as JObject;
                            if (obj != null)
                            {
                                value = obj[key].ToString();
                            }


                        }
                    }
                    file.Close();

                }
            }
            return value;
        }

        /// <summary>
        /// 读取JSON文件
        /// </summary>
        /// <param name="key">JSON文件中的最后一级的key值</param>
        /// <param name="key1">JSON文件中第一级key值</param>
        /// <returns>JSON文件中的value值</returns>
        public static string ReadJsonValue(string sFilePath, string key)
        {
            string value = "";

            bool flag = !File.Exists(sFilePath);
            if (flag)
            {
                return value;
            }
            else
            {
                using (System.IO.StreamReader file = System.IO.File.OpenText(sFilePath))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        JObject o = (JObject)JToken.ReadFrom(reader);
                        if (o != null)
                        {
                            value = o[key].ToString();
                        }
                    }
                    file.Close();

                }
            }
            return value;
        }
        /// <summary>
        /// 写入JSON文件
        /// </summary>
        /// <param name="key">JSON文件中的最后一级的key值</param>
        /// <param name="key1">JSON文件中第一级key值</param>
        /// <returns>JSON文件中的value值</returns>
        public static bool WriteJsonValue(string sFilePath, string key, string key1, string key2, string value)
        {
            bool result = false;
            try
            {
                StreamReader file = new StreamReader(sFilePath);
                JsonTextReader reader = new JsonTextReader(file);
                JObject obj = (JObject)JToken.ReadFrom(reader);
                if (obj != null)
                {
                    JObject obj1 = obj[key2] as JObject;
                    if (obj1 != null)
                    {
                        JObject obj2 = obj1[key1] as JObject;
                        if (obj2 != null)
                        {
                            if (obj2[key].ToString() != value)
                            {
                                obj2[key] = value;
                                result = true;
                            }

                        }
                    }
                }
                reader.Close();
                file.Close();

            }
            catch
            {
                return result;
            }
            return result;
        }

        /// <summary>
        /// 写入JSON文件
        /// </summary>
        /// <param name="key">JSON文件中的最后一级的key值</param>
        /// <param name="key1">JSON文件中第一级key值</param>
        /// <returns>JSON文件中的value值</returns>
        public static bool WriteJsonValue(string sFilePath, string key, string key1, string value)
        {
            bool result = false;
            try
            {
                StreamReader file = new StreamReader(sFilePath);
                JsonTextReader reader = new JsonTextReader(file);
                JObject obj = (JObject)JToken.ReadFrom(reader);
                if (obj != null)
                {
                    JObject obj1 = obj[key1] as JObject;
                    if (obj1 != null)
                    {
                        if (obj1[key].ToString() != value)
                        {
                            obj1[key] = value;
                            result = true;
                        }

                    }
                }
                reader.Close();
                file.Close();
                using (FileStream fs = new FileStream(sFilePath, FileMode.Open, FileAccess.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        JsonWriter jw = new JsonTextWriter(sw);
                        jw.WriteRaw(obj.ToString());
                        jw.Close();
                        sw.Close();
                    }
                    fs.Close();
                }
            }
            catch
            {
                return result;
            }
            return result;
        }

        /// <summary>
        /// 写入JSON文件
        /// </summary>
        /// <param name="key">JSON文件中的最后一级的key值</param>
        /// <param name="key1">JSON文件中第一级key值</param>
        /// <returns>JSON文件中的value值</returns>
        public static bool WriteJsonValue(string sFilePath, string key, string value)
        {
            bool result = false;
            try
            {
                StreamReader file = new StreamReader(sFilePath);
                JsonTextReader reader = new JsonTextReader(file);
                JObject obj = (JObject)JToken.ReadFrom(reader);
                if (obj != null)
                {
                    if (obj[key].ToString() != value)
                    {
                        obj[key] = value;
                        result = true;
                    }
                }
                reader.Close();
                file.Close();
            }
            catch
            {
                return result;
            }
            return result;
        }

        public static string GetResponseData(string JSONData, string Url)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(JSONData);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "application/json;charset=UTF-8";
            Stream reqstream = request.GetRequestStream();
            reqstream.Write(bytes, 0, bytes.Length);

            //声明一个HttpWebRequest请求

            request.Headers.Set("Pragma", "no-cache");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream streamReceive = response.GetResponseStream();
            Encoding encoding = Encoding.UTF8;

            StreamReader streamReader = new StreamReader(streamReceive, encoding);
            string strResult = streamReader.ReadToEnd();
            streamReceive.Dispose();
            streamReader.Dispose();

            return strResult;

        }

        public static JObject StringToJObj(string jkReturn)
        {

            return (JObject)JsonConvert.DeserializeObject(jkReturn);

        }

        public static string JObjToString(JObject jkReturn)
        {

            return JsonConvert.SerializeObject(jkReturn);

        }
        /*
    *  url:POST请求地址
    *  postData:json格式的请求报文,例如：{"key1":"value1","key2":"value2"}
    */

        public static string PostUrl(string url, string postData, int outTime)
        {
            string result = "";
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);

            req.Method = "POST";

            req.Timeout = outTime;//设置请求超时时间，单位为毫秒

            req.ContentType = "application/json";

            byte[] data = Encoding.UTF8.GetBytes(postData);

            req.ContentLength = data.Length;

            using (Stream reqStream = req.GetRequestStream())
            {
                reqStream.Write(data, 0, data.Length);

                reqStream.Close();
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Stream stream = resp.GetResponseStream();

            //获取响应内容
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            return result;
        }

        /// <summary>
        /// 不知道是{ 或者 [开始的json 时使用,不建议使用
        /// 建议自行通过StartsWith判断后选择使用 JsonToJObject 或 JsonToJArray
        /// </summary>
        /// <param name="str">需要转化的字符串</param>
        /// <returns> List<JObject> </returns>
        public static List<JObject> StringToJObjectList(string str)
        {
            List<JObject> jobList = new List<JObject>();
            if (str.StartsWith("["))
            {
                JArray jA = JArray.Parse(str);
                foreach (JObject jOb in jA)
                {
                    jobList.Add(jOb);
                }
            }
            else if (str.StartsWith("{"))
            {

                jobList.Add(JObject.Parse(str));
            }
            return jobList;
        }

        /// <summary>
        /// 从一个对象信息生成Json串
        /// </summary>
        /// <param name="obj">对象，类</param>
        /// <returns>string</returns>
        public static string ObjectToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// json还原到model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static T JsonToObject<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        public static void GetValue<T>(JObject obj, string key, ref T value)
        {
            if (obj[key] != null)
            {
                value = obj[key].ToObject<T>();
            }
        }

        public static string GetValue(JObject obj, string key, string defValue="")
        {
            string value;
            if (obj[key] != null)
            {
                value = obj[key].ToString();
            }
            else
            {
                value = defValue;
            }
            return value;
        }

        public static void SetValue(JObject obj, string key,JToken value)
        {
            obj.Add(key, value);
        }

        #region Get请求
        public static string Get(string uri)
        {
         
            //创建Web访问对  象
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(uri);
            //通过Web访问对象获取响应内容
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            //通过响应内容流创建StreamReader对象，因为StreamReader更高级更快
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            //string returnXml = HttpUtility.UrlDecode(reader.ReadToEnd());//解决编码问题
            string returnXml = reader.ReadToEnd();//利用StreamReader就可以从响应内容从头读到尾
            reader.Close();
            myResponse.Close();
            return returnXml;
        }
        #endregion

        #region Post请求
        public static string Post(string data, string uri)
        {
           
            //创建Web访问对象
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(uri);
            //数据转成“UTF-8”的字节流
            byte[] buf = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(data);

            myRequest.Method = "POST";
            myRequest.ContentLength = buf.Length;
            myRequest.ContentType = "application/json";
            myRequest.MaximumAutomaticRedirections = 1;
            myRequest.AllowAutoRedirect = true;
            //发送请求
            Stream stream = myRequest.GetRequestStream();
            stream.Write(buf, 0, buf.Length);
            stream.Close();

            //获取接口返回值
            //通过Web访问对象获取响应内容
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            //通过响应内容流创建StreamReader对象，因为StreamReader更高级更快
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            //string returnXml = HttpUtility.UrlDecode(reader.ReadToEnd());//解决编码问题
            string returnXml = reader.ReadToEnd();//利用StreamReader就可以从响应内容从头读到尾
            reader.Close();
            myResponse.Close();
            return returnXml;

        }
        #endregion

        #region Put请求
        public static string Put(string data, string uri)
        {
            
            //创建Web访问对象
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(uri);
            //把用户传过来的数据转成“UTF-8”的字节流
            byte[] buf = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(data);

            myRequest.Method = "PUT";
            myRequest.ContentLength = buf.Length;
            myRequest.ContentType = "application/json";
            myRequest.MaximumAutomaticRedirections = 1;
            myRequest.AllowAutoRedirect = true;
            //发送请求
            Stream stream = myRequest.GetRequestStream();
            stream.Write(buf, 0, buf.Length);
            stream.Close();

            //获取接口返回值
            //通过Web访问对象获取响应内容
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            //通过响应内容流创建StreamReader对象，因为StreamReader更高级更快
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            //string returnXml = HttpUtility.UrlDecode(reader.ReadToEnd());//解决编码问题
            string returnXml = reader.ReadToEnd();//利用StreamReader就可以从响应内容从头读到尾
            reader.Close();
            myResponse.Close();
            return returnXml;

        }
        #endregion


        #region Delete请求
        public static string Delete(string data, string uri)
        {
           
            //创建Web访问对象
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(uri);
            //把用户传过来的数据转成“UTF-8”的字节流
            byte[] buf = System.Text.Encoding.GetEncoding("UTF-8").GetBytes(data);

            myRequest.Method = "DELETE";
            myRequest.ContentLength = buf.Length;
            myRequest.ContentType = "application/json";
            myRequest.MaximumAutomaticRedirections = 1;
            myRequest.AllowAutoRedirect = true;
            //发送请求
            Stream stream = myRequest.GetRequestStream();
            stream.Write(buf, 0, buf.Length);
            stream.Close();

            //获取接口返回值
            //通过Web访问对象获取响应内容
            HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse();
            //通过响应内容流创建StreamReader对象，因为StreamReader更高级更快
            StreamReader reader = new StreamReader(myResponse.GetResponseStream(), Encoding.UTF8);
            //string returnXml = HttpUtility.UrlDecode(reader.ReadToEnd());//解决编码问题
            string returnXml = reader.ReadToEnd();//利用StreamReader就可以从响应内容从头读到尾
            reader.Close();
            myResponse.Close();
            return returnXml;

        }
        #endregion

    }
}
