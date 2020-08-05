using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaceRecognition.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Common.HttpUtils;

namespace FaceRecognition.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceRecognitionController : ControllerBase
    {
        private readonly FaceRecognitionService m_faceRecognitionService = null;

        private readonly int BaseErrorCode = 60010000;

        public FaceRecognitionController(FaceRecognitionService faceRecognitionService)
        {
            m_faceRecognitionService = faceRecognitionService;
        }

        /// <summary>
        /// 人脸图片入库
        /// </summary>
        /// <param name="file">图片二进制内容</param>
        /// <returns>人脸特征ID</returns>
        [HttpPost("add")]
        public async Task<IActionResult> Add(IFormFile file)
        {
            var result = await m_faceRecognitionService.AddAsync(file);
            return HandleError(result);
        }

        /// <summary>
        /// 人脸图片入库，前端取人脸特征
        /// </summary>
        /// <param name="feature">人脸特征</param>
        /// <returns>人脸特征ID</returns>
        [HttpPost("addByFeature")]
        public async Task<IActionResult> Add([FromForm]float[] feature)
        {
            var result = await m_faceRecognitionService.AddIndexAsync(feature);
            return HandleError(result);
        }

        /// <summary>
        /// 人脸匹配
        /// </summary>
        /// <param name="file">图片二进制内容</param>
        /// <returns>匹配上的人脸特征ID，对应的测试图片下载链接</returns>
        [HttpPost("match")]
        public async Task<IActionResult> MatchAsync(IFormFile file)
        {
            var result = await m_faceRecognitionService.MatchAsync(file);
            return HandleError(result);
        }

        /// <summary>
        /// 人脸匹配
        /// </summary>
        /// <param name="feature">人脸特征</param>
        /// <returns>匹配上的人脸特征ID，对应的测试图片下载链接</returns>
        [HttpPost("matchByFeature")]
        public async Task<IActionResult> MatchAsync([FromForm]float[] feature)
        {
            var result = await m_faceRecognitionService.MatchAsync(feature);
            return HandleError(result);
        }

        /// <summary>
        /// 测试使用，仅用于传图片情景。下载图片
        /// </summary>
        /// <param name="fileName">图片名</param>
        /// <returns></returns>
        [HttpGet("getImageFile/{fileName}")]
        public async Task<IActionResult> GetFileAsync(string fileName)
        {
            var data = await m_faceRecognitionService.GetImageFileAsync(fileName);

            return File(data, "image/jpeg", fileName);
        }

        private IActionResult HandleError(JsonResponse response)
        {
            if (response.Code != 0)
            {
                response.Code = BaseErrorCode + response.Code;
            }
            return new JsonResult(response);
        }
    }
}
