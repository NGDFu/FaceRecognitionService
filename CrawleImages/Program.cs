using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using AngleSharp.Html.Parser;
using System.Runtime.InteropServices.WindowsRuntime;

namespace CrawleImages
{
    class Program
    {
        static HttpClient Client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        static HtmlParser m_htmlParser = new HtmlParser();

        static void Main(string[] args)
        {
            Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:68.0) Gecko/20100101 Firefox/68.0");

            Start4j4j().Wait();
            Console.WriteLine("Hello World!");
        }

        static async Task Start4j4j()
        {
            var baseUrl = "http://www.4j4j.cn/mxtp/index_{0}.html";
            int startIndex = 1;
            int topicCount = 160;
            var baseTopicUrl = "http://www.4j4j.cn/";
            var savePath = @"D:\Workspace\FaceRecognition\Images\4j4j";

            int dirIndex = 0;

            IEnumerable<string> topics = Enumerable.Empty<string>();

            for(; startIndex <= topicCount; startIndex++)
            {
                Console.WriteLine($"Get topics index: {startIndex}");
                var topicUrl = string.Format(baseUrl, startIndex);
                var topicsTmp = await GetTopics(topicUrl, baseTopicUrl);
                //topics = topics.Concat(topicsTmp);

                Console.WriteLine($"There are {topicsTmp.Count()} topics");

                foreach (var topic in topicsTmp)
                {
                    Console.WriteLine($"Start process topic: {topic}");
                    var nextSavePath = Path.Join(savePath, $"{dirIndex}");
                    Directory.CreateDirectory(nextSavePath);
                    var imageLinks = await GetImageLinksAsync(topic);
                    foreach (var image in imageLinks)
                    {
                        await DownloadImageAsync(image, nextSavePath);
                    }
                    dirIndex++;
                }
            }

            //topics = topics.Distinct();

            //foreach (var topic in topics)
            //{
            //    var nextSavePath = Path.Join(savePath, $"{dirIndex}");
            //    Directory.CreateDirectory(nextSavePath);
            //    var imageLinks = await GetImageLinksAsync(topic);
            //    foreach (var image in imageLinks)
            //    {
            //        await DownloadImageAsync(image, nextSavePath);
            //    }
            //    dirIndex++;
            //}
        }

        static async Task<IEnumerable<string>> GetTopics(string url, string baseTopicUrl)
        {
            var strHtml = await GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(strHtml))
            {
                Console.WriteLine($"Get topics failed: {url}");
                return null;
            }
            var doc = await m_htmlParser.ParseDocumentAsync(strHtml);
            return doc.QuerySelectorAll("li,p,a")
                .Select(item =>
                {
                    var href = item.GetAttribute("href");
                    if (string.IsNullOrWhiteSpace(href) || !href.Contains("/mxtp/photos"))
                    {
                        return null;
                    }
                    return baseTopicUrl + href;
                })
                .Where(_ => null != _)
                .Distinct();
        }

        static async Task<IEnumerable<string>> GetImageLinksAsync(string url)
        {
            var strHtml = await GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(strHtml))
            {
                Console.WriteLine($"Get image links failed: {url}");
                return null;
            }
            var doc = await m_htmlParser.ParseDocumentAsync(strHtml);
            return doc.QuerySelectorAll("li,div,img")
                .Select(item =>
                {
                    var href = item.GetAttribute("src");
                    if (string.IsNullOrWhiteSpace(href) || !href.Contains("upload/star"))
                    {
                        return null;
                    }
                    var width = item.GetAttribute("width");
                    if (string.IsNullOrWhiteSpace(width))
                    {
                        return null;
                    }
                    var tokens = href.Split("_");
                    if (tokens.Length >= 2)
                    {
                        return tokens[0] + "_" + tokens[1];
                    }
                    return href;
                })
                .Where(_ => null != _);
        }

        static async Task<bool> DownloadImageAsync(string imageUrl, string savePath)
        {
            if (imageUrl.Contains(".gif"))
            {
                return true;
            }
            var fileName = GetFileNameFromUrl(imageUrl);

            var filePath = $"{savePath}/{fileName}";
            if (File.Exists(filePath))
            {
                return true;
            }

            Console.WriteLine($"Start crawling {imageUrl}");

            try
            {
                var buffer = await GetBytesAsync(imageUrl);
                if (buffer == null)
                {
                    Console.WriteLine($"Cann't get bytes from {imageUrl}");
                    return false;
                }
                await File.WriteAllBytesAsync(filePath, buffer);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Download image {imageUrl} failed: {ex.Message}");
                return false;
            }
        }

        static async Task<string> GetStringAsync(string url)
        {
            try
            {
                await Task.Delay(1000);
                return await Client?.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(GetStringAsync)} error: {ex.Message}");
                return null;
            }
        }

        static async Task<byte[]> GetBytesAsync(string url)
        {
            try
            {
                await Task.Delay(100);
                var response = await Client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{nameof(GetBytesAsync)} error: {ex.Message}");
                return null;
            }
        }

        static string GetFileNameFromUrl(string url)
        {
            var temps = url.Split('/');
            return temps[temps.Length - 1];
        }
    }
}
