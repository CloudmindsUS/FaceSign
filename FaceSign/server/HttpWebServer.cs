using FaceSign.data;
using FaceSign.db;
using FaceSign.http;
using FaceSign.http.req;
using FaceSign.log;
using FaceSign.model;
using FaceSign.utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.server
{
    public class HttpWebServer
    {
        public delegate void ShowPerson(PersonModel person);
        public static HttpWebServer Instance = new HttpWebServer();

        private HttpListener listener;

        public string UUID { get; private set; }
        public event ShowPerson OnPersonShow;
        string TerminalId;
        float Confidence = FaceManager.FaceConfidence;

        private HttpWebServer() {
            listener = new HttpListener();
            var host = GetHost();
            Log.I("IP："+host);
            listener.Prefixes.Add($@"http://127.0.0.1:9090/");
            var value = SharePreference.Read(Keys.KeyFaceConfidence,""+FaceManager.FaceConfidence);
            try
            {
                Confidence=float.Parse(value);
            }catch(Exception)
            {}
            SharePreference.Write(Keys.KeyFaceConfidence,Confidence+"");
        }

        private string GetHost()
        {
            string ip = "127.0.0.1";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress addr in host.AddressList)
            {
                if (addr.AddressFamily.ToString() == "InterNetwork")
                {
                    ip = addr.ToString();
                }
            }
            return ip;
        }

        public void Start(string TerminalId) {
            this.TerminalId = TerminalId;
            try
            {
                listener.Start();
                listener.BeginGetContext(Receive,null);
            }
            catch (Exception ex) {
                Log.I("can't start http server:"+ex.Message+System.Environment.NewLine+ex.StackTrace);
            }
        }

        private void Receive(IAsyncResult result) {
            var ctx = listener.EndGetContext(result);
            listener.BeginGetContext(Receive, null);
            var request = ctx.Request;
            var response = ctx.Response;
            var url = request.RawUrl;
            if (DownloadPersonServer.IsUpdatePerson)
            {
                SendResponse("1", "face lib is update", response);
                return;
            }
            if (request.HttpMethod.ToUpper() != "POST") {
                SendResponse("1", "it is only receive post method", response);
                return;
            }            
            if ("/is/ims/v1/sendAlarmData" == url)
            {
                HandleAlarmData(request,response);
            }
            else {
                SendResponse("1", "unknown request", response);
                return;
            }
        }


        private void HandleAlarmData(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                var postData = new StreamReader(request.InputStream).ReadToEnd();
                //Log.I("接收到的数据："+postData);
                IsAlarmEventModel AlarmEvent = JsonConvert.DeserializeObject<IsAlarmEventModel>(postData);                
                if (AlarmEvent.AlarmPointList != null && AlarmEvent.AlarmPointList.Count > 0)
                {
                    foreach (var model in AlarmEvent.AlarmPointList)
                    {
                        if (model.temperature >= 20)
                        {
                            ParseAlarm(model);
                        }
                    }
                }
                SendResponse("0", "success", response);
            }
            catch (Exception e) {
                Log.I("parse fail:"+e.Message);
                SendResponse("1", "date format fail:"+e.Message, response);
            }
        }
        private int index;
        private void ParseAlarm(IsAlarmPointModel model)
        {
            if (index > 20) {
                index = 0;
            }
            var data = Convert.FromBase64String(model.faceimg);
            var tempPath = $@"{FileUtil.GetFaceRecognitionPath()}\{index++}.jpg";
            FileStream fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            fs.Write(data,0,data.Length);
            fs.Flush();
            fs.Close();
            var fearture = FaceManager.Instance.GetFaceFeature(tempPath);
            if (fearture == null) {
                Log.I("no face!");
                PostStranger(model,-1);
                return;
            }
            var list = DownloadPersonServer.PersonList;
            if (list == null || list.Count <= 0) {
                Log.I("face lib count is 0");
                PostStranger(model,-1);
                return;
            }
            float maxSmiliaty = -1f;
            PersonModel verifyPerson = null;
            foreach (PersonModel person in list) {
                var personFeature = JsonConvert.DeserializeObject<float[]>(person.feature);
                var smiliaty = FaceManager.Instance.GetFaceSmiliaty(fearture,personFeature);
                if (smiliaty > maxSmiliaty) {
                    maxSmiliaty = smiliaty;
                    verifyPerson = person;
                }
            }
            Log.I($@"this face is:{verifyPerson?.name} ID:{verifyPerson?.person_id} core:{maxSmiliaty}");
            if (maxSmiliaty >= Confidence)
            {
                PersonModel person = new PersonModel
                {
                    person_id = verifyPerson.person_id,
                    name = verifyPerson.name,
                    type = verifyPerson.type,
                    temperature = model.temperature,
                    RealTimeFace = model.faceimg,
                    smiliaty = maxSmiliaty
                };
                OnPersonShow?.Invoke(person);
                Task.Factory.StartNew(() =>
                {
                    UploadPersonInfo(person, model);
                });
            }else{
                PostStranger(model,maxSmiliaty);
            }

        }

        private void PostStranger(IsAlarmPointModel model, float maxSmiliaty)
        {
            if (BuildConfig.IsSupportAccessControl)
            {
                if (!MetalDetectionManager.Instance.HasAlarm() && !MetalDetectionManager.Instance.IsAlarmTemperature(model.temperature))
                {
                    MetalDetectionManager.Instance.OpenDoor();
                }
            }
            PersonModel person = new PersonModel
            {
                person_id = "-1",
                name = "unknown",
                temperature = model.temperature,
                RealTimeFace = model.faceimg,
                smiliaty = maxSmiliaty
            };
            Task.Factory.StartNew(() =>
            {
                UploadPersonInfo(person, model);
            });
        }

        private async void UploadPersonInfo(PersonModel person, IsAlarmPointModel model)
        {
            OpenGuardReportRequest request = new OpenGuardReportRequest
            {
                image = model.faceimg,
                person_id = person.person_id,
                terminal_id = TerminalId,
                temp_val = (model.temperature * 10).ToString("f0")
            };
            var rep = await ApiService.OpenGuardReport(request);
        }

       
        private void SendResponse(string status,string message, HttpListenerResponse response)
        {
            IMSResponseModel model = new IMSResponseModel
            {
                status = status,
                message = message
            };
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
            response.StatusCode = 200;
            response.ContentLength64 = data.Length;
            response.ContentType = "application/json; Charset=UTF-8";
            //response.OutputStream.Write(data, 0, data.Length);
            response.Close(data,true);
        }
    }
}
