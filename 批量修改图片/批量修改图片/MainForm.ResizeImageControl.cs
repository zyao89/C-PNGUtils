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
        //开发者模式中，其他功能开启后的点击算法实现
        private void dev_other_resize_model_Click(object sender, EventArgs e)
        {
            string fromPath = this.from_path_textBox.Text;
            string outPath = this.out_path_textBox.Text;
            if (!checkFilePath(fromPath) || !checkFilePath(outPath))
            {
                this.console_log_textBox.AppendText(System.Environment.NewLine + @"文件路径有问题，执行失败！" + System.Environment.NewLine);
                return;
            }

            if (this.dev_other_resize_listBox.Items.Count <= 0)
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

            bool isResizeModel = this.dev_other_resize_checkBox.Checked;//比例模式

            bool isCanCut = this.dev_other_resize_cut_checkBox.Checked;//允许裁剪

            Thread changing = new Thread(() =>
            {
                if (cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine("子线程被终止！");
                    return;
                }
                foreach (string item in this.dev_other_resize_listBox.Items)
                {
                    Size newSize = handleSize(item);//尺寸列表
                    if (newSize.IsEmpty)
                    {
                        Console.WriteLine("尺寸解析有问题。。。");
                        return;
                    }
                    string outFullFile = outInfo + "\\" + item.Replace(" * ", "_");//根据尺寸划分路径
                    if (isResizeModel)
                    {
                        if (this.dev_other_resize_height_radioButton.Checked)
                        {
                            outFullFile = outInfo + "\\R_H_" + item.Replace(" * ", "_");//锁定高度路径
                        }
                        else if (this.dev_other_resize_width_radioButton.Checked)
                        {
                            outFullFile = outInfo + "\\R_W_" + item.Replace(" * ", "_");//锁定宽度路径
                        }
                    }
                    DirectoryInfo outCurrInfo = new DirectoryInfo(outFullFile);
                    otherScanAllFile(fromInfo, outCurrInfo, newSize, changeUtils, filterList, isResizeModel, isCanCut);
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
        * 解析出尺寸集合
        */
        private Size handleSize(string sizes)
        {
            string[] point = sizes.Split('*');
            if (point.Length >= 2)
            {
                int width = Convert.ToInt32(point[0].Trim().Substring(0, point[0].Trim().Length - 2));
                int height = Convert.ToInt32(point[1].Trim().Substring(0, point[1].Trim().Length - 2));
                return new Size(width, height);
            }
            return new Size();
        }

        /**
        * 扫描所有文件
        **/
        private void otherScanAllFile(DirectoryInfo fromInfo, DirectoryInfo outInfo, Size newSize, ChangeImageUtils change, StringCollection filterList, bool isResizeModel, bool isCanCut)
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
                    if (isResizeModel)
                    {
                        int mode = ChangeImageUtils.MODE_UNKNOW;//选择模式
                        if (this.dev_other_resize_height_radioButton.Checked)
                        {//锁定高度
                            if (isCanCut)
                            {
                                mode = ChangeImageUtils.MODE_CUT_HEIGHT;
                            }
                            else
                            {
                                mode = ChangeImageUtils.MODE_HEIGHT;
                            }
                        }
                        else if (this.dev_other_resize_width_radioButton.Checked)
                        {//锁定宽度
                            if (isCanCut)
                            {
                                mode = ChangeImageUtils.MODE_CUT_WIDTH;
                            }
                            else
                            {
                                mode = ChangeImageUtils.MODE_WIDTH;
                            }
                        }
                        else//不锁定
                        { }

                        if (!change.reSizeImage(finfo.FullName, outFullFileName, newSize.Width, newSize.Height, mode))
                        {//执行失败
                            logCatOut("文件名为：" + finfo.Name + " ， 输出失败！");
                        }
                    }
                    else
                    {//不锁定比例
                        if (!change.reSizeImage(finfo.FullName, outFullFileName, newSize.Width, newSize.Height))
                        {
                            logCatOut("文件名为：" + finfo.Name + " ， 输出失败！");
                        };
                    }

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
                    otherScanAllFile(info, outInfo, newSize, change, filterList, isResizeModel, isCanCut);
                }
                catch (PathTooLongException)
                {
                    Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" -->  文件路径过长！ 【" + outInfo.FullName + @"】 此路径下的文件已被过滤掉了... " + System.Environment.NewLine)));
                    change.setErrorPathCount();
                    continue;
                }
            }
        }

        //log日志输出
        private void logCatOut(string text)
        {
            Invoke(new Action(() => this.console_log_textBox.AppendText(System.Environment.NewLine + DateTime.Now.ToString("HH:mm:ss:fff") + @" --> ### " + text + " ### " + System.Environment.NewLine)));
        }

        //增加列表
        private void dev_other_resize_add_btn_Click(object sender, EventArgs e)
        {
            string height = this.dev_other_resize_height_textBox.Text.Trim();
            string width = this.dev_other_resize_width_textBox.Text.Trim();
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
            if (!this.dev_other_resize_listBox.Items.Contains(result))
            {
                this.dev_other_resize_listBox.Items.Add(result);
            }
        }

        //删除列表
        private void dev_other_resize_del_btn_Click(object sender, EventArgs e)
        {
            if (this.dev_other_resize_listBox.Items.Count <= 0)
            {
                return;
            }
            int index = this.dev_other_resize_listBox.SelectedIndex;
            if (index != -1)
            {
                this.dev_other_resize_listBox.Items.RemoveAt(index);
            }
        }
    }
}
