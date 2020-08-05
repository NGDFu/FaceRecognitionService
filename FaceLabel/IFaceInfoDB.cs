using System;

namespace FaceLabel
{
    public interface IFaceInfoDB
    {
        void Add(FaceInfo faceInfo);

        FaceInfo Get(int index);

        void Remove(int index);
    }
}
