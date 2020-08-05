using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Common.IOUtils
{
    public static class IOHelper
    {
        public static async Task<byte[]> GetFileDataAsync(IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                byte[] data = new byte[file.Length];
                var iRead = await stream.ReadAsync(data, 0, data.Length);
                if (iRead != data.Length)
                {
                    return null;
                }
                return data;
            }
        }
    }
}
