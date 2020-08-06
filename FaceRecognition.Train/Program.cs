using FaceRecognitionLib;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using MathNet.Numerics;

namespace FaceRecognition.Train
{
    class Program
    {
        static List<string> labels = new List<string>();
        static List<double[]> feats = new List<double[]>();

        static void Main(string[] args)
        {
            //DeleteEmptyDir(@"D:\Workspace\FaceRecognition\Images\lfw");

            //PrepareLfw(@"D:\Workspace\FaceRecognition\Images\lfw", @"D:\Workspace\FaceRecognition\Images\lfwTrain");

            //PrepareLabelAndFeat(@"D:\Workspace\FaceRecognition\Images\lfwTrain", @"D:\Workspace\FaceRecognition\Images/feat.bin", @"D:\Workspace\FaceRecognition\Images/labels.txt");
            Test(@"D:\Workspace\FaceRecognition\Images\lfw", @"D:\Workspace\FaceRecognition\Images\lfwResult");

            Console.WriteLine("Hello World!");
        }

        static void Test(string path, string savePath)
        {
            LoadLabelAndFeat(@"D:\Workspace\FaceRecognition\Images/feat.bin", @"D:\Workspace\FaceRecognition\Images/labels.txt");

            var faceRecognition = FaceRecogniser.Build(@"D:\Workspace\FaceRecognition\FaceRecognition\Data\Models");

            var dirs = Directory.GetDirectories(path);

            int i = 0;
            int dirCount = dirs.Length;
            int iPro = 0;
            foreach (var dir in dirs)
            {
                Console.WriteLine($"Process dir {iPro}/{dirCount} {dir}");
                iPro++;
                var files = Directory.GetFiles(dir);
                Console.WriteLine($"Has file: {files.Length}");
                foreach (var file in files)
                {
                    var tmps = faceRecognition.GetFaceEncodings(File.ReadAllBytes(file));
                    if (tmps == null || tmps.Count == 0)
                    {
                        continue;
                    }
                    var feat = tmps[0];
                    var searchResult = feats
                        .Zip(Enumerable.Range(0, feats.Count))
                        .AsParallel()
                        .Select(item =>
                        {
                            var dis = Distance.Euclidean(feat, item.First);
                            return new
                            {
                                Index = item.Second,
                                Dis = dis
                            };
                        })
                        .OrderBy(item => item.Dis)
                        .Take(20)
                        .ToList();
                    var dstPath = Path.Join(savePath, $"{i}");
                    Directory.CreateDirectory(dstPath);
                    i++;
                    var targetFileName = Path.GetFileName(file);
                    var targetPath = Path.Join(dstPath, $"00_{targetFileName}");
                    File.Copy(file, targetPath);
                    int m = 0;
                    foreach(var search in searchResult)
                    {
                        var dstFile = labels[search.Index];
                        var dstFileName = Path.GetFileName(dstFile);
                        var dis = search.Dis;
                        string tmpFileName = "";
                        if (dis <= 0.3)
                        {
                            tmpFileName = $"11_{m}_{dis}_{dstFileName}";
                        } else
                        {
                            tmpFileName = $"22_{m}_{dis}_{dstFileName}";
                        }
                        m++;
                        var tmpPath = Path.Join(dstPath, tmpFileName);
                        File.Copy(dstFile, tmpPath);
                    }
                }
            }
        }

        static void LoadLabelAndFeat(string featPath, string labelPath)
        {
            labels = File.ReadAllLines(labelPath).ToList();

            BinaryFormatter formatter = new BinaryFormatter();
            var stream = new MemoryStream(File.ReadAllBytes(featPath));
            feats = (List<double[]>)formatter.Deserialize(stream);
        }

        static void PrepareLabelAndFeat(string path, string featSavePath, string labelSavePath)
        {
            var faceRecognition = FaceRecogniser.Build(@"D:\Workspace\FaceRecognition\FaceRecognition\Data\Models");

            var files = Directory.GetFiles(path);

            var count = files.Length;
            var i = 0;

            foreach (var file in files)
            {
                Console.WriteLine($"Get feature {i}/{count}: file");
                i++;
                var tmps = faceRecognition.GetFaceEncodings(File.ReadAllBytes(file));
                if (tmps == null || tmps.Count == 0)
                {
                    continue;
                }
                var feat = tmps[0];
                labels.Add(file);
                feats.Add(feat);
            }

            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream vectorsStream = new MemoryStream();
            formatter.Serialize(vectorsStream, feats);
            File.WriteAllBytes(featSavePath, vectorsStream.ToArray());

            File.WriteAllLines(labelSavePath, labels);
        }

        static void DeleteMoreThan1(string path)
        {
            var faceRecognition = FaceRecogniser.Build(@"D:\Workspace\FaceRecognition\FaceRecognition\Data\Models");

            var files = Directory.GetFiles(path);

            foreach (var file in files)
            {
                var bytes = File.ReadAllBytes(file);
                if (faceRecognition.HasFaces(bytes))
                {
                    File.Delete(file);
                }
            }
        }

        static void DeleteEmptyDir(string path)
        {
            var faceRecognition = FaceRecogniser.Build(@"D:\Workspace\FaceRecognition\FaceRecognition\Data\Models");

            var dirs = Directory.GetDirectories(path);
            var count = dirs.Length;
            int i = 0;

            foreach (var dir in dirs)
            {
                Console.WriteLine($"Process: {i}/{count} {dir}");
                i++;
                var files = Directory.GetFiles(dir);
                if (files.Length == 0)
                {
                    Directory.Delete(dir);
                }
                //foreach(var file in files)
                //{
                //    var bytes = File.ReadAllBytes(file);
                //    if (faceRecognition.HasFaces(bytes))
                //    {
                //        File.Delete(file);
                //    }
                //}
            }

        }

        static void PrepareLfw(string path, string savePath)
        {
            var dirs = Directory.GetDirectories(path);
            var count = dirs.Length;
            int i = 0;

            foreach (var dir in dirs)
            {
                Console.WriteLine($"Process: {i}/{count} {dir}");
                var files = Directory.GetFiles(dir);
                if (files.Length == 0)
                {
                    continue;
                }
                var first = files[0];
                var fileName = Path.GetFileName(first);
                File.Move(first, Path.Join(savePath, fileName));
                i++;
            }
        }
    }
}
