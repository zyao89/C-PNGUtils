using System;
using System.Collections;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace 批量修改图片
{
    partial class MainForm
    {
        //开发者模式初始化
        private void DeveloperModel()
        {
            //初始化更多功能，禁用颜色选择
            this.select_color_groupBox.Enabled = !Properties.Settings.Default.DevOtherModel;
            //颜色选择增强模块
            this.select_color_ext_groupBox.Enabled = !Properties.Settings.Default.DevOtherModel;

            //初始化尺寸列表
            if (Properties.Settings.Default.DevOtherResizeList != null)
            {
                foreach (string item in Properties.Settings.Default.DevOtherResizeList)
                {
                    this.dev_other_resize_listBox.Items.Add(item);
                }
            }

            //初始化比例模式
            this.dev_other_resize_checkBox.Checked = Properties.Settings.Default.DevOtherResizeRadioButton;
            //允许裁剪
            this.dev_other_resize_cut_checkBox.Checked = Properties.Settings.Default.DevOtherResizeCutRadioButton;
            //开启选择图片保存格式
            this.dev_save_pic_format_checkBox.Checked = Properties.Settings.Default.DevOtherSavePicFormatCheckBox;
            //图片格式默认为png
            this.dev_save_pic_format_comboBox.SelectedIndex = 0;

            //初始化按比例尺寸列表
            if (Properties.Settings.Default.DevOtherScaleAddListBox != null)
            {
                foreach (string item in Properties.Settings.Default.DevOtherScaleAddListBox)
                {
                    this.dev_other_scale_add_listBox.Items.Add(item);
                }
            }

            //新算法初始化，新颜色初始化
            if (!this.dev_new_test_model.Checked)
            {
                this.new_color_checkBox1.Checked = false;
            }
            this.new_color_checkBox1.Visible = this.dev_new_test_model.Checked;
            this.new_color_textBox1.Visible = this.dev_new_test_model.Checked;
            this.new_color_show1.Visible = this.dev_new_test_model.Checked;
            this.select_new_color_btn1.Visible = this.dev_new_test_model.Checked;

            //开发者模块初始化
            if (this.developer_checkBox.Checked)
            {
                this.Width = 1150;
            }
            else
            {
                this.Width = 865;
                this.dev_other_model_checkBox.Checked = false;//强制关闭开发模块中的其他功能
            }
        }

        //新增颜色算法开关
        private void dev_new_test_model_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            this.new_color_checkBox1.Enabled = cb.Checked;
            this.new_color_checkBox1.Visible = cb.Checked;
            this.new_color_textBox1.Visible = cb.Checked;
            this.new_color_show1.Visible = cb.Checked;
            this.select_new_color_btn1.Visible = cb.Checked;
        }

        //开发者模式开关
        private void developer_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.Checked)
            {
                this.Width += 290;
            }
            else
            {
                this.Width -= 290;
            }
        }

        //更多功能开关
        private void dev_other_model_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            this.dev_other_model_groupBox.Enabled = cb.Checked;
            this.select_color_groupBox.Enabled = !cb.Checked;//禁用颜色选择区域
            //change_all_old_color_checkBox_enabled(!cb.Checked);//禁用颜色选择所有东西
            this.select_color_ext_groupBox.Enabled = !cb.Checked;//增强设置
        }

        //改变所有开发者功能选项使能
        private void change_all_dev_model_enabled(bool enabled)
        {
            //新测试功能
            this.dev_new_test_model.Enabled = enabled;
            //更多功能
            this.dev_other_model_checkBox.Enabled = enabled;
            //比例模式
            this.dev_other_resize_checkBox.Enabled = enabled;
            //允许裁剪
            this.dev_other_resize_cut_checkBox.Enabled = enabled;
            //开启选择图片保存格式
            this.dev_save_pic_format_checkBox.Enabled = enabled;
            //开发更多模式中的其它功能
            this.dev_other_model_groupBox.Enabled = enabled;
            if (enabled)
            {
                this.dev_other_model_groupBox.Enabled = this.dev_other_model_checkBox.Checked;
            }
        }

        //退出保存开发者模式功能
        private void save_dev_model_datas()
        {
            //新测试功能
            Properties.Settings.Default.DevNewTestModel = this.dev_new_test_model.Checked;
            //更多功能
            Properties.Settings.Default.DevOtherModel = this.dev_other_model_checkBox.Checked;
            //改变尺寸的尺寸列表
            StringCollection resizeList = new StringCollection();
            foreach (string item in this.dev_other_resize_listBox.Items)
            {
                resizeList.Add(item);
            }
            Properties.Settings.Default.DevOtherResizeList = resizeList;
            //比例模式
            Properties.Settings.Default.DevOtherResizeRadioButton = this.dev_other_resize_checkBox.Checked;
            //允许裁剪
            Properties.Settings.Default.DevOtherResizeCutRadioButton = this.dev_other_resize_cut_checkBox.Checked;
            //开启选择图片保存格式
            Properties.Settings.Default.DevOtherSavePicFormatCheckBox = this.dev_save_pic_format_checkBox.Checked;
            //改变按比例尺寸的尺寸列表
            StringCollection scaleList = new StringCollection();
            foreach (string item in this.dev_other_scale_add_listBox.Items)
            {
                scaleList.Add(item);
            }
            Properties.Settings.Default.DevOtherScaleAddListBox = scaleList;
        }

        private void dev_other_model_Click(object sender, EventArgs e)
        {
            Console.WriteLine(@"开发者模式，其他功能。。。");

            switch (this.dev_other_tabControl.SelectedIndex)
            {
                case 0:
                    Console.WriteLine(@"开发者模式，其他功能。。。Tab index : 0");
                    // 改变尺寸功能模块点击事件
                    dev_other_resize_model_Click(sender, e);
                    break;

                case 1:
                    Console.WriteLine(@"开发者模式，其他功能。。。Tab index : 1");
                    if (this.dev_other_scale_sure_button.Enabled)
                    {
                        MessageBox.Show(@"二货，请先锁定当前分辨率！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (this.dev_other_scale_add_listBox.Items.Count <= 0)
                    {
                        MessageBox.Show(@"二货，请先添加输出的分辨率到列表中！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // 比例输出功能模块点击事件
                    dev_other_Scale_model_Click(sender, e);
                    break;

                case 2:
                    Console.WriteLine(@"开发者模式，其他功能。。。Tab index : 2");
                    MessageBox.Show(@"功能正在开发... 未实现！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;

                default:
                    Console.WriteLine(@"开发者模式，其他功能。。。Tab index : others");
                    MessageBox.Show(@"功能正在开发... 未实现！", @"给二货的提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    break;
            }
        }
    }
}
