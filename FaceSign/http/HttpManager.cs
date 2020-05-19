using FaceSign.log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.http
{
    public class HttpManager
    {
        public static HttpManager Instance = new HttpManager();
        public static int TimeOut = 15 * 1000;
        public static string Tag = "HttpManager";
        public static string Host = "52.81.26.63";

        public static bool DownloadFile(string filePath, string url)
        {
            FileInfo info = new FileInfo($@"{filePath}");
            HttpWebRequest req = null;
            FileStream fs = null;
            bool result = false;
            try
            {
                if (info.Exists) {
                    info.Delete();
                }
                req = (HttpWebRequest)WebRequest.Create($@"{url}");
                req.Method = WebRequestMethods.Http.Get;
                req.AllowAutoRedirect = true;
                req.Timeout = TimeOut;
                req.KeepAlive = false;
                req.Accept = "*/*";
                var res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var stream = res.GetResponseStream();
                    fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    byte[] buffer = new byte[4096];
                    int len = -1;
                    while ((len=stream.Read(buffer,0,buffer.Length))>0) {
                        fs.Write(buffer,0,len);
                        fs.Flush();
                    }
                    result = true;
                }
            }
            catch (Exception e)
            {
                Log.I($@"下载文件{url}异常:{e.Message}");
            }
            finally
            {
                req?.Abort();
                fs?.Close();
            }
            return result;
        }

        public static bool DownloadFileByBreakpoint(string url,string path) {
            FileInfo info = new FileInfo($@"{path}");
            HttpWebRequest req = null;
            FileStream fs = null;
            bool result = false;
            try {
                long position = 0;
                long contentLen = 0;
                if (info.Exists)
                {
                    req = (HttpWebRequest)WebRequest.Create($@"{url}");
                    req.Method = WebRequestMethods.Http.Get;
                    req.AllowAutoRedirect = true;
                    req.Timeout = TimeOut;
                    req.KeepAlive = false;
                    req.Accept = "*/*";
                    var response = (HttpWebResponse)req.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        contentLen = response.ContentLength;
                        if (contentLen < info.Length)
                        {
                            Log.I("下载文件长度异常，删除该文件准备重新下载");
                            FileInfo file = new FileInfo(path);
                            file.Delete();
                            req.Abort();
                        } else if (contentLen == info.Length) {
                            Log.I("文件已经下载完毕，准备校验MD5");
                            return true;
                        } else {
                            position = info.Length;
                            req.Abort();
                        }
                    }
                    else {
                        Log.I($@"无法请求文件：{url},http code:{response.StatusCode}");
                        return false;
                    }          
                }                
                req = (HttpWebRequest)WebRequest.Create($@"{url}");
                req.Method = WebRequestMethods.Http.Get;
                req.AllowAutoRedirect = true;
                req.Timeout = TimeOut;
                req.KeepAlive = false;
                req.Accept = "*/*";
                if (position > 0)
                {
                    req.AddRange(position);
                    fs = File.OpenWrite(path);
                    fs.Seek(position,SeekOrigin.Current);
                }
                else {
                    fs = File.Open(path,FileMode.Create);
                }
                var res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK || res.StatusCode == HttpStatusCode.PartialContent)
                {
                    var stream = res.GetResponseStream();
                    byte[] buffer = new byte[4096];
                    int len = -1;
                    while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fs.Write(buffer, 0, len);
                        fs.Flush();
                    }
                    var file = new FileInfo(path);
                    if (contentLen == file.Length||res.ContentLength==file.Length)
                    {
                        result = true;
                    }
                    else {
                        Log.I($@"下载文件{url}异常,长度不匹配");
                        fs?.Close();
                        file.Delete();
                    }
                }
                else {
                    Log.I($@"下载文件{url}异常,HttpCode:{res.StatusCode}");
                }
            }
            catch (Exception e) {
                Log.I($@"下载文件{url}异常:{e.Message}");
            }
            finally
            {
                req?.Abort();
                fs?.Close();
            }
            return result;
        }



        public static T Get<T>(RequestParam requestParam) where T : Response, new()
        {
            var response = new T();
            StreamReader reader = null;
            HttpWebRequest req = null;
            var param = CreateGetParam(requestParam);
            try
            {
                req = (HttpWebRequest)WebRequest.Create($@"http://{Host}/{requestParam.Url}" + param);
                req.Method = WebRequestMethods.Http.Get;
                req.AllowAutoRedirect = true;
                req.Timeout = requestParam.TimeOut;
                req.KeepAlive = false;
                req.Accept = "*/*";
                var res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var stream = res.GetResponseStream();
                    if (stream != null)
                    {
                        reader = new StreamReader(stream, Encoding.UTF8);
                        var data = reader.ReadToEnd();
                        Log.I($@"{requestParam.Url} response:\r\n{data}");
                        response = JsonConvert.DeserializeObject<T>(data);
                    }
                    else
                    {
                        response.code = -1;
                        response.message = "网络请求异常";
                    }

                }
                else
                {
                    response.code = -1;
                    response.message = $@"网络请求异常({res.StatusCode})";
                }
            }
            catch (Exception e)
            {
                Log.I(Tag, $@"request {requestParam.Url}{param} :" + e.Message);
                response.code = -1;
                response.message = "网络请求异常";
            }
            finally
            {
                req?.Abort();
                reader?.Close();
            }
            return response;
        }

        protected static string CreateGetParam(RequestParam requestParam)
        {
            var param = "?";
            var enumerator = requestParam.Params.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var keyValuePair = enumerator.Current;
                param += keyValuePair.Key + "=" + keyValuePair.Value.ToString() + "&";
            }
            enumerator.Dispose();
            if (param.Substring(param.Length - 1, 1).Equals("&"))
            {
                param = param.Substring(0, param.Length - 1);
            }
            if (param.Equals("?"))
            {
                param = "";
            }
            return param;
        }

        public static T Post<T>(RequestParam requestParam) where T : Response, new()
        {
            var response = new T();
            StreamReader reader = null;
            HttpWebRequest req = null;
            var param = CreatePostParam(requestParam);
            Log.I(Tag, "Post Param:\r\n" + param);
            try
            {
                req = (HttpWebRequest)WebRequest.Create($@"http://{Host}/{requestParam.Url}");
                req.Method = WebRequestMethods.Http.Post;
                req.AllowAutoRedirect = true;
                req.Timeout = requestParam.TimeOut;
                req.KeepAlive = false;
                req.Accept = "*/*";
                req.ContentType="application/json";
                var input = req.GetRequestStream();
                var buffer = Encoding.UTF8.GetBytes(param);
                input.Write(buffer,0,buffer.Length);
                input.Close();
                var res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var stream = res.GetResponseStream();
                    if (stream != null)
                    {
                        reader = new StreamReader(stream, Encoding.UTF8);
                        var data = reader.ReadToEnd();
                        Log.I(Tag, "Post Result:\r\n" + data);
                        response = JsonConvert.DeserializeObject<T>(data);
                    }
                    else
                    {
                        response.code = -1;
                        response.message = "网络请求异常";
                    }
                }
                else
                {
                    response.code = -1;
                    response.message = $@"网络请求异常({res.StatusCode})";
                }
            }
            catch (Exception e)
            {
                Log.I(Tag, $@"request {requestParam.Url} {param} :" + e.Message);
                response.code = -1;
                response.message = "网络请求异常";
            }
            return response;
        }

        public static T Post<T>(string host,RequestParam requestParam) where T : Response, new()
        {
            var response = new T();
            StreamReader reader = null;
            HttpWebRequest req = null;
            var param = CreatePostParam(requestParam);
            Log.I(Tag, "Post Param:\r\n" + param);
            try
            {
                req = (HttpWebRequest)WebRequest.Create($@"http://{host}/{requestParam.Url}");
                req.Method = WebRequestMethods.Http.Post;
                req.AllowAutoRedirect = true;
                req.Timeout = requestParam.TimeOut;
                req.KeepAlive = false;
                req.Accept = "*/*";
                req.ContentType = "application/json";
                var input = req.GetRequestStream();
                var buffer = Encoding.UTF8.GetBytes(param);
                input.Write(buffer, 0, buffer.Length);
                input.Close();
                var res = (HttpWebResponse)req.GetResponse();
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var stream = res.GetResponseStream();
                    if (stream != null)
                    {
                        reader = new StreamReader(stream, Encoding.UTF8);
                        var data = reader.ReadToEnd();
                        Log.I(Tag, "Post Result:\r\n" + data);
                        response = JsonConvert.DeserializeObject<T>(data);
                    }
                    else
                    {
                        response.code = -1;
                        response.message = "网络请求异常";
                    }
                }
                else
                {
                    response.code = -1;
                    response.message = $@"网络请求异常({res.StatusCode})";
                }
            }
            catch (Exception e)
            {
                Log.I(Tag, $@"request {requestParam.Url} {param} :" + e.Message);
                response.code = -1;
                response.message = "网络请求异常";
            }
            return response;
        }

        protected static string CreatePostParam(RequestParam requestParam)
        {
            return JsonConvert.SerializeObject(requestParam.PostParam);
        }
    }

    public class RequestParam
    {
        public string Url;
        public Dictionary<string, string> Header = new Dictionary<string, string>();
        public Dictionary<string, object> Params = new Dictionary<string, object>();
        public object PostParam;
        public int TimeOut = HttpManager.TimeOut;

        public RequestParam(string url)
        {
            Url = url;
        }

        public void AddParam(string key, object value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
                return;
            Params.Add(key, value);
        }

        public void AddHeader(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
                return;
            Header.Add(key, value);
        }
    }
}
