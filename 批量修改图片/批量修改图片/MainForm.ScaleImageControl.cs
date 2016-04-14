using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 批量修改图片
{
    partial class MainForm
    {
        #region 比例输出功能

        private void dev_other_scale_sure_button_Click(object sender, EventArgs e)
        {
            this.dev_other_scale_width_textBox.Text = this.dev_other_scale_width_textBox.Text.Trim();
            this.dev_other_scale_height_textBox.Text = this.dev_other_scale_height_textBox.Text.Trim();
            if (this.dev_other_scale_width_textBox.Text == string.Empty || this.dev_other_scale_height_textBox.Text == string.Empty)
            {
                MessageBox.Show("二货，你输入的尺寸不能为空！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            this.dev_other_scale_width_textBox.Enabled = false;
            this.dev_other_scale_height_textBox.Enabled = false;
            this.dev_other_scale_change_button.Enabled = true;
            this.dev_other_scale_sure_button.Enabled = false;
        }

        private void dev_other_scale_change_button_Click(object sender, EventArgs e)
        {
            this.dev_other_scale_width_textBox.Enabled = true;
            this.dev_other_scale_height_textBox.Enabled = true;
            this.dev_other_scale_sure_button.Enabled = true;
            this.dev_other_scale_change_button.Enabled = false;
        }

        private void dev_other_scale_textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Trim() == string.Empty)
            {
                return;
            }
            if (!Validator.IsIntegerPositive(tb.Text.Trim()))
            {
                MessageBox.Show("二货，你输入的尺寸不对！请重新输入！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tb.Text = "1";
            }

        }

        private void dev_other_scale_add_button_Click(object sender, EventArgs e)
        {
            string width = this.dev_other_scale_width_add_textBox.Text.Trim();
            string height = this.dev_other_scale_height_add_textBox.Text.Trim();
            if (height == string.Empty || width == string.Empty)
            {
                Console.WriteLine("输入的尺寸为空！");
                MessageBox.Show(@"输入的尺寸为空！");
                return;
            }
            if (!Validator.IsIntegerPositive(height))
            {
                Console.WriteLine("输入的高度有误！");
                MessageBox.Show(@"输入的高度有误！");
                return;
            }
            if (!Validator.IsIntegerPositive(width))
            {
                Console.WriteLine("输入的宽度有误！");
                MessageBox.Show(@"输入的宽度有误！");
                return;
            }
            string result = width + "px * " + height + "px";
            if (!this.dev_other_scale_add_listBox.Items.Contains(result))
            {
                this.dev_other_scale_add_listBox.Items.Add(result);
            }
        }

        private void dev_other_scale_del_button_Click(object sender, EventArgs e)
        {
            if (this.dev_other_scale_add_listBox.Items.Count <= 0)
            {
                return;
            }
            int index = this.dev_other_scale_add_listBox.SelectedIndex;
            if (index != -1)
            {
                this.dev_other_scale_add_listBox.Items.RemoveAt(index);
            }
        }

        #endregion

        #region 比例输出实现

        //开发者模式中，其他功能开启后的点击算法实现
        private void dev_other_Scale_model_Click(object sender, EventArgs e)
        {
            string fromPath = this.from_path_textBox.Text;
            string outPath = this.out_path_textBox.Text;
            if (!checkFilePath(fromPath) || !checkFilePath(outPath))
            {
                this.console_log_textBox.AppendText(System.Environment.NewLine + @"文件路径有问题，执行失败！" + System.Environment.NewLine);
                return;
            }

            if (this.dev_other_scale_add_listBox.Items.Count <= 0)
            {
                Console.WriteLine("尺寸列表为空。。。");
                return;
            }


            DirectoryInfo fromInfo = new DirectoryInfo(fromPath);
            DirectoryInfo outInfo = new DirectoryInfo(outPath);
            ChangeImageUtils changeUtils = new ChangeImageUtils();
            if (this.dev_save_pic_format_checkBox.Checked)
            {
                changeUtils.setImageFormat(this.dev_save_pic_format_comboBox.Text);
            }
            else
            {
                changeUtils.setImageFormat("不设置输出格式");
            }

            StringCollection filterList = new StringCollection(); //过滤列表
            foreach (string item in this.filter_listView.Items)
            {
                filterList.Add(item);
            }

            Thread changing = new Thread(() =>
            {
                if (cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine("子线程被终止！");
                    return;
                }
                foreach (string item in this.dev_other_scale_add_listBox.Items)
                {
                    Size newSize = handleSize(item);//尺寸列表
                    if (newSize.IsEmpty)
                    {
                        Console.WriteLine("尺寸解析有问题。。。");
                        return;
                    }
                    string outFullFile = outInfo + "\\" + item.Replace(" * ", "_");//根据尺寸划分路径
                    if (this.dev_other_scale_height_lock_radioButton.Checked)
                    {
                        outFullFile = outInfo + "\\S_H_" + item.Replace(" * ", "_");//锁定高度路径
                    }
                    else if (this.dev_other_scale_width_lock_radioButton.Checked)
                    {
                        outFullFile = outInfo + "\\S_W_" + item.Replace(" * ", "_");//锁定宽度路径
                    }

                    DirectoryInfo outCurrInfo = new DirectoryInfo(outFullFile);
                    otherScaleScanAllFile(fromInfo, outCurrInfo, newSize, changeUtils, filterList);
                }
                Invoke(new Action(() =>
                {
                    this.console_log_textBox.AppendText(System.Environment.NewLine + @"★☆★  执行结束，共计成功图片：【" + changeUtils.getCount() + @"】张！ 失败图片：【" + changeUtils.getErrorCount() + @"】张！ 过滤图片：【" + changeUtils.getFilterCount() + @"】张！ 失败文件路径个数：【" + changeUtils.getErrorPathCount() + @"】个！ ☆★☆" + System.Environment.NewLine);
                    this.console_log_textBox.AppendText(System.Environment.NewLine + @"====================================================================================" + System.Environment.NewLine +
                                                                                     @"=                                                                                                                                                                                        =" + System.Environment.NewLine +
                                                                                     @"=                                                                             I LOVE XYY, 圆圆珍藏版!                                                                         =" + System.Environment.NewLine +
                                                                                     @"=                                                                                                                                                                                        =" + System.Environment.NewLine +
                                                                                     @"====================================================================================" + System.Environment.NewLine);
                    setEnabled(true);
                }));
                changeUtils = null;
                Console.WriteLine(@"Finish...");
            });

            setEnabled(false);
            //线程控制操作注册
            this.cts = new CancellationTokenSource();
            this.cts.Token.Register(() =>
            {//回调
                setEnabled(true);
                Console.WriteLine("工作子线程被手动终止了。");
                Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + @"☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆  你已停止了尺寸大小修改操作！  ☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆" + System.Environment.NewLine)));
            });
            changing.Start();
        }

        /**
        * 扫描所有文件
        **/
        private void otherScaleScanAllFile(DirectoryInfo fromInfo, DirectoryInfo outInfo, Size newSize, ChangeImageUtils change, StringCollection filterList)
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
                else
                {
                    int mode = ChangeImageUtils.MODE_UNKNOW;//选择模式
                    if (this.dev_other_scale_height_lock_radioButton.Checked)
                    {
                        mode = ChangeImageUtils.MODE_SCALE_HEIGHT;
                    }
                    else if (this.dev_other_scale_width_lock_radioButton.Checked)
                    {
                        mode = ChangeImageUtils.MODE_SCALE_WIDTH;
                    }
                    //比例输出
                    int oldW = Convert.ToInt32(dev_other_scale_width_textBox.Text.Trim());//标准宽度
                    int oldH = Convert.ToInt32(dev_other_scale_height_textBox.Text.Trim());//标准高度
                    if (!change.scaleImage(finfo.FullName, outFullFileName, oldW, oldH, newSize.Width, newSize.Height, mode))
                    {
                        logCatOut("====== ★☆★☆★ ====== 文件名为：" + finfo.Name + " ， 输出失败！");
                    };

                    Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" --> " + outFullFileName + @" [ 耗时：" + (DateTime.Now - tempTime).TotalMilliseconds + @" ]" + System.Environment.NewLine)));
                }

            }
            DirectoryInfo[] infos = fromInfo.GetDirectories();
            DirectoryInfo outSubInfo = outInfo;//记录根目录
            foreach (DirectoryInfo info in infos)
            {
                try
                {
                    outInfo = new DirectoryInfo(outSubInfo.FullName.Replace(@"\\", @"/") + "/" + info.Name);
                    otherScaleScanAllFile(info, outInfo, newSize, change, filterList);
                }
                catch (PathTooLongException)
                {
                    Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" -->  文件路径过长！ 【" + outInfo.FullName + @"】 此路径下的文件已被过滤掉了... " + System.Environment.NewLine)));
                    change.setErrorPathCount();
                    continue;
                }
            }
        }

        #endregion
    }
}
