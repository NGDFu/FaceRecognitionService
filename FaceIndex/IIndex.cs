using System;
using System.Collections.Generic;

namespace FaceIndex
{
    public interface IIndex
    {
        int Count { get; }

        void Add(float[] feat, out int index);

        IList<SearchResult> Search(float[] feat, int topK);

        bool Remove(int index, out float[] value);

        void Save(string savePath);
    }
}
