using FaceSign.log;
using FaceSign.utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FaceSign.widget
{
    public class PluginContainer: ContentControl
    {
        public PluginContainer(string appFileName, IntPtr hostHandle)
        {
            _appFilename = appFileName;
            _hostWinHandle = hostHandle;
        }

        private void OpenExternProcess(int width, int height)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo(_appFilename);
                info.UseShellExecute = true;
                //info.WindowStyle = ProcessWindowStyle.Minimized;
                info.WindowStyle = ProcessWindowStyle.Hidden;

                AppProcess = System.Diagnostics.Process.Start(info);
                // Wait for process to be created and enter idle condition
                AppProcess.WaitForInputIdle();
                while (AppProcess.MainWindowHandle == IntPtr.Zero)
                {
                    Thread.Sleep(5);
                }
                Log.I("进程启动成功");
            }
            catch (Exception ex)
            {
                Log.I("启动进程失败:"+ex.Message);
            }
        }
        public bool EmbedProcess(int width, int height)
        {
            OpenExternProcess(width, height);
            try
            {
                var pluginWinHandle = AppProcess.MainWindowHandle;//Get the handle of main window.
                embedResult = Win32Api.SetParent(pluginWinHandle, _hostWinHandle);//set parent window
                Win32Api.SetWindowLong(new HandleRef(this, pluginWinHandle), Win32Api.GWL_STYLE, Win32Api.WS_VISIBLE);//Set window style to "None".
                var moveResult = Win32Api.MoveWindow(pluginWinHandle, 0, 0, width, height, true);//Move window to fixed position(up-left is (0,0), and low-right is (width, height)).
                //embed failed, and tries again
                if (!moveResult || embedResult == 0)
                {
                    AppProcess.Kill();
                    if (MAXCOUNT-- > 0)
                    {
                        EmbedProcess(width, height);
                    }
                }
                else
                {
                    Win32Api.ShowWindow(pluginWinHandle, (short)Win32Api.SW_MAXIMIZE);
                }
            }
            catch (Exception ex)
            {
                var errorString = Win32Api.GetLastError();
                MessageBox.Show(errorString + ex.Message);
            }
            return (embedResult != 0);
        }

        #region

        public int embedResult = 0;

        public Process AppProcess { get; set; }
        private IntPtr _hostWinHandle { get; set; }

        private string _appFilename = "";

        private int MAXCOUNT = 10;

        #endregion


    }
}
