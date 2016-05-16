using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 批量修改图片
{
    public partial class ChangeImageUtils
    {
        //成功数量
        private int mCount = 0;
        //失败数量
        private int mErrorCount = 0;
        //过滤数量
        private int mFilterCount = 0;
        //失败文件路径
        private int mErrorPathCount = 0;
        //图片保存格式
        private ImageFormat mSavePICFormat = null;
        //图片扩展名
        private string mSavePICFormatString = "png";

        /**
         * @function changeImageColor
         * @Description TODO 改变图片颜色
         * @author zhangyao
         * @date 2015年11月19日 下午2:14:02
         * @param srcImageFilePath
         * @param resultFilePath
         * @param oldColor
         * @param newColor
         * @param mSimilarity 相似度
         * @param isSingleValue 饱和度和明度不发生改变
         */
        public bool changeImageColor(String srcImageFilePath, String resultFilePath, Color oldColor, Color newColor, double mSimilarity, bool isSingleValue)
        {
            bool flag = false;
            //        System.out.println("mSimilarity:" + mSimilarity);

            Image img;
            try
            {
                img = Image.FromFile(srcImageFilePath);//读入图片文件
            }
            catch (OutOfMemoryException)
            {
                return true;
            }
            int width = img.Width; // 得到源图宽
            int height = img.Height; // 得到源图长
            Bitmap src = new Bitmap(img);
            img.Dispose();
            img = null;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color currColor = src.GetPixel(i, j);
                    //int r = color.ToArgb() & 0xFFFFFF;
                    int a = currColor.A;

                    if (isEqual(currColor, oldColor, mSimilarity))
                    {
                        //int[] rgb = Hex2RGB(oldColor.ToArgb());
                        //double[] hsl = RGB2HSL(rgb[0], rgb[1], rgb[2]);
                        //int[] rgb1 = Hex2RGB(currColor.ToArgb());
                        //double[] hsl1 = RGB2HSL(rgb1[0], rgb1[1], rgb1[2]);
                        //int[] rgb2 = Hex2RGB(newColor.ToArgb());
                        //double[] hsl2 = RGB2HSL(rgb2[0], rgb2[1], rgb2[2]);

                        float currH = currColor.GetHue();
                        float oldH = oldColor.GetHue();
                        float newH = newColor.GetHue();

                        float currS = currColor.GetSaturation();
                        float oldS = oldColor.GetSaturation();
                        float newS = newColor.GetSaturation();

                        float currB = currColor.GetBrightness();
                        float oldB = oldColor.GetBrightness();
                        float newB = newColor.GetBrightness();

                        float tempH = Math.Min((Math.Max(currH, oldH) - Math.Min(currH, oldH)), (Math.Min(currH, oldH) + 360 - Math.Max(currH, oldH)));//色度色差 (0~360)
                        float tempS = Math.Min((Math.Max(currS, oldS) - Math.Min(currS, oldS)), (Math.Min(currS, oldS) + 1 - Math.Max(currS, oldS)));//饱和度色差 (0~1.0)
                        float tempB = Math.Min((Math.Max(currB, oldB) - Math.Min(currB, oldB)), (Math.Min(currB, oldB) + 1 - Math.Max(currB, oldB)));//明度色差 (0~1.0)

                        //double t = hsl[0] - hsl1[0];//色度色差
                        //double temp = hsl[1] - hsl1[1];//饱和度色差
                        //double temp1 = hsl[2] - hsl1[2];//明度色差
                        //                        System.out.println(hsl2[1] + "::" + hsl2[2]);
                        //                        System.out.println(temp + "::" + temp1);

                        float h = calculateHSB(oldH, currH, newH, tempH);
                        float s = calculateHSB(oldS, currS, newS, tempS);
                        float l = calculateHSB(oldB, currB, newB, tempB);

                        //float h = newH + tempH;
                        //float s = newS + tempS;
                        //float l = newB + tempB;
                        if (isSingleValue)
                        {
                            s = currS;
                            l = currB;
                        }

                        if (h < 0)
                        {
                            h += 360;
                        }
                        else if (h >= 360)
                        {
                            h -= 360;
                        }
                        if (s < 0)
                        {
                            //s += 1;
                            s = 0;
                        }
                        else if (s > 1)
                        {
                            //s -= 1;
                            s = 1;
                        }
                        if (l < 0)
                        {
                            //l += 1;
                            l = 0;
                        }
                        else if (l > 1)
                        {
                            //l -= 1;
                            l = 1;
                        }

                        int[] newRGB = HSL2RGB(h, s, l);

                        //                        int hex = RGB2Hex(newRGB[0], newRGB[1], newRGB[2]);
                        //int hex = RGB2Hex(newRGB[0], newRGB[1], newRGB[2]);
                        //                        System.out.println(Integer.toHexString(hex));

                        Color tempColor = Color.FromArgb(a, newRGB[0], newRGB[1], newRGB[2]);
                        src.SetPixel(i, j, tempColor);

                        //if (!mExcludeFilesNameList.contains(srcImageFilePath))
                        //{
                        //    mExcludeFilesNameList.add(srcImageFilePath);
                        //    //                            System.out.println(srcImageFilePath.substring(srcImageFilePath.lastIndexOf("\\") + 1));
                        //}

                        flag = true;
                    }
                }
            }
            if(this.mSavePICFormat == null)
            {
                ImageFormat endstr = getImageFormat(resultFilePath.Substring(resultFilePath.LastIndexOf(".") + 1));
                src.Save(resultFilePath, endstr); // 输出到文件流
            }
            else
            {
                resultFilePath = resultFilePath.Substring(0, resultFilePath.LastIndexOf(@".") + 1) + this.mSavePICFormatString;
                src.Save(resultFilePath, this.mSavePICFormat); // 输出到文件流
            }
            src.Dispose();
            src = null;
            mCount++;
            //image image = src.getscaledinstance(width, height, image.scale_default);
            //bufferedimage tag = new bufferedimage(width, height, bufferedimage.translucent);
            //graphics g = tag.getgraphics();
            //g.drawimage(image, 0, 0, null); // 绘制缩小后的图
            //g.dispose();
            //file file = new file(resultfilepath);
            //if (!file.exists())
            //{
            //    file.mkdirs();
            //}
            //imageio.write(tag, "png", new file(resultfilepath));// 输出到文件流

            return flag;
        }

  
        /**
        * 复制图片
        */
        public void copyImage(String srcImageFilePath, String resultFilePath)
        {
            Image img;
            try
            {
                img = Image.FromFile(srcImageFilePath);//读入图片文件
            }
            catch (OutOfMemoryException)
            {
                return;
            }
            int width = img.Width; // 得到源图宽
            int height = img.Height; // 得到源图长
            Bitmap src = new Bitmap(img);
            img.Dispose();
            img = null;

            if (this.mSavePICFormat == null)
            {
                ImageFormat endstr = getImageFormat(resultFilePath.Substring(resultFilePath.LastIndexOf(".") + 1));
                src.Save(resultFilePath, endstr); // 输出到文件流
            }
            else
            {
                resultFilePath = resultFilePath.Substring(0, resultFilePath.LastIndexOf(".") + 1) + this.mSavePICFormatString;
                src.Save(resultFilePath, this.mSavePICFormat); // 输出到文件流
            }
            src.Dispose();
            src = null;
        }

        /**
        * 计算新色值
        */
        private float calculateHSB(float oldH, float currH, float newH, float tempH)
        {
            float tempH01;// 大的减小的
            if (currH > oldH)
            {
                tempH01 = currH - oldH;
            }
            else
            {
                tempH01 = oldH - currH;
            }

            float tempH02;// 小的减大的
            if (currH > oldH)
            {
                tempH02 = oldH - currH + 360;
            }
            else
            {
                tempH02 = currH - oldH + 360;
            }

            float h;
            if (tempH01 == tempH)
            {
                if (currH > oldH)
                {
                    h = newH + tempH;
                }
                else
                {
                    h = newH - tempH;
                }
            }
            else if (tempH02 == tempH)
            {
                if (currH > oldH)
                {
                    h = newH - tempH;
                }
                else
                {
                    h = newH + tempH;
                }
            }
            else
            {
                h = newH;
            }
            return h;
        }

        /**
        * 统计总数
        */
        public int getCount()
        {
            return this.mCount;
        }

        public void clearCount()
        {
            this.mCount = 0;
        }

        /**
        * 统计错误总数
        **/
        public void setErrorCount()
        {
            this.mErrorCount++;
        }

        public int getErrorCount()
        {
            return this.mErrorCount;
        }

        public void clearErrorCount()
        {
            this.mErrorCount = 0;
        }

        /**
        * 统计错误路径总数
        **/
        public void setErrorPathCount()
        {
            this.mErrorPathCount++;
        }

        public int getErrorPathCount()
        {
            return this.mErrorPathCount;
        }

        public void clearErrorPathCount()
        {
            this.mErrorPathCount = 0;
        }

        /**
        * 过滤总数
        **/
        public void setFilterCount()
        {
            this.mFilterCount++;
        }

        public int getFilterCount()
        {
            return this.mFilterCount;
        }

        public void clearFilterCount()
        {
            this.mFilterCount = 0;
        }


        public void setImageFormat(string format)
        {
            switch (format.ToUpper())
            {
                case "PNG":
                    this.mSavePICFormat = ImageFormat.Png;
                    this.mSavePICFormatString = "png";
                    break;
                case "BMP":
                    this.mSavePICFormat = ImageFormat.Bmp;
                    this.mSavePICFormatString = "bmp";
                    break;
                case "JPG":
                case "JPEG":
                    this.mSavePICFormat = ImageFormat.Jpeg;
                    this.mSavePICFormatString = "jpg";
                    break;
                case "GIF":
                    this.mSavePICFormat = ImageFormat.Gif;
                    this.mSavePICFormatString = "gif";
                    break;
                case "EMF":
                    this.mSavePICFormat = ImageFormat.Emf;
                    this.mSavePICFormatString = "emf";
                    break;
                case "ICON":
                    this.mSavePICFormat = ImageFormat.Icon;
                    this.mSavePICFormatString = "ico";
                    break;
                case "TIFF":
                    this.mSavePICFormat = ImageFormat.Tiff;
                    this.mSavePICFormatString = "tiff";
                    break;
                case "EXIF":
                    this.mSavePICFormat = ImageFormat.Exif;
                    this.mSavePICFormatString = "jpeg";
                    break;
                case "WMF":
                    this.mSavePICFormat = ImageFormat.Wmf;
                    this.mSavePICFormatString = "wmf";
                    break;
                default:
                    this.mSavePICFormat = null;
                    this.mSavePICFormatString = "";
                    break;
            }
            Console.WriteLine(@"dev_save_pic_format_comboBox.SelectedText: " + this.mSavePICFormat + " : " + this.mSavePICFormatString);
        }

        public ImageFormat getImageFormat(string format)
        {
            switch (format.ToUpper())
            {
                case "PNG":
                default:
                    return ImageFormat.Png;
                case "BMP":
                    return ImageFormat.Bmp;
                case "JPG":
                case "JPEG":
                    return ImageFormat.Jpeg;
                case "GIF":
                    return ImageFormat.Gif;
                case "EMF":
                    return ImageFormat.Emf;
                case "ICO":
                    return ImageFormat.Icon;
                case "TIFF":
                    return ImageFormat.Tiff;
                case "EXIF":
                    return ImageFormat.Exif;
                case "WMF":
                    return ImageFormat.Wmf;
            }
        }


        /**
     * @function Hex2RGB
     * @Description TODO 十六进制转RGB
     * @author zhangyao
     * @date 2015年11月19日 上午9:46:47
     * @param color
     * @return
     */
        public int[] Hex2RGB(int color)
        {
            int b = 0xFF & color;
            int g = 0xFF00 & color;
            g >>= 8;
            int r = 0xFF0000 & color;
            r >>= 16;
            return new int[] { r, g, b };
        }

        /**
     * @function RGBToHex
     * @Description TODO RGB转十六进制
     * @author zhangyao
     * @date 2015年11月19日 下午12:34:17
     * @param r
     * @param g
     * @param b
     * @return
     */
        public int RGB2Hex(int r, int g, int b)
        {
            r <<= 16;
            g <<= 8;
            return r + g + b;
        }

        /**
     * @function HSL2RGB
     * @Description HSL转换为RGB
     * @author zhangyao
     * @date 2015年11月19日 上午11:30:33
     * @param H
     * @param S
     * @param L
     * @return
     */
        public int[] HSL2RGB(double H, double S, double L)
        {
            double R, G, B;
            double temp1, temp2;
            if (S == 0)
            {
                R = G = B = L * 255;
            }
            else
            {
                if (L < 0.5)
                {
                    temp2 = L * (1.0 + S);
                }
                else
                {
                    temp2 = L + S - L * S;
                }

                temp1 = 2.0 * L - temp2;

                H /= 360;

                R = 255.0 * Hue2RGB(temp1, temp2, H + (1.0 / 3.0));
                G = 255.0 * Hue2RGB(temp1, temp2, H);
                B = 255.0 * Hue2RGB(temp1, temp2, H - (1.0 / 3.0));
            }

            //        BigDecimal b = new BigDecimal(R);
            //        int R1 = b.setScale(2, BigDecimal.ROUND_HALF_UP).intValue();
            //        b = new BigDecimal(G);
            //        int G1 = b.setScale(2, BigDecimal.ROUND_HALF_UP).intValue();
            //        b = new BigDecimal(B);
            //        int B1 = b.setScale(2, BigDecimal.ROUND_HALF_UP).intValue();

            //        return new int[] {R1, G1, B1};
            //        System.out.println(R + ":::" + (int)Math.round(R));
            //        System.out.println(G + ":::" + (int)Math.round(G));
            //        System.out.println(B + ":::" + (int)Math.round(B));
            return new int[] { (int)Math.Round(R), (int)Math.Round(G), (int)Math.Round(B) };
        }

        /**
         * @function Hue2RGB
         * @Description Hue转换为RGB
         * @author zhangyao
         * @date 2015年11月19日 下午1:19:53
         * @param v1
         * @param v2
         * @param vH
         * @return
         */
        private double Hue2RGB(double v1, double v2, double vH)
        {
            if (vH < 0)
                vH += 1.0;
            if (vH > 1)
                vH -= 1.0;
            if (6.0 * vH < 1)
                return v1 + (v2 - v1) * 6.0 * vH;
            if (2.0 * vH < 1)
                return v2;
            if (3.0 * vH < 2)
                return v1 + (v2 - v1) * ((2.0 / 3.0) - vH) * 6.0;
            return (v1);
        }


        /**
     * @function RGBtoHSV
     * @Description TODO RGB转换HSV
     * @author zhangyao
     * @date 2015年11月18日 下午9:39:19
     * @param r
     * @param g
     * @param b
     * @return
     */
        public double[] RGB2HSV(int r, int g, int b)
        {

            double h, s, v, l;

            double min, max, delta;

            min = Math.Min(Math.Min(r, g), b);
            max = Math.Max(Math.Max(r, g), b);

            // V
            v = max;
            delta = max - min;
            l = (max + min) / 2 / 255;

            if (min == 255)
            {
                v = 255;
                s = 0;
                h = 0;
                return new double[] { h, s, v };
            }
            // S
            if (max != 0)
            {
                if (l < 0.5)
                {
                    s = (max - min) / (max + min);
                }
                else
                {
                    s = (max - min) / (2.0 * 255 - max - min);
                }
            }
            else
            {
                v = 0;
                s = 0;
                h = 0;
                return new double[] { h, s, v };
            }

            // H
            if (r == max)
            {
                h = (g - b) / delta; // between yellow & magenta
            }
            else if (g == max)
            {
                h = 2 + (b - r) / delta; // between cyan & yellow
            }
            else
            {
                h = 4 + (r - g) / delta; // between magenta & cyan
            }

            h *= 60;    // degrees

            if (h < 0)
                h += 360;

            //        System.out.println(h);
            //        System.out.println(s);
            //        System.out.println(v);

            return new double[] { h, s, v };
        }

        /**
         * @function RGBtoHSL
         * @Description TODO RGB转换HSL
         * @author zhangyao
         * @date 2015年11月19日 上午9:50:21
         * @param r
         * @param g
         * @param b
         * @return
         */
        public double[] RGB2HSL(int r1, int g1, int b1)
        {

            double h, s, v, l, r, g, b;

            double min, max, delta;

            r = r1 / 255.0;
            g = g1 / 255.0;
            b = b1 / 255.0;

            min = Math.Min(Math.Min(r, g), b);
            max = Math.Max(Math.Max(r, g), b);

            // V
            v = max;
            delta = max - min;
            l = (max + min) / 2;

            if (min == max)
            {
                s = 0;
                h = 0;
                return new double[] { h, s, l };
            }
            // S
            if (max != 0)
            {
                if (l <= 0.5)
                {
                    s = (max - min) / (max + min);
                }
                else if (l > 0.5)
                {
                    s = (max - min) / (2.0 - max - min);
                }
                else
                {
                    s = 0;
                }
            }
            else
            {
                l = 0;
                s = 0;
                h = 0;
                return new double[] { h, s, l };
            }

            // H
            if (r == max)
            {
                h = (g - b) / delta; // between yellow & magenta
            }
            else if (g == max)
            {
                h = 2 + (b - r) / delta; // between cyan & yellow
            }
            else
            {
                h = 4 + (r - g) / delta; // between magenta & cyan
            }

            h *= 60;    // degrees

            //        h = Math.round(h);

            if (h < 0)
                h += 360;

            //        System.out.println(h);
            //        System.out.println(s);
            //        System.out.println(l);

            return new double[] { h, s, l };
        }

        /**
         * @function isEqual
         * @Description TODO
         * @author zhangyao
         * @date 2015年11月19日 下午2:13:30
         * @param currColor 当前颜色
         * @param oldColor 旧颜色
         * @param mSimilarity 相似度
         * @return
         */
        public bool isEqual(Color currColor, Color oldColor, double mSimilarity)
        {
            if (currColor.A == 0)
            {
                return false;
            }

            if (currColor.ToArgb() == oldColor.ToArgb())
            {
                return true;
            }

            float currH = currColor.GetHue();
            float oldH = oldColor.GetHue();
            float currS = currColor.GetSaturation();
            float oldS = oldColor.GetSaturation();
            float currB = currColor.GetBrightness();
            float oldB = oldColor.GetBrightness();

            //double[] currHSL = RGB2HSL(currColor.R, currColor.G, currColor.B);
            //double[] oldHSL = RGB2HSL(oldColor.R, oldColor.G, oldColor.B);

            //Console.WriteLine(currColor.GetHue() + " - " + currColor.GetSaturation() + " - " + currColor.GetBrightness());
            //Console.WriteLine(oldColor.GetHue() + " - " + oldColor.GetSaturation() + " - " + oldColor.GetBrightness());

            double tempH = Math.Min((Math.Max(currH, oldH) - Math.Min(currH, oldH)), (Math.Min(currH, oldH) + 360 - Math.Max(currH, oldH)));
            //        if (temp > Math.min(hsl[0], hsl2[0]))
            //        {//360与0度等价转换
            //            temp = 360 - temp;
            //            System.out.println("1::" + temp);
            //        }
            //        else
            //        {
            //            temp = Math.abs(hsl[0] - hsl2[0]);
            //            System.out.println("2::" + temp);
            //        }
            if (tempH <= mSimilarity * 10 && tempH >= 0)
            {
                double tempS = Math.Min((Math.Max(currS, oldS) - Math.Min(currS, oldS)), (Math.Min(currS, oldS) + 1.0f - Math.Max(currS, oldS)));
                if (tempS <= mSimilarity && tempS >= 0)
                {
                    double tempB = Math.Min((Math.Max(currB, oldB) - Math.Min(currB, oldB)), (Math.Min(currB, oldB) + 1.0f - Math.Max(currB, oldB)));
                    if (tempB <= mSimilarity && tempB >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


    }


}
