using FaceSign.data;
using FaceSign.db;
using FaceSign.http;
using FaceSign.http.rep;
using FaceSign.http.req;
using FaceSign.log;
using FaceSign.model;
using FaceSign.utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceSign.server
{
    public class DownloadPersonServer
    {
        public delegate void OnUpdatePerson(bool doing);
        public static bool IsUpdatePerson = false;
        private static DownloadPersonServer instance = new DownloadPersonServer();
        public event OnUpdatePerson onUpdatePerson;
        public static List<PersonModel> PersonList = new List<PersonModel>();

        private DownloadPersonServer() {
        }

        public static DownloadPersonServer GetInstance() {
            return instance;
        }

        private string terminalId;
        private CancellationToken token;
        private int PollingTime = 60*1000;
        private bool isUpdate = false;
        private bool isCheckUpdate = false;

        public object Sharepreference { get; private set; }

        public void Start(string terminalId) {
            this.terminalId = terminalId;
            token = new CancellationToken();
            Task.Factory.StartNew(()=> {
                if (!BuildConfig.IsSupportBlacklist)
                {
                    PersonList = DBManager.Instance.DB.Queryable<PersonModel>().ToList();
                }
                else
                {
                    PersonList = DBManager.Instance.PersonDB.GetList(it => it.type == "8");
                }
                while (!token.IsCancellationRequested)
                {
                    CheckPersonUpdate();
                    Thread.Sleep(PollingTime);
                }
            });
        }

        

        private async void CheckPersonUpdate()
        {
            if (isUpdate) return;
            isUpdate = true;
            var ts = SharePreference.Read(Keys.KeyTs, "0");
            var rep =await ApiService.GetPersonList(terminalId, ts);
            if (rep.code != 0)
            {
                Log.I("check face lib update fail:" + rep.code);
            }
            else {
                DeletePersons(rep.deleted_data);
                InsertPersons(rep.updated_data);
                SharePreference.Write(Keys.KeyTs, rep.ts);
            }
            isUpdate = false;
        }
        int failCount = 0;
        private void InsertPersons(List<PersonModel> list)
        {
            if (list == null||list.Count<=0) return;
            var start = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            onUpdatePerson?.Invoke(true);
            IsUpdatePerson = true;
            failCount = 0;
            for(int index=0;index<list.Count;index++) {
                var model = list[index];
                UpdatePerson(model,index);
            }
            Log.I("fail picture count:"+failCount);
            IsUpdatePerson = false;
            onUpdatePerson?.Invoke(false);
            var end = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            Log.I("update start time:"+start+",end time:"+end);
            if (!BuildConfig.IsSupportBlacklist)
            {
                PersonList = DBManager.Instance.DB.Queryable<PersonModel>().ToList();
            }
            else
            {
                PersonList = DBManager.Instance.PersonDB.GetList(it=>it.type=="8");
            }
            Log.I("person list:"+(PersonList == null));
        }

        private void UpdatePerson(PersonModel model,int index)
        {
            var facePath = $@"{FileUtil.GetFaceDirPath()}\{model.person_id}.jpg";
            var avaterPath = $@"{FileUtil.GetAvaterDirPath()}\{model.person_id}.jpg";
            var success = HttpManager.DownloadFile(facePath,model.face);
            if (!success) {
                Log.I($@"UpdatePerson down face fail:{model.name},{model.person_id}");
                return;
            }
            success = HttpManager.DownloadFile(avaterPath, model.avatar);
            if (!success)
            {
                Log.I($@"UpdatePerson down avater fail:{model.name},{model.person_id}");
                return;
            }
            float[] feature = FaceManager.Instance.GetFaceFeature(facePath);            
            if (feature == null)
            {
                failCount++;
                Log.I($@"{model.person_id}.jpg can't get feature，this picture is:{model.name}");
            }
            else {
                model.feature = JsonConvert.SerializeObject(feature);
                var list = DBManager.Instance.DB.Queryable<PersonModel>()
                .Where(it => it.person_id == model.person_id).ToList();
                if (list == null || list.Count == 0)
                {
                    var result = DBManager.Instance.DB.Insertable<PersonModel>(model).ExecuteCommand();                   
                    Log.I($@"insert {model.name} {index} result:" + result);
                }
                else
                {
                    var result = DBManager.Instance.DB.Updateable<PersonModel>(model).ExecuteCommand();
                    Log.I($@"update {model.name} {index} result:" + result);
                }
            }

        }

        private void DeletePersons(List<string> ids)
        {
            if (ids == null) return;
            foreach (var id in ids)
            {
                var model = new PersonModel();
                DBManager.Instance.DB.Deleteable<PersonModel>().Where(model).ExecuteCommand();
            }
        }
    }
}
