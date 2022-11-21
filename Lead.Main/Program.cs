using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lead.Main
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //var x = new DevMainForm();
            //x.StartPosition = FormStartPosition.CenterScreen;
            //Application.Run(x);

            FileInfo fi = new FileInfo(Application.ExecutablePath);
            System.Diagnostics.Process[] ps = System.Diagnostics.Process.GetProcessesByName(fi.Name.Replace(fi.Extension, ""));

            if (ps.Length > 1)
            {
                MessageBox.Show($"{fi.Name}已经开启了进程，不要重复操作！");
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                var x = new DevMainForm();
                x.StartPosition = FormStartPosition.CenterScreen;
                Application.Run(x);
            }
        }
    }
}
