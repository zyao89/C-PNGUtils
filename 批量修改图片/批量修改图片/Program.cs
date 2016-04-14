using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 批量修改图片
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoadingForm loading = new LoadingForm();
            loading.isSuccess(isSucc);
            loading.StartPosition = FormStartPosition.CenterScreen;
            Application.Run(loading);
        }

        private static void isSucc(bool succ)
        {
            if (succ)
            {
                MainForm main = new MainForm();
                main.StartPosition = FormStartPosition.CenterScreen;
                main.Show();
            }
            else
            {
                MessageBox.Show("验证失败！系统强制退出！");
                Application.Exit();
            }
        }
    }
}
