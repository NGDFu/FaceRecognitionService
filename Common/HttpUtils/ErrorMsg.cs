using System;
using System.Collections.Generic;
using System.Text;

namespace Common.HttpUtils
{
    /// <summary>
    /// 错误提示
    /// </summary>
    public static class ErrorMsg
    {
        private static readonly Dictionary<ErrorCode, string> m_msgDic = new Dictionary<ErrorCode, string>
        {
            { ErrorCode.Success, "Success" },
            { ErrorCode.ArgExceedLimit, "Argument exceeds the limit" },
            { ErrorCode.UnsupportError, "Unsuport operation" },
            { ErrorCode.FaceEncodingError, "Face Recognition failed" },
            { ErrorCode.IndexError, "Add index failed" },
            { ErrorCode.MatchError, "Match index failed" },
        };

        public static string GetErrMsg(ErrorCode errorCode)
        {
            string result = null;
            m_msgDic.TryGetValue(errorCode, out result);
            return result;
        }
    }
}
