using System.Collections.Generic;
using System.IO;
using System.Linq;
using FaceRecognitionDotNet;
using System.Drawing;
using System;

namespace FaceRecognitionLib
{
    /// <summary>
    /// 检测图像中的头像特征
    /// </summary>
    public class FaceRecogniser
    {
        public string ModelPath { get; private set; }

        private FaceRecognitionDotNet.FaceRecognition m_faceRecognition;

        /// <summary>
        /// 创建图像中头像特征提取器
        /// </summary>
        /// <param name="modelPath">深度学习路径</param>
        /// <returns></returns>
        public static FaceRecogniser Build(string modelPath)
        {
            return new FaceRecogniser(modelPath);
        }

        private FaceRecogniser(string modelPath)
        {
            if (string.IsNullOrWhiteSpace(modelPath) || !Directory.Exists(modelPath))
            {
                throw new DirectoryNotFoundException($"Model path invalid: {modelPath}");
            }

            m_faceRecognition = FaceRecognitionDotNet.FaceRecognition.Create(modelPath);
        }

        /// <summary>
        /// 取头像特征
        /// </summary>
        /// <param name="imagePath">图片路径</param>
        /// <returns></returns>
        public IList<double[]> GetFaceEncodings(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                throw new System.ArgumentException($"Argument invalid or file not exists");
            }

            using (var image = FaceRecognitionDotNet.FaceRecognition.LoadImageFile(imagePath))
            {
                var encodings = m_faceRecognition.FaceEncodings(image);
                if (encodings.Count() <= 0)
                {
                    return null;
                }
                return encodings.Select(item => item.GetRawEncoding()).ToList();
            }
        }

        /// <summary>
        /// 取头像特征
        /// </summary>
        /// <param name="imageData">图像的二进制数据</param>
        /// <returns></returns>
        public IList<double[]> GetFaceEncodings(byte[] imageData)
        {
            if (imageData == null || imageData.Length <= 0)
            {
                return null;
            }
            using (var stream = new MemoryStream(imageData))
            {
                using(var image = (Bitmap)System.Drawing.Image.FromStream(stream))
                {
                    using(var targetImage = FaceRecognitionDotNet.FaceRecognition.LoadImage(image))
                    {
                        var encoding = m_faceRecognition.FaceEncodings(targetImage);
                        if (encoding.Count() == 0)
                        {
                            return null;
                        }
                        return encoding.Select(item => item.GetRawEncoding()).ToList();
                    }
                }
            }
        }

        /// <summary>
        /// 检测图像中是否有人脸
        /// </summary>
        /// <param name="imagePath">图像路径</param>
        /// <returns></returns>
        public bool HasFace(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
            {
                throw new System.ArgumentException($"Argument invalid or file not exists");
            }

            using (var image = FaceRecognitionDotNet.FaceRecognition.LoadImageFile(imagePath))
            {
                return HasFace(image);
            }
        }

        /// <summary>
        /// 检测图像中是否有人脸
        /// </summary>
        /// <param name="imageData">图像二进制数据</param>
        /// <returns></returns>
        public bool HasFace(byte[] imageData)
        {
            if (imageData == null || imageData.Length <= 0)
            {
                throw new ArgumentException("Argument invalid");
            }
            using(var stream = new MemoryStream(imageData))
            {
                using(var bitmap = (Bitmap)System.Drawing.Image.FromStream(stream))
                {
                    using(var image = FaceRecognitionDotNet.FaceRecognition.LoadImage(bitmap))
                    {
                        return HasFace(image);
                    }
                }
            }
        }

        public bool HasFaces(byte[] imageData)
        {
            using (var stream = new MemoryStream(imageData))
            {
                using (var bitmap = (Bitmap)System.Drawing.Image.FromStream(stream))
                {
                    using (var image = FaceRecognitionDotNet.FaceRecognition.LoadImage(bitmap))
                    {
                        var locations = m_faceRecognition.FaceLocations(image);
                        return locations.Count() > 1;
                    }
                }
            }
        }

        private bool HasFace(FaceRecognitionDotNet.Image image)
        {
            var locations = m_faceRecognition.FaceLocations(image);
            return locations.Count() != 0;
        }
    }
}
