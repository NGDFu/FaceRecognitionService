using System;
using System.Collections.Generic;
using System.Text;

namespace Common.HttpUtils
{
    public enum ErrorCode
    {
        Success = 0,
        ArgExceedLimit,
        UnsupportError,
        FaceEncodingError,
        IndexError,
        MatchError,
    }
}
