using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoImporter
{
    class MtpLogic
    {
        private const uint maxDepthMtpInclusion = 3;    // do not go deeper than this level if no included name in path
        static HashSet<string> extensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".mov", ".mts" };    // only include files with these extensions
        static HashSet<string> exceptions = new HashSet<string> { "\\android\\data\\", "\\com.", "cache", "\\.", "\\magazineunlock" };  // do not include files if path contains any of these
        static HashSet<string> inclusions = new HashSet<string> { "\\dcim", "\\camera" };   // include files only if path contains any of these

        public static string GetDeviceSerial(string deviceName)
        {
            var mediaDevice = MediaDevices.MediaDevice.GetDevices().FirstOrDefault(i => i.FriendlyName == deviceName);
            if (mediaDevice == null) return null;
            mediaDevice.Connect();
            string serial = mediaDevice.SerialNumber;
            mediaDevice.Disconnect();
            return serial;
        }

        public static List<FileInfo> Import(string dst, string deviceName, Index index)
        {
            var device = MediaDevices.MediaDevice.GetDevices().FirstOrDefault(i => i.FriendlyName == deviceName);

            if (device == null)
            {
                Log.W("device is null: " + deviceName);
                return new List<FileInfo>();
            }

            device.Connect();
            var found = EnumerateMtpFiles("\\", device, index.LoadForDevice(device.SerialNumber) ?? new HashSet<string>());
            //filter by index (and possible compare hash?)
            index.UnloadForDevice(device.SerialNumber);
            if (dst != null)
                DoCopy(dst, device, found);

            index.UpdateForDevice(device.SerialNumber, found);
            device.Disconnect();

            return found;
        }

        private static bool DoCopy(string dst, MediaDevices.MediaDevice device, List<FileInfo> files)
        {
            if (!Directory.Exists(dst))
            {
                Log.W("Directory does not exist: " + dst);
                return false;
            }

            if (device == null || !device.IsConnected)
            {
                Log.W("Device is not connected");
                return false;
            }

            files.ForEach(i => Console.WriteLine($"{i} {i.CreationDate}"));
            foreach (var file in files)
            {
                string extension = Path.GetExtension(file.Path);
                string filename = file.CreationDate.Value.ToString("yyyy-mm-dd hh-MM-ss");
                string path = Path.Combine(dst, filename + "." + extension);
                int i = 1;
                while (File.Exists(filename))
                    path = Path.Combine(dst, filename + " (" + ++i + ")." + extension);

                using (var stream = File.Create(path))
                {
                    device.DownloadFile(file.Path, stream);
                }
            }
            
            return true;
        }

        public static List<FileInfo> EnumerateMtpFiles(string path, MediaDevices.MediaDevice device, HashSet<string> skipFiles, int depthLevel = 0)
        {
            List<FileInfo> result = new List<FileInfo>();
            bool included = inclusions.Count == 0 || inclusions.Any(i => path.ToLower().Contains(i));
            if (depthLevel > maxDepthMtpInclusion && !included)
                return result;

            if (exceptions.Any(i => path.ToLower().Contains(i)))
                return result;

            if (included)
                result.AddRange(
                        device.EnumerateFiles(path)
                        .Where(i => !skipFiles.Contains(i))
                        .Where(i => extensions.Any(e => i.ToLower().EndsWith(e)))
                        .Select(i =>
                        {
                            var info = device.GetFileInfo(i);
                            return new FileInfo() { Path = i, CreationDate = info.LastWriteTime, Size = (long)info.Length };
                        })
                        .ToList());

            device.EnumerateDirectories(path).ToList().ForEach(i => result.AddRange(EnumerateMtpFiles(i, device, skipFiles, depthLevel + 1)));

            return result;
        }
    }
}
