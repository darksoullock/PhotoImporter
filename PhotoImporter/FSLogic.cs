using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoImporter
{
    class FSLogic
    {
        static List<FileInfo> EnumerateFileSystemFiles(string path, HashSet<string> extensions)
        {
            List<FileInfo> result = new List<FileInfo>();
            try
            {
                result.AddRange(
                    Directory.EnumerateFiles(path)
                    .Where(i => extensions.Any(e => i.ToLower().EndsWith(e)))
                    .Select(i => new FileInfo() { Path = i, CreationDate = File.GetCreationTime(i), Size = GetFileSize(i) })
                    .ToList());

                Directory.EnumerateDirectories(path).ToList().ForEach(i => result.AddRange(EnumerateFileSystemFiles(i, extensions)));
            }
            catch (IOException e)
            {
                Log.E(e);
            }
            return result;
        }


        private static long GetFileSize(string filename)
        {
            using (var f = File.OpenRead(filename))
            {
                return f.Length;
            }
        }

        private static DateTime GetFileDate(string file)
        {
            DateTime result;
            file = Path.GetFileNameWithoutExtension(file);
            if (file.Length > 19)
                file = file.Substring(0, 19);

            if (!DateTime.TryParseExact(file, "yyyy-MM-dd HH-mm-ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                result = File.GetCreationTime(file);

            return result;
        }

        static List<DriveInfo> EnumerateExternalDrives()
        {
            var drives = DriveInfo.GetDrives();
            return drives.Where(d => d.DriveType == DriveType.Removable).ToList();
        }


        //static Dictionary<long, DateTime> BuildFilesMap(string path, out List<Tuple<long, DateTime>> duplicateKeys)
        //{
        //    var result = new Dictionary<long, DateTime>();
        //    duplicateKeys = new List<Tuple<long, DateTime>>();
        //    foreach (var file in Directory.EnumerateFiles(path))
        //    {
        //        long size = GetFileSize(file);
        //        if (!result.ContainsKey(size))
        //            result[size] = (GetFileDate(file));
        //        else
        //            duplicateKeys.Add(Tuple.Create(size, GetFileDate(file)));
        //    }

        //    return result;
        //}
    }
}
