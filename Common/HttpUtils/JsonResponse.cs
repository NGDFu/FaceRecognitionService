using System;
using System.Collections.Generic;
using System.Text;

namespace Common.HttpUtils
{
    public class JsonResponse
    {
        public int Code { get; set; } = 0;

        public string Msg { get; set; } = "Success";

        public dynamic Data { get; set; }

        public static JsonResponse NewSuccess<T>(T data)
        {
            return new JsonResponse
            {
                Data = data
            };
        }

        public static JsonResponse ArgExceedLimit()
        {
            return NewErrorValue(ErrorCode.ArgExceedLimit);
        }

        public static JsonResponse UnsupportError()
        {
            return NewErrorValue(ErrorCode.UnsupportError);
        }

        public static JsonResponse FaceEncodingError()
        {
            return NewErrorValue(ErrorCode.FaceEncodingError);
        }

        public static JsonResponse IndexError()
        {
            return NewErrorValue(ErrorCode.IndexError);
        }

        public static JsonResponse MatchError()
        {
            return NewErrorValue(ErrorCode.MatchError);
        }

        private static JsonResponse NewErrorValue(ErrorCode errorCode)
        {
            return new JsonResponse
            {
                Code = (int)errorCode,
                Msg = ErrorMsg.GetErrMsg(errorCode),
            };
        }
    }
}
