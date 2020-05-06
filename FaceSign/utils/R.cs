using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FaceSign.utils
{
    public class R
    {
        public static ResourceDictionary Resource {
            get {
                var langName = "";
                var culture = System.Threading.Thread.CurrentThread.CurrentCulture.Name;
                if (culture.Equals("zh-CN"))
                {
                    langName = "zh";
                }
                else
                {
                    langName = "en";
                }
                var lang = Application.LoadComponent(new Uri(@"language\" + langName + ".xaml", UriKind.Relative)) as ResourceDictionary;
                return lang;
            }
        }

        public static class Str {

            public static string login { get { return Resource["login"].ToString(); } }
            public static string main_window_title { get { return Resource["main_window_title"].ToString(); } }
            public static string update_face_lib { get { return Resource["update_face_lib"].ToString(); } }
            public static string pause_face_recognition { get { return Resource["pause_face_recognition"].ToString(); } }
            public static string dialog_tip { get { return Resource["dialog_tip"].ToString(); } }
            public static string terminal_id_can_not_null { get { return Resource["terminal_id_can_not_null"].ToString(); } }
            public static string login_fail { get { return Resource["login_fail"].ToString(); } }
            public static string serial_port_setting { get { return Resource["serial_port_setting"].ToString(); } }
            public static string txt_serial_port_name { get { return Resource["txt_serial_port_name"].ToString(); } }
            public static string txt_query_interval { get { return Resource["txt_query_interval"].ToString(); } }
            public static string txt_alarm_threshold { get { return Resource["txt_alarm_threshold"].ToString(); } }
            public static string txt_quiet_threshold { get { return Resource["txt_quiet_threshold"].ToString(); } }
            public static string txt_continue_count { get { return Resource["txt_continue_count"].ToString(); } }
            public static string txt_save_config { get { return Resource["txt_save_config"].ToString(); } }
            public static string txt_alarm_temperature { get { return Resource["txt_alarm_temperature"].ToString(); } }
            public static string txt_query_interval_error { get { return Resource["txt_query_interval_error"].ToString(); } }
            public static string txt_alarm_threshold_error { get { return Resource["txt_alarm_threshold_error"].ToString(); } }
            public static string txt_quiet_threshold_error { get { return Resource["txt_quiet_threshold_error"].ToString(); } }
            public static string txt_continue_count_error { get { return Resource["txt_continue_count_error"].ToString(); } }
            public static string txt_alarm_temperature_error { get { return Resource["txt_alarm_temperature_error"].ToString(); } }
            public static string txt_close_door_delay_error { get { return Resource["txt_close_door_delay_error"].ToString(); } }
            public static string txt_open_door_delay_error { get { return Resource["txt_open_door_delay_error"].ToString(); } }
            public static string txt_pass_person_count { get { return Resource["txt_pass_person_count"].ToString(); } }
        }
    }
}
