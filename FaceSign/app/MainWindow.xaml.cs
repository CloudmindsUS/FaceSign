using FaceSign.app;
using FaceSign.data;
using FaceSign.db;
using FaceSign.http;
using FaceSign.http.rep;
using FaceSign.http.req;
using FaceSign.log;
using FaceSign.model;
using FaceSign.server;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FaceSign
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            if (!BuildConfig.IsSupportAccessControl)
            {
                SerialPortSetting.Visibility = Visibility.Hidden;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //PersonModel model = new PersonModel
            //{
            //    person_id = "232",
            //    name = "张益达1",
            //    type="1",
            //    person_no = "1111",
            //    uuid = "222",
            //    gender = "男",
            //    face = "2",
            //    rfid = "3",
            //    fingerprint_id = "4",
            //    voice_prompt = "5"
            //};
            //var result = DBManager.Instance.DB.Updateable<PersonModel>(model).ExecuteCommand();
            //Log.I("更新数据结果:" + result);
            if (TerminalId.Text == null || TerminalId.Text.Trim() == "") {
                MessageBox.Show(R.Str.terminal_id_can_not_null, R.Str.dialog_tip, MessageBoxButton.OK);
                return;
            }
            string id = TerminalId.Text.Trim();
            BindDevice(id);

        }

        private void SavePicture(string text)
        {
            var buffer = Convert.FromBase64String(text);
            var fs = new FileStream("save.jpg",FileMode.Create);
            fs.Write(buffer,0,buffer.Length);
            fs.Flush();
            fs.Close();
        }

        private async void BindDevice(string id)
        {
            var request = new BindRequest
            {
                hardkey = id,
                terminal_id = id
            };
            var rep = await ApiService.Bind(request);
            if (rep.code != 0) {
                MessageBox.Show($@"{R.Str.login_fail}[{rep.message}]",R.Str.dialog_tip,MessageBoxButton.OK);
                return;
            }
            SharePreference.Write(Keys.KeyTerminalId, id);
            SharePreference.Write(Keys.KeyHardkey, id);
            PersonsWindow window = new PersonsWindow(id);
            window.Show();
            Close();
        }

        private void SerialPortSetting_Click(object sender, RoutedEventArgs e)
        {
            new SerialPortWindow().Show();
        }
    }
}
