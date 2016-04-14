using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 批量修改图片
{
    class ScaleBeans
    {
        private String mFileName;
        private int mWidth;
        private int mHeigth;
        public String getmFileName()
        {
            return mFileName;
        }
        public void setmFileName(String mFileName)
        {
            this.mFileName = mFileName;
        }
        public int getmWidth()
        {
            return mWidth;
        }
        public void setmWidth(int mWidth)
        {
            this.mWidth = mWidth;
        }
        public int getmHeigth()
        {
            return mHeigth;
        }
        public void setmHeigth(int mHeigth)
        {
            this.mHeigth = mHeigth;
        }
    }
}
