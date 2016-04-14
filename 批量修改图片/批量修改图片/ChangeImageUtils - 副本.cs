using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 批量修改图片
{
    class ChangeImageUtils
    {
        private int mCount = 0;

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
        public void changeImageColor(String srcImageFilePath, String resultFilePath, Color oldColor, Color newColor, double mSimilarity, bool isSingleValue)
        {

            mSimilarity *= 10;//相似度
            //        System.out.println("mSimilarity:" + mSimilarity);

            var img = Image.FromFile(srcImageFilePath);  // 读入文件
            int width = img.Width; // 得到源图宽
            int height = img.Height; // 得到源图长
            Bitmap src = new Bitmap(img);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Color currColor = src.GetPixel(i, j);
                    //int r = color.ToArgb() & 0xFFFFFF;
                    int a = currColor.A;

                    if (isEqual(currColor, oldColor, mSimilarity))
                    {
                        int[] rgb = Hex2RGB(oldColor.ToArgb());
                        double[] hsl = RGB2HSL(rgb[0], rgb[1], rgb[2]);
                        int[] rgb1 = Hex2RGB(currColor.ToArgb());
                        double[] hsl1 = RGB2HSL(rgb1[0], rgb1[1], rgb1[2]);
                        int[] rgb2 = Hex2RGB(newColor.ToArgb());
                        double[] hsl2 = RGB2HSL(rgb2[0], rgb2[1], rgb2[2]);

                        double t = hsl[0] - hsl1[0];//色度色差
                        double temp = hsl[1] - hsl1[1];//饱和度色差
                        double temp1 = hsl[2] - hsl1[2];//明度色差
                        //                        System.out.println(hsl2[1] + "::" + hsl2[2]);
                        //                        System.out.println(temp + "::" + temp1);

                        double h = hsl2[0] - t;
                        double s = hsl2[1] - temp;
                        double l = hsl2[2] - temp1;
                        if (isSingleValue)
                        {
                            s = hsl1[1];
                            l = hsl1[2];
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
                            s += 1;
                        }
                        else if (s > 1)
                        {
                            s -= 1;
                        }
                        if (l < 0)
                        {
                            l += 1;
                        }
                        else if (l > 1)
                        {
                            l -= 1;
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
                    }
                }
            }

            src.Save(resultFilePath, System.Drawing.Imaging.ImageFormat.Png); // 输出到文件流
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
        }

        /**
        * 统计总数
        */
        public int getCount()
        {
            return mCount;
        }

        public void clearCount()
        {
            mCount = 0;
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
            int[] rgb = Hex2RGB(currColor);
            double[] hsl = RGB2HSL(rgb[0], rgb[1], rgb[2]);
            int[] rgb2 = Hex2RGB(oldColor);
            double[] hsl2 = RGB2HSL(rgb2[0], rgb2[1], rgb2[2]);
            //        System.out.println(hsl[0] + "===" + hsl2[0]);

            double temp =
                Math.Min((Math.Max(hsl[0], hsl2[0]) - Math.Min(hsl[0], hsl2[0])), (Math.Min(hsl[0], hsl2[0]) + 1.0 - Math.Max(hsl[0], hsl2[0])));
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
            if (temp <= mSimilarity)
            {
                //            System.out.println(Math.abs(hsl[0] - hsl2[0]));
                if (hsl[0] == 0 && hsl[1] == 0)
                {
                    if (color == color2)
                    {
                        return true;
                    }
                    else
                    {
                        //                    System.out.println(color + "::" + color2);
                        temp =
                            Math.Min((Math.Max(hsl[2], hsl2[2]) - Math.Min(hsl[2], hsl2[2])),
                                (Math.Min(hsl[2], hsl2[2]) + 1.0 - Math.Max(hsl[2], hsl2[2])));
                        if (temp < mSimilarity * 0.1)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                //            System.out.println(color + "===" + color2);
                return true;
            }
            return false;
        }

    }

    
}
