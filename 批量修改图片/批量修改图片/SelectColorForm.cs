using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 批量修改图片
{
    public partial class SelectColorForm : Form
    {
        private TextBox _Main_colorTextBox;
        private Label _Main_showColor;
        private Color mCurrColor;
        private Timer mTime;
        private IntPtr hwnd;
        private IntPtr hdc;
        private bool isShowTop_Left = true;//当前面板展示位置在左上方

        [DllImport("user32.dll")]//取设备场景
        private static extern IntPtr GetDC(IntPtr hwnd);//返回设备场景句柄

        [DllImport("gdi32.dll")]//取指定点颜色
        private static extern int GetPixel(IntPtr hdc, Point p);

        /// <summary>
        /// 释放由调用GetDC函数获取的指定设备场景
        /// </summary>
        /// <param name="hwnd">要释放的设备场景相关的窗口句柄</param>
        /// <param name="hdc">要释放的设备场景句柄</param>
        /// <returns>执行成功为1，否则为0</returns>
        [DllImport("user32.dll")]
        public static extern Int32 ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int y, int Width, int Height, int flags);
        /// <summary>
        /// 得到当前活动的窗口
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern System.IntPtr GetForegroundWindow();


        public SelectColorForm(TextBox colorTextBox, Label showColor)
        {
            InitializeComponent();

            //背景透明
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.BackColor = Color.Gold;
            this.TransparencyKey = this.BackColor;

            //传参
            this._Main_colorTextBox = colorTextBox;
            this._Main_showColor = showColor;

            //this.MaximizeBox = false;  
            this.MinimizeBox = false;
            this.DesktopBounds = Screen.PrimaryScreen.Bounds; // 在桌面区域全屏显示。  
            this.FormBorderStyle = FormBorderStyle.None;
            this.AutoSizeMode = AutoSizeMode.GrowAndShrink; // 禁用手动调整窗体大小。  
            this.SizeGripStyle = SizeGripStyle.Hide;        // 隐藏窗口大小调整手柄。  

            this.ShowInTaskbar = false;//窗口不显示在任务栏中

            //this.Cursor = System.Windows.Forms.Cursors.PanNE;

            //鼠标样式
            Bitmap cursor = Properties.Resources.location_arrow_outline_48_25px;
            SetCursor(cursor, new Point(0, 0));
        }

        private void SelectColorForm_Load(object sender, EventArgs e)
        {
            SetWindowPos(this.Handle, -1, 0, 0, 0, 0, 1 | 2); //最后参数也有用1 | 4　

            //this.BackColor = Color.White;
            //this.TransparencyKey = Color.White;
            //this.Opacity = 0.01;


            //取色线程
            this.mTime = new System.Windows.Forms.Timer();
            this.mTime.Interval = 1;
            this.mTime.Tick += delegate
            {
                this.hwnd = new IntPtr(0);
                Point p = new Point(MousePosition.X, MousePosition.Y);//取置顶点坐标
                this.hdc = GetDC(hwnd);//取到设备场景(0就是全屏的设备场景)
                int c = GetPixel(hdc, p);//取指定点颜色

                this.point_label.Text = @"当前鼠标位置： X = " + MousePosition.X + @" ， Y = " + MousePosition.Y;

                int r = (c & 0xFF);//转换R
                this.r_color_textBox.Text = r.ToString();
                int g = (c & 0xFF00) / 256;//转换G
                this.g_color_textBox.Text = g.ToString();
                int b = (c & 0xFF0000) / 65536;//转换B
                this.b_color_textBox.Text = b.ToString();
                this.mCurrColor = Color.FromArgb(r, g, b);

                this.binary_color_textBox.Text = Convert.ToString(c, 2);
                this.octal_color_textBox.Text = Convert.ToString(c, 8);
                this.decimal_color_textBox.Text = Convert.ToString(c, 10);
                this.hexadecimal_color_textBox.Text = ColorTranslator.ToHtml(mCurrColor);

                this.hsb_h_color_textBox.Text = Math.Round(mCurrColor.GetHue(),2).ToString();
                this.hsb_s_color_textBox.Text = Math.Round(mCurrColor.GetSaturation(),2).ToString();
                this.hsb_b_color_textBox.Text = Math.Round(mCurrColor.GetBrightness(),2).ToString();
                this.show_color.BackColor = mCurrColor;

                //Console.WriteLine("mCurrColor --> " + mCurrColor.ToString());
            };
            mTime.Start();
        }

        private void exit_btn_Click(object sender, EventArgs e)
        {
            this.mTime.Stop();
            this.mTime.Dispose();
            ReleaseDC(this.hwnd, this.hdc);
            this.Dispose();//关闭当前窗口,以后不可以调用.
        }

        //鼠标样式
        public void SetCursor(Bitmap cursor, Point hotPoint)
        {
            int hotX = hotPoint.X;
            int hotY = hotPoint.Y;

            Bitmap myNewCursor = new Bitmap(cursor.Width * 2 - hotX, cursor.Height * 2 - hotY);
            Graphics g = Graphics.FromImage(myNewCursor);
            g.Clear(Color.FromArgb(0, 0, 0, 0));
            g.DrawImage(cursor, cursor.Width - hotX, cursor.Height - hotY, cursor.Width, cursor.Height);

            this.Cursor = new Cursor(myNewCursor.GetHicon());

            g.Dispose();
            myNewCursor.Dispose();
        }

        private void SelectColorForm_MouseClick(object sender, MouseEventArgs e)
        {
            this.mTime.Stop();
            this.mTime.Dispose();
            ReleaseDC(this.hwnd, this.hdc);

            if (e.Button == MouseButtons.Right)
            {
                //右键
            }
            if (e.Button == MouseButtons.Left)
            {
                this._Main_colorTextBox.Text = ColorTranslator.ToHtml(mCurrColor);
                this._Main_showColor.BackColor = mCurrColor;
            }

            this.Dispose();//关闭当前窗口,以后不可以调用.
        }


        //去除边框颜色
        private void groupBox1_Paint(object sender, PaintEventArgs e)
        {
            GroupBox box = (GroupBox)sender;
            e.Graphics.Clear(SystemColors.Control);
            e.Graphics.DrawString(box.Text, box.Font, Brushes.Black, 0, 0);
        }

        private void groupBox1_MouseHover(object sender, EventArgs e)
        {
            if (this.isShowTop_Left)
            {
                this.groupBox1.Location = new Point(this.DesktopBounds.Width - this.groupBox1.Width, this.DesktopBounds.Height - this.groupBox1.Height);
                this.isShowTop_Left = false;
                //Console.WriteLine("true --> this.groupBox1.Width: " + this.groupBox1.Width + " ; this.groupBox1.Width: " + this.groupBox1.Height);
            }
            else
            {
                this.groupBox1.Location = new Point(0, 0);
                this.isShowTop_Left = true;
                //Console.WriteLine("false --> this.groupBox1.Width: " + this.groupBox1.Width + " ; this.groupBox1.Width: " + this.groupBox1.Height);
            }
        }
    }
}
