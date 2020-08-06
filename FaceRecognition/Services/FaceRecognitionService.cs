using FaceIndex;
using FaceLabel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaceRecognitionLib;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.IO;
using FaceIndex.MemIndexLib;
using FaceLabel.LevelFaceInfoLib.LevelFaceInfo;
using Common.HttpUtils;
using Microsoft.AspNetCore.Http;
using Common.IOUtils;

namespace FaceRecognition.Services
{
    public class FaceRecognitionService
    {
        public ServiceOption ServiceOption { get; private set; }

        private IIndex m_index = null;
        private IFaceInfoDB m_faceInfo = null;
        private FaceRecogniser m_faceRecogniser = null;

        private ILogger<FaceRecognitionService> m_logger = null;

        public FaceRecognitionService(IOptions<ServiceOption> options, ILogger<FaceRecognitionService> logger)
        {
            ServiceOption = options.Value;
            m_logger = logger;

            CheckEnv();
            Init();
        }

        public void Start()
        {

        }

        private void Init()
        {
            m_faceRecogniser = FaceRecogniser.Build(ServiceOption.ModelPath);
            m_logger.LogInformation($"Init face recogniser success");

            m_index = MemIndex.Create(ServiceOption.FeaturePath);
            m_logger.LogInformation($"Init index success");

            m_faceInfo = LevelFaceInfo.Create(ServiceOption.DBName);
            m_logger.LogInformation($"Init faceInfo database success");
        }

        private void CheckEnv()
        {
            if (ServiceOption == null)
            {
                throw new ArgumentNullException("ServiceOption invalid");
            }

            if (string.IsNullOrWhiteSpace(ServiceOption.ModelPath))
            {
                ServiceOption.ModelPath = "./Data/Models";
            }
            if (!Directory.Exists(ServiceOption.ModelPath))
            {
                throw new DirectoryNotFoundException("Face recognition model path invalid");
            }
            m_logger.LogInformation($"Face recognition model path: {ServiceOption.ModelPath}");

            if (string.IsNullOrWhiteSpace(ServiceOption.DBName))
            {
                ServiceOption.DBName = "./Data/Database";
            }
            if (!Directory.Exists(ServiceOption.DBName))
            {
                Directory.CreateDirectory(ServiceOption.DBName);
            }
            m_logger.LogInformation($"Face info database path: {ServiceOption.DBName}");

            if (!string.IsNullOrWhiteSpace(ServiceOption.FeaturePath))
            {
                if (!File.Exists(ServiceOption.FeaturePath))
                {
                    throw new FileNotFoundException($"Feature path must be valid file");
                }
            }
            var featMsg = string.IsNullOrWhiteSpace(ServiceOption.FeaturePath) ? "Use empty feature, no data" : $"Use feature file: {ServiceOption.FeaturePath}";
            m_logger.LogInformation(featMsg);

            if (string.IsNullOrWhiteSpace(ServiceOption.ImageSavePath))
            {
                ServiceOption.ImageSavePath = "./Data/Images";
            }
            if (!Directory.Exists(ServiceOption.ImageSavePath))
            {
                Directory.CreateDirectory(ServiceOption.ImageSavePath);
            }

            m_logger.LogInformation($"Image save to: {ServiceOption.ImageSavePath}");
        }

        public async Task<JsonResponse> AddAsync(IFormFile file)
        {
            if (file == null)
            {
                return JsonResponse.ArgExceedLimit();
            }

            var fileData = await IOHelper.GetFileDataAsync(file);
            if (fileData.Length <= 0)
            {
                return JsonResponse.ArgExceedLimit();
            }

            var feat = FaceEncoding(fileData);
            if (feat == null)
            {
                return JsonResponse.FaceEncodingError();
            }

            var id = Guid.NewGuid().ToString();

            var imagePath = await SaveImageFileAsync(ServiceOption.ImageSavePath, id, file.FileName, fileData);

            return await AddIndexAsync(feat, id, imagePath);
        }

        public async Task<JsonResponse> AddIndexAsync(float[] feat, string id = null, string imagePath = "")
        {
            var bAdd = await Task.FromResult(AddIndex(feat, out var index));
            if (!bAdd)
            {
                return JsonResponse.IndexError();
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
            }
            if (imagePath == null)
            {
                imagePath = "";
            }
            FaceInfo faceInfo = new FaceInfo
            {
                Id = id,
                Index = index,
                ImagePath = imagePath,
                Feature = feat
            };

            bAdd = AddFaceInfo(faceInfo);
            if (!bAdd)
            {
                RemoveIndex(index);
            }

            return new JsonResponse
            {
                Code = 0,
                Msg = "Success",
                Data = new
                {
                    Id = id
                }
            };
        }

