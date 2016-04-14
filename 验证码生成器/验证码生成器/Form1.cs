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

namespace 验证码生成器
{
    public partial class Form1 : Form
    {
        private string resultPsw;

        public Form1()
        {
            InitializeComponent();
            this.username_textBox.Text = Environment.UserName;
            createPassword();
        }

        private void createPassword()
        {
            string key = @"ILOVEXYY";
            byte[] result = Encoding.Default.GetBytes(DateTime.Now.ToLongDateString().ToString() + this.username_textBox.Text);    //tbPass为输入密码的文本框
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] output = md5.ComputeHash(result);
            output = md5.ComputeHash(Encoding.Default.GetBytes(BitConverter.ToString(output) + key));//加密两次
            resultPsw = BitConverter.ToString(output).Replace("-", "");
            this.password_textBox.Text = resultPsw;  //password_textBox为输出加密文本的文本框
            this.sign_textBox.Text = key;
        }

        private void create_btn_Click(object sender, EventArgs e)
        {
            createPassword();
            MessageBox.Show(@"密码已生成！");
        }

        private void copy_btn_Click(object sender, EventArgs e)
        {
            if (this.password_textBox.Text != string.Empty || this.sign_textBox.Text.Trim() != string.Empty)
            {
                Clipboard.SetDataObject(this.password_textBox.Text + this.sign_textBox.Text);
                MessageBox.Show(@"密码已复制！");
            }
        }

    }
}
