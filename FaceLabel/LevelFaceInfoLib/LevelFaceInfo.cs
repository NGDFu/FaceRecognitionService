using System.IO;
using Newtonsoft.Json;
using Storage.LevelDB;

namespace FaceLabel.LevelFaceInfoLib.LevelFaceInfo
{
    public class LevelFaceInfo : IFaceInfoDB
    {
        private LevelDBClient m_db;

        public static LevelFaceInfo Create(string dbName)
        {
            return new LevelFaceInfo(dbName);
        }

        private LevelFaceInfo(string dbName)
        {
            if (!Directory.Exists(dbName))
            {
                throw new DirectoryNotFoundException($"DB not found: {dbName}");
            }

            m_db = LevelDBClient.Create(dbName);
        }

        public void Add(FaceInfo faceInfo)
        {
            var key = faceInfo.Index;

            var strValue = JsonConvert.SerializeObject(faceInfo);

            m_db.Put($"{key}", strValue);
        }

        public FaceInfo Get(int index)
        {
            var strValue = m_db.Get($"{index}");
            return JsonConvert.DeserializeObject<FaceInfo>(strValue);
        }

        public void Remove(int index)
        {
            m_db.Delete($"{index}");
        }
    }
}
