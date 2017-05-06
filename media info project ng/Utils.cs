using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaInfoLib;

namespace media_info_project_ng
{
    static class Utils
    {
		// TODO: Determine what should be excluded
        private static List<string> _excludedDirs = new List<string>
        {
            "CDs",
            "Scans"
        };

        public static string LoadDirectory(string dir, ref FileInfoModel fileInfoModel)
        {
            var str = string.Empty;
            foreach (var path in Directory.GetFiles(dir))
            {
                str += LoadFile(path, ref fileInfoModel);
            }
            foreach (var path in Directory.GetDirectories(dir))
            {
                if (_excludedDirs.Contains(Path.GetFileName(path)))
                    continue;
                str += LoadDirectory(path, ref fileInfoModel);
            }
            return str;
        }

        public static string LoadFile(string path, ref FileInfoModel fileInfoModel)
        {
            var sw = new Stopwatch();
            var length = new System.IO.FileInfo(path).Length;
            sw.Start();
            fileInfoModel.AddItems(new List<string> {path});
            sw.Stop();
            return "Loading: " + path + "\r\nCost " + sw.ElapsedMilliseconds + "ms! Length: " +
                   length +
                   "bytes \r\n";
        }
    }
}