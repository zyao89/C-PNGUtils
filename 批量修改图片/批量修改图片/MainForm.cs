using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 批量修改图片
{
    public partial class MainForm : Form
    {
        private AboutBox _mAboutBox;
        private HelpForm _mHelpForm;

        private FolderBrowserDialog _mSelectFileDialog;
        private ColorDialog _mSelectColorDialog;

        //控制子线程状态的变量
        private CancellationTokenSource cts = new CancellationTokenSource();
        private volatile bool isCancel = false;


        public MainForm()
        {
            InitializeComponent();
            _mSelectFileDialog = new FolderBrowserDialog();
            _mSelectColorDialog = new ColorDialog();
        }

        #region 以下选择文件路径

        /**
         * 源文件选择
         **/
        private void select_from_btn_Click(object sender, EventArgs e)
        {
            if (this._mSelectFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = this._mSelectFileDialog.SelectedPath;
                Console.WriteLine(path);
                from_path_textBox.Text = path;
            }
        }

        /**
         *  目标选择
         **/
        private void select_out_btn_Click(object sender, EventArgs e)
        {
            if (this._mSelectFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = this._mSelectFileDialog.SelectedPath;
                Console.WriteLine(path);
                this.out_path_textBox.Text = path;
            }
        }

        // 以下为文件拖拽
        private void from_path_textBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
                this.from_path_textBox.Cursor = System.Windows.Forms.Cursors.Arrow;  //指定鼠标形状（更好看）  
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void from_path_textBox_DragDrop(object sender, DragEventArgs e)
        {
            //GetValue(0) 为第1个文件全路径  
            //DataFormats 数据的格式，下有多个静态属性都为string型，除FileDrop格式外还有Bitmap,Text,WaveAudio等格式  
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            this.from_path_textBox.Text = path;
            this.from_path_textBox.Cursor = System.Windows.Forms.Cursors.IBeam; //还原鼠标形状  
        }

        private void out_path_textBox_DragDrop(object sender, DragEventArgs e)
        {
            //GetValue(0) 为第1个文件全路径  
            //DataFormats 数据的格式，下有多个静态属性都为string型，除FileDrop格式外还有Bitmap,Text,WaveAudio等格式  
            string path = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
            this.out_path_textBox.Text = path;
            this.out_path_textBox.Cursor = System.Windows.Forms.Cursors.IBeam; //还原鼠标形状  
        }

        private void out_path_textBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
                this.out_path_textBox.Cursor = System.Windows.Forms.Cursors.Arrow;  //指定鼠标形状（更好看）  
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        #endregion 以上选择文件路径

        #region 以下点击改变颜色

        private void change_color_Click(object sender, EventArgs e)
        {
            if (this.isCancel)
            {
                this.cts.Cancel();
                return;
            }

            if (MessageBox.Show(@"亲爱的圆仔仔，这是【开启执行】程序的按钮，你确定你没点错嘛?", @"X22温馨提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
            {
                return;
            }

            //判断选择模式
            if (this.dev_other_model_checkBox.Checked)
            {
                dev_other_model_Click(sender, e);//开发者模式
                return;
            }

            string fromPath = this.from_path_textBox.Text;
            string outPath = this.out_path_textBox.Text;
            if (!checkFilePath(fromPath) || !checkFilePath(outPath))
            {
                this.console_log_textBox.AppendText(System.Environment.NewLine + @"文件路径有问题，执行失败！" + System.Environment.NewLine);
                return;
            }

            DirectoryInfo fromInfo = new DirectoryInfo(fromPath);
            DirectoryInfo outInfo = new DirectoryInfo(outPath);
            ChangeImageUtils change = new ChangeImageUtils();
            if (this.dev_save_pic_format_checkBox.Checked)
            {
                change.setImageFormat(this.dev_save_pic_format_comboBox.Text);
            }
            else
            {
                change.setImageFormat("不设置输出格式");
            }

            StringCollection filterList = new StringCollection(); //过滤列表

            foreach (string item in this.filter_listView.Items)
            {
                filterList.Add(item);
            }

            double similarity = Convert.ToDouble(this.similarity_numericUpDown.Value); //相似度
            bool isSingleValue = this.singleValue_checkBox.Checked; //单一色相

            bool isDevNewTestModel = this.dev_new_test_model.Checked;//开发者模式 - 新算法测试

            ArrayList oldColorList = new ArrayList(); //旧颜色集合
            oldColorList.Add(this.old_color_show.BackColor);

            ArrayList newColorList = new ArrayList();//新颜色集合
            newColorList.Add(this.new_color_show.BackColor); //新颜色

            if (this.new_color_checkBox1.Checked)
            {
                newColorList.Add(this.new_color_show1.BackColor);//新颜色1
            }

            if (this.old_color_checkBox1.Checked)
            {
                oldColorList.Add(this.old_color_show1.BackColor);
            }
            if (this.old_color_checkBox2.Checked)
            {
                oldColorList.Add(this.old_color_show2.BackColor);
            }
            if (this.old_color_checkBox3.Checked)
            {
                oldColorList.Add(this.old_color_show3.BackColor);
            }
            if (this.old_color_checkBox4.Checked)
            {
                oldColorList.Add(this.old_color_show4.BackColor);
            }
            if (this.old_color_checkBox5.Checked)
            {
                oldColorList.Add(this.old_color_show5.BackColor);
            }

            //Color oldColor = this.old_color_show.BackColor;
            //Color newColor1 = this.new_color_show1.BackColor; //新颜色1

            Thread changing = new Thread(() =>
            {
                scanAllFile(fromInfo, outInfo, oldColorList, newColorList, change, filterList, similarity, isSingleValue, isDevNewTestModel);
                Invoke(new Action(() =>
                {
                    this.console_log_textBox.AppendText(System.Environment.NewLine + @"★★☆☆★★  执行结束，共计成功图片：【" + change.getCount() + @"】张！ 失败图片：【" + change.getErrorCount() + @"】张！ 过滤图片：【" + change.getFilterCount() + @"】张！ 失败文件路径个数：【" + change.getErrorPathCount() + @"】个！ ☆☆★★☆☆" + System.Environment.NewLine);
                    this.console_log_textBox.AppendText(System.Environment.NewLine + @"====================================================================================" + System.Environment.NewLine +
                                                                                     @"=                                                                                                                                                                                        =" + System.Environment.NewLine +
                                                                                     @"=                                                                             I LOVE XYY, 圆圆珍藏版!                                                                         =" + System.Environment.NewLine +
                                                                                     @"=                                                                                                                                                                                        =" + System.Environment.NewLine +
                                                                                     @"====================================================================================" + System.Environment.NewLine);
                    setEnabled(true);
                }));
                change = null;
                Console.WriteLine(@"Finish...");
            });

            setEnabled(false);
            //线程控制操作注册
            this.cts = new CancellationTokenSource();
            this.cts.Token.Register(() =>
            {//回调
                setEnabled(true);
                Console.WriteLine("工作子线程被手动终止了。");
                Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + @"☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆  你已停止了图片颜色替换操作！  ☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆" + System.Environment.NewLine)));
            });
            changing.Start();
        }

        /**
         * 扫描所有文件
         **/
        private void scanAllFile(DirectoryInfo fromInfo, DirectoryInfo outInfo, ArrayList oldColorList, ArrayList newColorList, ChangeImageUtils change, StringCollection filterList, double similarity, bool isSingleValue, bool isDevNewTestModel)
        {
            if (this.cts.Token.IsCancellationRequested)
            {
                Console.WriteLine("子线程被终止！");
                return;
            }
            if (fromInfo.ToString().Length >= 248)
            {
                //Console.WriteLine(@"文件路径过长，程序被迫终止！");
                Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" -->  文件路径过长！ 【" + fromInfo.FullName + @"】 此路径下的文件已被过滤掉了... " + System.Environment.NewLine)));
                change.setErrorPathCount();
                return;
            }
            if (!fromInfo.Exists)
            {
                return;
            }
            if (outInfo.ToString().Length >= 248)
            {
                //Console.WriteLine(@"文件路径过长，程序被迫终止！");
                Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" -->  文件路径过长！ 【" + outInfo.FullName + @"】 此路径下的文件已被过滤掉了... " + System.Environment.NewLine)));
                change.setErrorPathCount();
                return;
            }
            if (!outInfo.Exists)
            {
                outInfo.Create();
            }
            FileInfo[] finfos = fromInfo.GetFiles();
            foreach (FileInfo finfo in finfos)
            {
                if (cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine("子线程被终止！");
                    return;
                }
                if (!finfo.Exists)
                {
                    continue;
                }
                DateTime tempTime = DateTime.Now;

                string outFullFileName = (outInfo + "\\" + finfo.Name).Replace(@"\\", @"/");
                if (outFullFileName.Length >= 260)
                {
                    //Console.WriteLine(@"文件名称过长，程序被迫终止！");
                    Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" -->  文件路径名称过长！ 【" + finfo.Name + @"】 此文件已被过滤掉了... [ 耗时：" + (DateTime.Now - tempTime).TotalMilliseconds + @" ]" + System.Environment.NewLine)));
                    change.setErrorCount();
                    continue;
                }

                if (this.filter_checkBox.Checked && filterList.Contains(finfo.Name))//过滤图片
                {
                    change.copyImage(finfo.FullName, outFullFileName);//复制原图片
                    Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" --> 【" + finfo.Name + @"】 此文件已被过滤掉了... [ 耗时：" + (DateTime.Now - tempTime).TotalMilliseconds + @" ]" + System.Environment.NewLine)));
                    change.setFilterCount();
                    continue;
                }

                if (isDevNewTestModel)
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        Console.WriteLine("子线程被终止！");
                        return;
                    }

                    if (this.new_color_checkBox1.Checked)
                    {
                        change.changeImageColor(finfo.FullName, outFullFileName, oldColorList, newColorList, similarity, isSingleValue);//开发者模式-新算法2-新颜色集合
                    }
                    else
                    {
                        Color newColor = (Color)newColorList[0];
                        change.changeImageColor(finfo.FullName, outFullFileName, oldColorList, newColor, similarity, isSingleValue);//开发者模式-新算法
                    }
                }
                else
                {
                    Color newColor = (Color)newColorList[0];
                    foreach (Color oldColor in oldColorList)
                    {
                        if (cts.Token.IsCancellationRequested)
                        {
                            Console.WriteLine("子线程被终止！");
                            return;
                        }
                        if (change.changeImageColor(finfo.FullName, outFullFileName, oldColor, newColor, similarity, isSingleValue))//普通模式
                        {
                            break;
                        }
                    }
                }

                Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" --> " + outFullFileName + @" [ 耗时：" + (DateTime.Now - tempTime).TotalMilliseconds + @" ]" + System.Environment.NewLine)));

            }
            DirectoryInfo[] infos = fromInfo.GetDirectories();
            DirectoryInfo outSubInfo = outInfo;//记录根目录
            foreach (DirectoryInfo info in infos)
            {
                try
                {
                    outInfo = new DirectoryInfo(outSubInfo.FullName.Replace(@"\\", @"/") + "/" + info.Name);
                    scanAllFile(info, outInfo, oldColorList, newColorList, change, filterList, similarity, isSingleValue, isDevNewTestModel);
                }
                catch (PathTooLongException)//文件名过长
                {
                    Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" -->  文件路径过长！ 【" + outInfo.FullName + @"】 此路径下的文件已被过滤掉了... " + System.Environment.NewLine)));
                    change.setErrorPathCount();
                    continue;
                }
            }
        }

        /**
        * 检测路径是否正确存在
        */
        private bool checkFilePath(string path)
        {
            if (path.Trim() != String.Empty)
            {
                return Directory.Exists(path);
            }
            return false;
        }

        #endregion 以上点击改变颜色

        /**
        * 禁用或开启所有按钮
        */
        private void setEnabled(bool enabled)
        {
            //this.change_color_btn.Enabled = enabled;
            this.isCancel = !enabled;

            this.change_color_btn.BackgroundImage = enabled ? Properties.Resources.Battle_Bears_128px : Properties.Resources.Cuevana_Storm_128px;

            this.similarity_numericUpDown.Enabled = enabled;
            this.singleValue_checkBox.Enabled = enabled;
            this.developer_checkBox.Enabled = enabled;

            this.select_out_btn.Enabled = enabled;
            this.select_from_btn.Enabled = enabled;
            this.out_path_textBox.Enabled = enabled;
            this.from_path_textBox.Enabled = enabled;
            this.filter_listView.Enabled = enabled;
            this.filter_add_textBox.Enabled = enabled;
            this.filter_add_btn.Enabled = enabled;
            this.filter_del_btn.Enabled = enabled;
            this.filter_checkBox.Enabled = enabled;

            //所有备份颜色选项
            change_all_old_color_checkBox_enabled(enabled);

            //所有开发者功能选项
            change_all_dev_model_enabled(enabled);
        }

        #region 颜色选择点击按钮实现
        /**
         * 选择旧颜色
         * */
        private void select_old_color_btn_Click(object sender, EventArgs e)
        {
            select_color_click(this.old_color_textBox, this.old_color_show);
        }

        private void select_new_color_btn_Click(object sender, EventArgs e)
        {
            select_color_click(this.new_color_textBox, this.new_color_show);
        }

        private void select_new_color_btn1_Click(object sender, EventArgs e)
        {
            select_color_click(this.new_color_textBox1, this.new_color_show1);
        }

        //颜色选择方法
        private void select_color_click(TextBox textBox, Label show)
        {
            if (this._mSelectColorDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = ColorTranslator.ToHtml(_mSelectColorDialog.Color);
                show.BackColor = _mSelectColorDialog.Color;
            }
        }

        private void select_old_color_btn1_Click(object sender, EventArgs e)
        {
            select_color_click(this.old_color_textBox1, this.old_color_show1);
        }

        private void select_old_color_btn2_Click(object sender, EventArgs e)
        {
            select_color_click(this.old_color_textBox2, this.old_color_show2);
        }

        private void select_old_color_btn3_Click(object sender, EventArgs e)
        {
            select_color_click(this.old_color_textBox3, this.old_color_show3);
        }

        private void select_old_color_btn4_Click(object sender, EventArgs e)
        {
            select_color_click(this.old_color_textBox4, this.old_color_show4);
        }

        private void select_old_color_btn5_Click(object sender, EventArgs e)
        {
            select_color_click(this.old_color_textBox5, this.old_color_show5);
        }

        #endregion 以上颜色选择点击按钮实现

        #region 过滤列表实现

        //过滤添加
        private void filter_add_btn_Click(object sender, EventArgs e)
        {
            if (this.filter_add_textBox.Text == string.Empty)
            {
                return;
            }
            if (this.filter_listView.Items.Contains(this.filter_add_textBox.Text))
            {
                return;
            }
            this.filter_listView.Items.Add(this.filter_add_textBox.Text);
        }

        //过滤删除
        private void filter_del_btn_Click(object sender, EventArgs e)
        {
            if (this.filter_listView.Items.Count <= 0)
            {
                return;
            }
            int index = this.filter_listView.SelectedIndex;
            if (index != -1)
            {
                this.filter_listView.Items.RemoveAt(index);
            }
        }

        private void filter_listView_DragDrop(object sender, DragEventArgs e)
        {
            string[] paths = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            for (int i = 0; i < paths.Length; i++)
            {
                string name = paths[i].Substring(paths[i].LastIndexOf("\\") + 1);  //文件名
                this.filter_listView.Items.Add(name);
            }
            this.out_path_textBox.Cursor = System.Windows.Forms.Cursors.IBeam; //还原鼠标形状  
        }

        private void filter_listView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Link;
                this.out_path_textBox.Cursor = System.Windows.Forms.Cursors.Arrow;  //指定鼠标形状（更好看）  
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void filter_listView_DoubleClick(object sender, EventArgs e)
        {
            ListBox listBox = (ListBox)sender;
            if (listBox.Items.Count <= 0)
            {
                return;
            }
            int index = listBox.SelectedIndex;
            if (index != -1)
            {
                listBox.Items.RemoveAt(index);
            }
        }

        #endregion 过滤列表实现

        #region 数据保存
        private void saveDatas()
        {
            Properties.Settings.Default.SettingsOldFilePath = this.from_path_textBox.Text;
            Properties.Settings.Default.SettingsNewFilePath = this.out_path_textBox.Text;

            Properties.Settings.Default.SettingsOldColor = this.old_color_show.BackColor;
            Properties.Settings.Default.SettingsOldColorShow1 = this.old_color_show1.BackColor;
            Properties.Settings.Default.SettingsOldColorShow2 = this.old_color_show2.BackColor;
            Properties.Settings.Default.SettingsOldColorShow3 = this.old_color_show3.BackColor;
            Properties.Settings.Default.SettingsOldColorShow4 = this.old_color_show4.BackColor;
            Properties.Settings.Default.SettingsOldColorShow5 = this.old_color_show5.BackColor;
            Properties.Settings.Default.SettingsNewColorShow1 = this.new_color_show1.BackColor;//new

            Properties.Settings.Default.SettingsOldColorCheckBox1 = this.old_color_checkBox1.Checked;
            Properties.Settings.Default.SettingsOldColorCheckBox2 = this.old_color_checkBox2.Checked;
            Properties.Settings.Default.SettingsOldColorCheckBox3 = this.old_color_checkBox3.Checked;
            Properties.Settings.Default.SettingsOldColorCheckBox4 = this.old_color_checkBox4.Checked;
            Properties.Settings.Default.SettingsOldColorCheckBox5 = this.old_color_checkBox5.Checked;
            Properties.Settings.Default.SettingsNewColorCheckBox1 = this.new_color_checkBox1.Checked;//new

            Properties.Settings.Default.SettingsNewColor = this.new_color_show.BackColor;
            Properties.Settings.Default.SettingsSimilarity = this.similarity_numericUpDown.Value;
            Properties.Settings.Default.SettingIsSingleValue = this.singleValue_checkBox.Checked;
            Properties.Settings.Default.SettingIsFilterCheck = this.filter_checkBox.Checked;
            Properties.Settings.Default.SettingsDeveloperModel = this.developer_checkBox.Checked;
            //this.filter_listView.Enabled = enabled;
            //this.filter_add_textBox.Enabled = enabled;

            StringCollection filterList = new StringCollection(); //过滤列表
            foreach (string item in this.filter_listView.Items)
            {
                filterList.Add(item);
            }
            Properties.Settings.Default.SettingFilterListView = filterList;

            save_dev_model_datas();//保存开发者模式功能

            Properties.Settings.Default.Save();
        }
        #endregion 数据保存

        //程序即将关闭时，触发操作
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            saveDatas();//保存数据

            if (MessageBox.Show("亲爱的圆仔仔，你真的确定退出该应用嘛?", "X22温馨提示", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) == DialogResult.No)
            {
                e.Cancel = true;
            }
        }

        //主窗口加载
        private void MainForm_Load(object sender, EventArgs e)
        {
            StringCollection filterList = Properties.Settings.Default.SettingFilterListView; //过滤列表
            if (filterList != null)
            {//加载过滤列表
                foreach (string item in filterList)
                {
                    this.filter_listView.Items.Add(item);
                }
            }

            //初始化TextBox颜色值
            update_color_textBox(this.old_color_textBox, this.old_color_show);
            update_color_textBox(this.old_color_textBox1, this.old_color_show1);
            update_color_textBox(this.old_color_textBox2, this.old_color_show2);
            update_color_textBox(this.old_color_textBox3, this.old_color_show3);
            update_color_textBox(this.old_color_textBox4, this.old_color_show4);
            update_color_textBox(this.old_color_textBox5, this.old_color_show5);
            update_color_textBox(this.new_color_textBox, this.new_color_show);
            update_color_textBox(this.new_color_textBox1, this.new_color_show1);

            //初始化备用颜色选项
            change_old_color_checkBox_enabled(this.old_color_textBox1, this.old_color_show1, this.select_old_color_btn1, this.old_color_checkBox1.Checked);
            change_old_color_checkBox_enabled(this.old_color_textBox2, this.old_color_show2, this.select_old_color_btn2, this.old_color_checkBox2.Checked);
            change_old_color_checkBox_enabled(this.old_color_textBox3, this.old_color_show3, this.select_old_color_btn3, this.old_color_checkBox3.Checked);
            change_old_color_checkBox_enabled(this.old_color_textBox4, this.old_color_show4, this.select_old_color_btn4, this.old_color_checkBox4.Checked);
            change_old_color_checkBox_enabled(this.old_color_textBox5, this.old_color_show5, this.select_old_color_btn5, this.old_color_checkBox5.Checked);
            //new1
            change_old_color_checkBox_enabled(this.new_color_textBox1, this.new_color_show1, this.select_new_color_btn1, this.new_color_checkBox1.Checked);

            //初始化开发者模式
            DeveloperModel();

            //初始化按钮的鼠标指针悬停样式
            this.change_color_btn.Cursor = new Cursor(Properties.Resources.Pay_Per_Click.GetHicon());
        }

        //窗口关闭
        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
            Application.ExitThread(); //强制中止调用线程上的所有消息，同样面临其它线程无法正确退出的问题；
            Environment.Exit(0); //这是最彻底的退出方式，不管什么线程都被强制退出，把程序结束的很干净。
        }

        //禁用所有颜色选择
        private void change_all_old_color_checkBox_enabled(bool enabled)
        {
            //主色
            this.new_color_textBox.Enabled = enabled;
            this.new_color_show.Enabled = enabled;
            this.select_new_color_btn.Enabled = enabled;
            //目标色
            this.old_color_textBox.Enabled = enabled;
            this.old_color_show.Enabled = enabled;
            this.select_old_color_btn.Enabled = enabled;

            //备选颜色
            this.old_color_textBox1.Enabled = enabled;
            this.old_color_textBox2.Enabled = enabled;
            this.old_color_textBox3.Enabled = enabled;
            this.old_color_textBox4.Enabled = enabled;
            this.old_color_textBox5.Enabled = enabled;
            //new1
            this.new_color_textBox1.Enabled = enabled;

            this.old_color_show1.Enabled = enabled;
            this.old_color_show2.Enabled = enabled;
            this.old_color_show3.Enabled = enabled;
            this.old_color_show4.Enabled = enabled;
            this.old_color_show5.Enabled = enabled;
            //new1
            this.new_color_show1.Enabled = enabled;

            this.old_color_checkBox1.Enabled = enabled;
            this.old_color_checkBox2.Enabled = enabled;
            this.old_color_checkBox3.Enabled = enabled;
            this.old_color_checkBox4.Enabled = enabled;
            this.old_color_checkBox5.Enabled = enabled;
            //new1
            this.new_color_checkBox1.Enabled = enabled;

            this.select_old_color_btn1.Enabled = enabled;
            this.select_old_color_btn2.Enabled = enabled;
            this.select_old_color_btn3.Enabled = enabled;
            this.select_old_color_btn4.Enabled = enabled;
            this.select_old_color_btn5.Enabled = enabled;
            //new1
            this.select_new_color_btn1.Enabled = enabled;

            if (enabled)
            {
                //初始化备用颜色选项
                change_old_color_checkBox_enabled(this.old_color_textBox1, this.old_color_show1, this.select_old_color_btn1, this.old_color_checkBox1.Checked);
                change_old_color_checkBox_enabled(this.old_color_textBox2, this.old_color_show2, this.select_old_color_btn2, this.old_color_checkBox2.Checked);
                change_old_color_checkBox_enabled(this.old_color_textBox3, this.old_color_show3, this.select_old_color_btn3, this.old_color_checkBox3.Checked);
                change_old_color_checkBox_enabled(this.old_color_textBox4, this.old_color_show4, this.select_old_color_btn4, this.old_color_checkBox4.Checked);
                change_old_color_checkBox_enabled(this.old_color_textBox5, this.old_color_show5, this.select_old_color_btn5, this.old_color_checkBox5.Checked);
                //new1
                change_old_color_checkBox_enabled(this.new_color_textBox1, this.new_color_show1, this.select_new_color_btn1, this.new_color_checkBox1.Checked);
            }
        }

        //更新TextBox颜色值
        private void update_color_textBox(TextBox textBox, Label show)
        {
            textBox.Text = ColorTranslator.ToHtml(show.BackColor);
        }


        #region 颜色区域改变事件监听

        // 旧颜色监听字符改变事件
        private void old_color_textBox_TextChanged(object sender, EventArgs e)
        {
            updateShowBox_TextChanged(this.old_color_textBox, this.old_color_show);
        }

        // 新颜色监听字符改变事件
        private void new_color_textBox_TextChanged(object sender, EventArgs e)
        {
            updateShowBox_TextChanged(this.new_color_textBox, this.new_color_show);
        }

        private void new_color_textBox1_TextChanged(object sender, EventArgs e)
        {
            updateShowBox_TextChanged(this.new_color_textBox1, this.new_color_show1);
        }

        //监听TextBox改变，并随时更新展示的颜色
        private void updateShowBox_TextChanged(TextBox textBox, Label show)
        {
            string input = textBox.Text.Trim();
            if (input == String.Empty && input.Length < 3)
            {
                return;
            }
            Color color;
            if (input.StartsWith("#"))
            {
                if (!Validator.IsColorValue(input.Substring(1)))
                {
                    return;
                }
                color = ColorTranslator.FromHtml(input);
            }
            else
            {
                if (!Validator.IsColorValue(input))
                {
                    return;
                }
                color = ColorTranslator.FromHtml("#" + input);
            }
            show.BackColor = color;
        }

        //以下是监听文本，改变颜色状态
        private void old_color_textBox1_TextChanged(object sender, EventArgs e)
        {
            updateShowBox_TextChanged(this.old_color_textBox1, this.old_color_show1);
        }

        private void old_color_textBox2_TextChanged(object sender, EventArgs e)
        {
            updateShowBox_TextChanged(this.old_color_textBox2, this.old_color_show2);
        }

        private void old_color_textBox3_TextChanged(object sender, EventArgs e)
        {
            updateShowBox_TextChanged(this.old_color_textBox3, this.old_color_show3);
        }

        private void old_color_textBox4_TextChanged(object sender, EventArgs e)
        {
            updateShowBox_TextChanged(this.old_color_textBox4, this.old_color_show4);
        }

        private void old_color_textBox5_TextChanged(object sender, EventArgs e)
        {
            updateShowBox_TextChanged(this.old_color_textBox5, this.old_color_show5);
        }

        //以下是备用颜色开启禁用开关
        private void change_old_color_checkBox_enabled(TextBox textBox, Label show, Button select, bool enabled)
        {
            textBox.Enabled = enabled;
            show.Enabled = enabled;
            select.Enabled = enabled;
        }

        private void old_color_checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            change_old_color_checkBox_enabled(this.old_color_textBox1, this.old_color_show1, this.select_old_color_btn1, this.old_color_checkBox1.Checked);
        }

        private void old_color_checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            change_old_color_checkBox_enabled(this.old_color_textBox2, this.old_color_show2, this.select_old_color_btn2, this.old_color_checkBox2.Checked);
        }

        private void old_color_checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            change_old_color_checkBox_enabled(this.old_color_textBox3, this.old_color_show3, this.select_old_color_btn3, this.old_color_checkBox3.Checked);
        }

        private void old_color_checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            change_old_color_checkBox_enabled(this.old_color_textBox4, this.old_color_show4, this.select_old_color_btn4, this.old_color_checkBox4.Checked);
        }

        private void old_color_checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            change_old_color_checkBox_enabled(this.old_color_textBox5, this.old_color_show5, this.select_old_color_btn5, this.old_color_checkBox5.Checked);
        }

        private void new_color_checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            change_old_color_checkBox_enabled(this.new_color_textBox1, this.new_color_show1, this.select_new_color_btn1, this.new_color_checkBox1.Checked);
        }

        # endregion 颜色区域改变事件监听

        #region 以下是调用吸色窗口
        private void old_color_show_Click(object sender, EventArgs e)
        {
            new SelectColorForm(this.old_color_textBox, this.old_color_show).Show();
        }

        private void old_color_show1_Click(object sender, EventArgs e)
        {
            new SelectColorForm(this.old_color_textBox1, this.old_color_show1).Show();
        }

        private void old_color_show2_Click(object sender, EventArgs e)
        {
            new SelectColorForm(this.old_color_textBox2, this.old_color_show2).Show();
        }

        private void old_color_show3_Click(object sender, EventArgs e)
        {
            new SelectColorForm(this.old_color_textBox3, this.old_color_show3).Show();
        }

        private void old_color_show4_Click(object sender, EventArgs e)
        {
            new SelectColorForm(this.old_color_textBox4, this.old_color_show4).Show();
        }

        private void old_color_show5_Click(object sender, EventArgs e)
        {
            new SelectColorForm(this.old_color_textBox5, this.old_color_show5).Show();
        }

        private void new_color_show_Click(object sender, EventArgs e)
        {
            new SelectColorForm(this.new_color_textBox, this.new_color_show).Show();
        }

        private void new_color_show1_Click(object sender, EventArgs e)
        {
            new SelectColorForm(this.new_color_textBox1, this.new_color_show1).Show();
        }
        #endregion 以上是调用吸色窗口


        private void dev_other_resize_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            this.dev_other_resize_width_radioButton.Enabled = cb.Checked;
            this.dev_other_resize_height_radioButton.Enabled = cb.Checked;
            this.dev_other_resize_cut_checkBox.Enabled = cb.Checked;
        }

        private void dev_save_pic_format_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            //MessageBox.Show("二货，别乱点，这个功能没有用！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            CheckBox cb = (CheckBox)sender;
            this.dev_save_pic_format_comboBox.Enabled = cb.Checked;
        }

        #region 工具栏菜单实现

        private void MenuItemAbout_Click(object sender, EventArgs e)
        {
            if (_mAboutBox != null)
            {
                _mAboutBox.Dispose();
            }
            _mAboutBox = new AboutBox();
            _mAboutBox.Show();
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveDatas();
            MessageBox.Show("二货，配置文件信息保存成功了！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void 帮助文档HelpMeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_mHelpForm != null)
            {
                _mHelpForm.Dispose();
            }
            _mHelpForm = new HelpForm();
            _mHelpForm.Show();
        }

        private void 截取当前屏幕ScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //截屏幕方法  
            string sFileName = "";
            if (this._mSelectFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string sPath = this._mSelectFileDialog.SelectedPath + "\\";
                    Console.WriteLine(sPath);

                    Random rnd = new Random();
                    int width = 0;
                    int heigh = 0;
                    width = this.Size.Width;
                    heigh = this.Size.Height;
                    Bitmap bmp = new Bitmap(width, heigh);

                    System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
                    sFileName = sPath + ass.GetName().Name + "_" + DateTime.Now.ToString(@"yyyyMMddHHmmss_") + "_" + rnd.Next(999) + ".jpg";

                    this.DrawToBitmap(bmp, new Rectangle(0, 0, width, heigh));

                    bmp.Save(sFileName, System.Drawing.Imaging.ImageFormat.Jpeg);

                    MessageBox.Show(@"二货，截图成功了！" + System.Environment.NewLine + @"输出地址：" + sFileName, @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception)
                {
                    MessageBox.Show(@"二货，截图失败了！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        #endregion 工具栏菜单实现


        #region 区域内画矩形

        private int intStartX = 0;

        private int intStartY = 0;

        private bool isMouseDraw = false;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            isMouseDraw = true;

            intStartX = e.X;

            intStartY = e.Y;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDraw)
            {

                try
                {

                    //Image tmp = Image.FromFile("1.png");

                    Graphics g = this.pictureBox1.CreateGraphics();

                    //清空上次画下的痕迹

                    g.Clear(Color.Transparent);

                    Brush brush = new SolidBrush(Color.Red);

                    Pen pen = new Pen(brush, 1);

                    pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

                    g.DrawRectangle(pen, new Rectangle(intStartX > e.X ? e.X : intStartX, intStartY > e.Y ? e.Y : intStartY, Math.Abs(e.X - intStartX), Math.Abs(e.Y - intStartY)));

                    g.Dispose();

                    //this.pictureBox_Src.Image = tmp;

                }

                catch (Exception ex)
                {

                    ex.ToString();

                }

            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isMouseDraw = false;

            intStartX = 0;

            intStartY = 0;
        }

        #endregion 区域内画矩形

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            MessageBox.Show(@"二货，别乱点，我知道这个很性感，但是这个功能并没有用的！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }


    }
}
