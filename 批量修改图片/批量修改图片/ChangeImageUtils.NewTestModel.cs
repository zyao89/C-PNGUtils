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
    partial class ChangeImageUtils
    {

        /**
          * 开发者新算法 - 测试
          **/
        public bool changeImageColor(String srcImageFilePath, String resultFilePath, ArrayList oldColorList, Color newColor, double mSimilarity, bool isSingleValue)
        {
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

                    foreach (Color oldColor in oldColorList)
                    {
                        if (isEqualDev(currColor, oldColor, mSimilarity))
                        {
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

                            float h = calculateHSB(oldH, currH, newH, tempH);
                            float s = calculateHSB(oldS, currS, newS, tempS);
                            float l = calculateHSB(oldB, currB, newB, tempB);

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

                            int[] rgb = HSL2RGB(h, s, l);

                            Color tempColor = Color.FromArgb(a, rgb[0], rgb[1], rgb[2]);
                            src.SetPixel(i, j, tempColor);
                        }
                    }
                }
            }

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
            mCount++;

            return true;
        }

        //开发者模式引用算法
        public bool isEqualDev(Color currColor, Color oldColor, double mSimilarity)
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
                double tempS = Math.Max(currS, oldS) - Math.Min(currS, oldS);
                if (tempS <= mSimilarity && tempS >= 0)
                {
                    double tempB = Math.Max(currB, oldB) - Math.Min(currB, oldB);
                    if (tempB <= mSimilarity && tempB >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //增加新颜色集合的算法（临时增加）
        public bool changeImageColor(String srcImageFilePath, String resultFilePath, ArrayList oldColorList, ArrayList newColorList, double mSimilarity, bool isSingleValue)
        {
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

                    ArrayList tempOldColorList = oldColorList;
                    int index = 2;

                    if (oldColorList.Count >= 2 && newColorList.Count >= 2)
                    {
                        tempOldColorList = oldColorList.GetRange(0, oldColorList.Count / 2);
                        index = 0;
                    }
                    foreach (Color newColor in newColorList)
                    {
                        index++;
                        foreach (Color oldColor in tempOldColorList)
                        {
                            if (isEqualDev(currColor, oldColor, mSimilarity))
                            {
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

                                float h = calculateHSB(oldH, currH, newH, tempH);
                                float s = calculateHSB(oldS, currS, newS, tempS);
                                float l = calculateHSB(oldB, currB, newB, tempB);

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

                                int[] rgb = HSL2RGB(h, s, l);

                                Color tempColor = Color.FromArgb(a, rgb[0], rgb[1], rgb[2]);
                                src.SetPixel(i, j, tempColor);
                            }
                        }

                        if (index < 2)
                        {
                            tempOldColorList = oldColorList.GetRange(oldColorList.Count / 2, oldColorList.Count / 2);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

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
            mCount++;

            return true;
        }
    }

}
