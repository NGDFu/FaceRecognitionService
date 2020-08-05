using System;
using System.Collections.Generic;
using System.Text;

namespace FaceLabel
{
    public class FaceInfo
    {
        /// <summary>
        /// 人脸特征的索引
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 人脸特征的ID，外界使用此ID将个人信息和人脸特征对应
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 如果是从图片取人脸特征，那么该图片保存路径，可以为空
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// 人脸特征
        /// </summary>
        public float[] Feature { get; set; }
    }
}
