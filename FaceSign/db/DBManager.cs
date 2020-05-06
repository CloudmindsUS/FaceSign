using FaceSign.model;
using FaceSign.utils;
using SQLite;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceSign.db
{
    public class DBManager
    {
        public static DBManager Instance = new DBManager();

        public SqlSugarClient DB;

        private string DBName = "FaceSign.db";
        private DBManager() {
            DB = new SqlSugarClient(new ConnectionConfig() {
                ConnectionString = $@"Data Source={FileUtil.GetAppRootPath()}\{DBName};",
                DbType = DbType.Sqlite,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute
            });
            DB.Aop.OnLogExecuting = (sql, pars) =>
            {
                Console.WriteLine(sql + "\r\n" +
                DB.Utilities.SerializeObject(pars.ToDictionary(it => it.ParameterName, it => it.Value)));
                Console.WriteLine();
            };
            DB.CodeFirst.InitTables(typeof(PersonModel));
        }
    }
}