        public async Task<JsonResponse> IndexCount()
        {
            var count = await Task.FromResult(m_index.Count);

            return new JsonResponse
            {
                Code = 0,
                Msg = "Success",
                Data = new
                {
                    Count = count
                }
            };
        }

        public async Task<JsonResponse> MatchAsync(IFormFile file)
        {
            if (file == null)
            {
                return JsonResponse.ArgExceedLimit();
            }

            var fileData = await IOHelper.GetFileDataAsync(file);
            if (fileData.Length <= 0)
            {
                return JsonResponse.ArgExceedLimit();
            }

            var feat = FaceEncoding(fileData);
            if (feat == null)
            {
                return JsonResponse.FaceEncodingError();
            }
            return await MatchAsync(feat);
        }

        public async Task<JsonResponse> MatchAsync(float[] feat)
        {
            if (feat.Length != ServiceOption.FeatureLength)
            {
                return JsonResponse.ArgExceedLimit();
            }

            var bSearch = await Task.FromResult(Search(feat, out var searchResult));
            if (!bSearch)
            {
                return JsonResponse.MatchError();
            }

            var dis = searchResult.Select(item => item.Dis);
            var faceInfos = searchResult.Select(item => m_faceInfo.Get(item.Index));
            var ids = faceInfos.Select(item => item.Id);
            var imagePaths = faceInfos
                .Select(item => item.ImagePath)
                .Select(imagePath => $"http://{ServiceOption.PublicIP}/api/FaceRecognition/getImageFile/{Path.GetFileName(imagePath)}");

            return new JsonResponse
            {
                Code = 0,
                Msg = "Success",
                Data = new
                {
                    Ids = ids.ToArray(),
                    Dis = dis.ToArray(),
                    ImageUrls = imagePaths.ToArray()
                }
            };
        }

        public async Task<byte[]> GetImageFileAsync(string fileName)
        {
            var bytes = await File.ReadAllBytesAsync(Path.Join(ServiceOption.ImageSavePath, fileName));

            return bytes;
        }

        private float[] FaceEncoding(byte[] data)
        {
            try
            {
                var encodings = m_faceRecogniser.GetFaceEncodings(data);
                if (encodings == null || encodings.Count() == 0)
                {
                    m_logger.LogError($"No face found");
                    return null;
                }

                return encodings.First().Select(item => (float)item).ToArray();
            }
            catch(Exception ex)
            {
                m_logger.LogError($"{nameof(FaceEncoding)} error: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        private bool Search(float[] feat, out IList<SearchResult> result)
        {
            try
            {
                var searchResult = m_index.Search(feat, ServiceOption.TopK);
                result = searchResult.Where(item => item.Dis <= ServiceOption.Threshold).ToList();
                return true;
            } catch(Exception ex)
            {
                m_logger.LogError($"{nameof(Search)} error: {ex.Message}\n{ex.StackTrace}");
                result = null;
                return false;
            }
        }

        private bool AddIndex(float[] feat, out int index)
        {
            index = -1;
            try
            {
                m_index.Add(feat, out index);
                if (index < 0)
                {
                    m_logger.LogError($"Add index failed: {index}");
                    return false;
                }
                return true;
            } catch(Exception ex)
            {
                m_logger.LogError($"{nameof(AddIndex)} error: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private bool RemoveIndex(int index)
        {
            try
            {
                return m_index.Remove(index, out var feat);
            }catch(Exception ex)
            {
                m_logger.LogError($"{nameof(RemoveIndex)} error: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private bool AddFaceInfo(FaceInfo faceInfo)
        {
            try
            {
                m_faceInfo.Add(faceInfo);
                return true;
            } catch(Exception ex)
            {
                m_logger.LogError($"{nameof(AddFaceInfo)} error: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        private async Task<string> SaveImageFileAsync(string path,string id, string fileName, byte[] data)
        {
            try
            {
                var savePath = Path.Join(path, $"{id}{Path.GetExtension(fileName)}");
                await File.WriteAllBytesAsync(savePath, data);
                return savePath;
            } catch(Exception ex)
            {
                m_logger.LogError($"{nameof(SaveImageFileAsync)} error: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
    }
}
