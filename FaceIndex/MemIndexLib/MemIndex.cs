using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.Serialization.Formatters.Binary;
using MathNet.Numerics;

namespace FaceIndex.MemIndexLib
{
    public class MemIndex : IIndex
    {
        private ConcurrentDictionary<int, float[]> m_feats = new ConcurrentDictionary<int, float[]>();

        public int Count { 
            get 
            { 
                if (m_feats == null)
                {
                    return 0;
                }
                var count = m_feats.Count;
                return count;
            }
        }

        public static MemIndex Create(string path)
        {
            return new MemIndex(path);
        }

        private MemIndex(string path = null)
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                m_feats = (ConcurrentDictionary<int, float[]>)formatter.Deserialize(new MemoryStream(File.ReadAllBytes(path)));
            }
        }

        public void Add(float[] feat, out int index)
        {
            var len = m_feats.Count;
            var bAdd = m_feats.TryAdd(len, feat);
            if (!bAdd)
            {
                index = -1;
            }
            else
            {
                index = len;
            }
        }

        public bool Remove(int index, out float[] value)
        {
            return m_feats.TryRemove(index, out value);
        }

        public IList<SearchResult> Search(float[] feat, int topK)
        {
            return m_feats
                .AsParallel()
                .Select(item =>
                {
                    var dis = Distance.Euclidean(feat, item.Value);
                    return new SearchResult
                    {
                        Index = item.Key,
                        Dis = dis
                    };
                })
                .OrderBy(item => item.Dis)
                .Take(topK)
                .ToList();
        }

        public void Save(string savePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream vectorsStream = new MemoryStream();
            formatter.Serialize(vectorsStream, m_feats);
            File.WriteAllBytes(savePath, vectorsStream.ToArray());
        }
    }
}
