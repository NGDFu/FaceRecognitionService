using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceRecognition.Services
{
    public class ServiceOption
    {
        /// <summary>
        /// 匹配时取最相似的top k个人脸特征
        /// </summary>
        public int TopK { get; set; }

        /// <summary>
        /// 当前版本，判断是否是同一个人的阈值
        /// </summary>
        public float Threshold { get; set; } = 0.3f;

        /// <summary>
        /// 人脸特征长度，dlib是128维数组
        /// </summary>
        public int FeatureLength { get; set; }

        /// <summary>
        /// 人脸识别模型目录
        /// </summary>
        public string ModelPath { get; set; }

        /// <summary>
        /// 保存人脸信息的路径(LevelDB使用本地存储)
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// 人脸特征存储文件路径
        /// </summary>
        public string FeaturePath { get; set; }

        /// <summary>
        /// 如果是传图片过来，那么图片保存路径
        /// </summary>
        public string ImageSavePath { get; set; }

        public string PublicIP { get; set; }
    }
}
