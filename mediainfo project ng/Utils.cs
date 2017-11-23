using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace mediainfo_project_ng
{
    static class Utils
    {
        // TODO: Determine what should be excluded
        private static readonly List<string> ExcludeDirs = new List<string>
        {
            "CDs",
            "Scans"
        };
        private static readonly List<string> ExcludeExts = new List<string>
        {
            ".txt",
            ".log",
            ".torrent"
        };

        public static IEnumerable<string> EnumerateFolder(string folderPath)
        {
            foreach (var file in Directory.GetFiles(folderPath))
            {
                yield return file;
            }
            var folderQueue = new Queue<string>();
            folderQueue.EnqueueRange(Directory.GetDirectories(folderPath));
            while (folderQueue.Count > 0)
            {
                var currentFolder = folderQueue.Dequeue();
                foreach (var file in Directory.GetFiles(currentFolder))
                {
                    yield return file;
                }
                folderQueue.EnqueueRange(Directory.GetDirectories(currentFolder));
            }
        }

        public static async Task<(IEnumerable<FileInfo>,long)> Load(string[] urls, Action<string> progressCallback = null)
        {
            var fileInfos = new List<FileInfo>();
            var sw = new Stopwatch();
            sw.Start();
            foreach (var file in urls.Where(File.Exists))
            {
                progressCallback?.Invoke(file);
                var info = await LoadFile(file);
                fileInfos.Add(info);
            }
            foreach (var dir in urls.Where(Directory.Exists))
            {
                if (ExcludeDirs.Contains(Path.GetFileName(dir))) continue;
                foreach (var file in EnumerateFolder(dir))
                {
                    progressCallback?.Invoke(file);
                    var info = await LoadFile(file);
                    fileInfos.Add(info);
                }
            }
            sw.Stop();
            return (fileInfos.Where(item=>item!=null), sw.ElapsedMilliseconds);
        }

        public static async Task<FileInfo> LoadFile(string path)
        {
            if (!File.Exists(path)) return null;
            if (ExcludeExts.Contains(Path.GetExtension(path))) return null;
            return await Task.Run(() => new FileInfo(path));
        }

        public static int TryParseAsInt(this string s)
        {
            return decimal.TryParse(s, out var i) ? (int) i : 0;
        }

        public static int TryParseAsMillisecond(this string s)
        {
            return TimeSpan.TryParse(s, out var ts) ? (int) ts.TotalMilliseconds : 0;
        }

        private static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> source)
        {
            foreach (var item in source)
            {
                queue.Enqueue(item);
            }
        }
    }
}
