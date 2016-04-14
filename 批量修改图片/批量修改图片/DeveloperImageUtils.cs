using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 批量修改图片
{
    public partial class ChangeImageUtils
    {
        public static readonly int MODE_UNKNOW = -1;
        public static readonly int MODE_WIDTH = 1;
        public static readonly int MODE_HEIGHT = 2;
        public static readonly int MODE_CUT_WIDTH = 11;
        public static readonly int MODE_CUT_HEIGHT = 21; 
        public static readonly int MODE_SCALE_WIDTH = 31; 
        public static readonly int MODE_SCALE_HEIGHT = 32;

        #region 按尺寸缩放

        public bool reSizeImage(string srcImageFilePath, string resultFilePath, int newW, int newH)
        {
            return reSizeImage(srcImageFilePath, resultFilePath, newW, newH, MODE_UNKNOW);
        }

        //按比例模式缩放
        public bool reSizeImage(string srcImageFilePath, string resultFilePath, double newW, double newH, int Mode)
        {
            if (srcImageFilePath == null || srcImageFilePath == string.Empty)
            {
                return false;
            }
            if (resultFilePath == null || resultFilePath == string.Empty)
            {
                return false;
            }

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

            Bitmap result;
            if (newH <= 0 || newW <= 0 || width <= 0 || height <= 0)
            {
                return false;
            }

            switch (Mode)
            {
                case -1: //unknow
                    result = new Bitmap((int)newW, (int)newH);
                    break;
                case 1: //width
                case 11://MODE_CUT_WIDTH
                    int h = (int)(newW * height / width);
                    if (h <= 0)
                    {
                        return false;
                    }
                    result = new Bitmap((int)newW, h);
                    break;

                case 21://MODE_CUT_HEIGHT
                case 2: //height
                    int w = (int)(newH * width / height);
                    if (w <= 0)
                    {
                        return false;
                    }
                    result = new Bitmap(w, (int)newH);
                    break;

                case 31://MODE_SCALE_WIDTH
                    result = new Bitmap((int)(width / newW), (int)(height / newW));
                    break;
                case 32://MODE_SCALE_HEIGHT
                    result = new Bitmap((int)(width / newH), (int)(height / newH));
                    break;
                default:
                    return false;
            }

            if (result == null)
            {
                return false;
            }

            try
            {
                Graphics g = Graphics.FromImage(result);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic; // 插值算法的质量   
                g.CompositingQuality = CompositingQuality.HighQuality; // 设置画布的描绘质量 
                g.SmoothingMode = SmoothingMode.AntiAlias;  //使绘图质量最高，即消除锯齿
                g.DrawImage(img, new Rectangle(0, 0, result.Width, result.Height), new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
                img.Dispose();
                img = null;
                g.Dispose();
                g = null;

                if(Mode == MODE_CUT_WIDTH || Mode == MODE_CUT_HEIGHT)//MODE_CUT_WIDTH || MODE_CUT_HEIGHT
                {
                    return Cut(result, resultFilePath, newW, newH, Mode);
                }

                if (this.mSavePICFormat == null)
                {
                    ImageFormat endstr = getImageFormat(resultFilePath.Substring(resultFilePath.LastIndexOf(".") + 1));
                    result.Save(resultFilePath, endstr); // 输出到文件流
                }
                else
                {
                    resultFilePath = resultFilePath.Substring(0, resultFilePath.LastIndexOf(".") + 1) + this.mSavePICFormatString;
                    result.Save(resultFilePath, this.mSavePICFormat); // 输出到文件流
                }
                result.Dispose();
                result = null;

                mCount++;
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion

        #region 允许裁剪

        /// <summary>  
        /// 剪裁 -- 用GDI+   
        /// </summary>  
        /// <param name="img">原始Bitmap</param>  
        /// <param name="resultFilePath">输出路径</param>  
        /// <param name="newW">新宽度</param>  
        /// <param name="newH">新高度</param>  
        /// <param name="Mode">模式</param>  
        private bool Cut(Image img, string resultFilePath, double newW, double newH, int Mode)
        {
            int width = img.Width; // 得到源图宽
            int height = img.Height; // 得到源图长

            int StartX = 0;//X截取起始位置
            int StartY = 0;//X截取起始位置
            double offset = 0;

            Bitmap result;
            if (newH <= 0 || newW <= 0 || width <= 0 || height <= 0)
            {
                return false;
            }
            switch (Mode)
            {
                case 11: //MODE_CUT_WIDTH
                    offset = height - newH;
                    StartY = (int)(offset / 2);
                    result = new Bitmap((int)newW, (int)newH);
                    break;

                case 21: //MODE_CUT_HEIGHT
                    offset = width - newW;
                    StartX = (int)(offset / 2);
                    result = new Bitmap((int)newW, (int)newH);
                    break;

                default:
                    return false;
            }

            if (result == null)
            {
                return false;
            }

            try
            {
                Graphics g = Graphics.FromImage(result);
                // 插值算法的质量   
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                // 设置画布的描绘质量         
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(img, new Rectangle(0, 0, result.Width, result.Height), new Rectangle(StartX, StartY, (width - StartX * 2), (height - StartY * 2)), GraphicsUnit.Pixel);
                img.Dispose();
                img = null;
                g.Dispose();
                g = null;

                if (this.mSavePICFormat == null)
                {
                    ImageFormat endstr = getImageFormat(resultFilePath.Substring(resultFilePath.LastIndexOf(".") + 1));
                    result.Save(resultFilePath, endstr); // 输出到文件流
                }
                else
                {
                    resultFilePath = resultFilePath.Substring(0, resultFilePath.LastIndexOf(".") + 1) + this.mSavePICFormatString;
                    result.Save(resultFilePath, this.mSavePICFormat); // 输出到文件流
                }
                result.Dispose();
                result = null;

                mCount++;
            }
            catch
            {
                return false;
            }

            return true;
        }

        #endregion

        //比例输出
        public bool scaleImage(string srcImageFilePath, string resultFilePath, int oldW, int oldH, int newW, int newH, int mode)
        {
            double scaleW = (double)oldW / (double)newW;//宽比例
            double scaleH = (double)oldH / (double)newH;//高比例
            return reSizeImage(srcImageFilePath, resultFilePath, scaleW, scaleH, mode);
        }
    }
}
