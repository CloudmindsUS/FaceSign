using FaceSign.data;
using FaceSign.http.rep;
using FaceSign.http.req;
using FaceSign.log;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.http
{
    public class ApiService
    {
        public async static Task<PersonsResponse> GetPersonList(string id,string ts) {
            RequestParam param = new RequestParam(HttpMethod.GetPersonList);
            param.AddParam("terminal_id", id);
            param.AddParam("ts", ts);
            return await Task.Run<PersonsResponse>(()=>{
                return HttpManager.Get<PersonsResponse>(param);
            });
        }

        public async static Task<Response> OpenGuardReport(OpenGuardReportRequest request) {
            RequestParam param = new RequestParam(HttpMethod.OpenGuardReport)
            {
                PostParam = request
            };
            return await Task.Run<Response>(() => {
                return HttpManager.Post<Response>(param);
            });
        }

        public async static Task<Response> OpenGuardReport(string host,OpenGuardReportRequest request)
        {
            RequestParam param = new RequestParam(BuildConfig.OtherWebPath)
            {
                PostParam = request
            };
            return await Task.Run<Response>(() => {
                return HttpManager.Post<Response>(host,param);
            });
        }

        public async static Task<InitResponse> Init(InitRequest request)
        {
            RequestParam param = new RequestParam(HttpMethod.Init)
            {
                PostParam = request
            };
            return await Task.Run<InitResponse>(() => {
                return HttpManager.Post<InitResponse>(param);
            });
        }

        public async static Task<Response> Bind(BindRequest request)
        {
            RequestParam param = new RequestParam(HttpMethod.Bind)
            {
                PostParam = request
            };
            return await Task.Run<Response>(() => {
                return HttpManager.Post<Response>(param);
            });
        }


        public async static Task<RefreshResponse> Refresh(RefreshRequest request)
        {
            RequestParam param = new RequestParam(HttpMethod.Refresh)
            {
                PostParam = request
            };
            return await Task.Run<RefreshResponse>(() => {
                return HttpManager.Post<RefreshResponse>(param);
            });
        }



    }
}
