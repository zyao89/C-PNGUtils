using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 批量修改图片
{
    public partial class LoadingForm : Form
    {
        public delegate void OnCallBack(bool success);

        private OnCallBack onCallBack;//回调
        private string resultPsw;

        public LoadingForm()
        {
            InitializeComponent();

            string key = @"ILOVEXYY";
            byte[] result = Encoding.Default.GetBytes(DateTime.Now.ToLongDateString().ToString() + Environment.UserName);    //tbPass为输入密码的文本框
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            output = md5.ComputeHash(Encoding.Default.GetBytes(BitConverter.ToString(output) + key));//加密两次
            //this.password_textBox.Text = BitConverter.ToString(output).Replace("-", "");  //password_textBox为输出加密文本的文本框
            resultPsw = BitConverter.ToString(output).Replace("-", "") + key;
            this.password_textBox.Text = Properties.Settings.Default.SettingsPassword;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (onCallBack != null)
            {
                this.onCallBack(this.password_textBox.Text == resultPsw);
                this.Hide();

                //存储设置信息
                Properties.Settings.Default.SettingsPassword = this.password_textBox.Text.Trim();
                Properties.Settings.Default.Save();
            }
            else
            {
                this.Dispose();
            }
        }

        public void isSuccess(OnCallBack callback)
        {
            this.onCallBack = callback;
        }

        private void LoadingForm_Shown(object sender, EventArgs e)
        {
            //自定验证登陆
            if (Properties.Settings.Default.SettingsPassword.Equals(resultPsw))
            {
                if (this.onCallBack != null)
                {
                    this.Hide();
                    this.onCallBack(true);
                }
            }
        }
    }
}
