using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoImporter
{
    class Index
    {
        string WorkingDirectory;
        Dictionary<string, HashSet<string>> DeviceIndices;
        Dictionary<string, HashSet<string>> DeviceIndexUpdates;
        HashSet<FileSign> indexUpdates;
        HashSet<FileSign> index;

        public Index(string workingDirectory)
        {
            WorkingDirectory = Path.Combine(workingDirectory, "index");
            var lines = File.ReadAllLines(Path.Combine(WorkingDirectory, "index.txt"));
            index = new HashSet<FileSign>(lines.Select(i => FileSign.fromString(i)));
        }

        public HashSet<string> LoadForDevice(string serial)
        {
            if (DeviceIndices == null) DeviceIndices = new Dictionary<string, HashSet<string>>();
            var filename = Path.Combine(WorkingDirectory, serial);
            if (File.Exists(filename) && !DeviceIndices.ContainsKey(serial))
                DeviceIndices[serial] = new HashSet<string>(File.ReadAllLines(filename));

            return DeviceIndices[serial];
        }

        public HashSet<string> GetDeviceIndex(string serial)
        {
            return DeviceIndices[serial];
        }

        public void UpdateForDevice(string serial, ICollection<FileInfo> newFiles)
        {
            if (DeviceIndexUpdates == null) DeviceIndexUpdates = new Dictionary<string, HashSet<string>>();
            if (DeviceIndexUpdates[serial] == null) DeviceIndexUpdates[serial] = new HashSet<string>();
            if (DeviceIndices[serial] == null) DeviceIndices[serial] = new HashSet<string>();

            var deviceIndex = DeviceIndices[serial];
            var updates = DeviceIndexUpdates[serial];

            foreach (var file in newFiles)
            {
                deviceIndex.Add(file.Path);
                updates.Add(file.Path);
                if (file.CreationDate != null) {
                    var sign = new FileSign((ulong)file.Size, file.CreationDate.Value);
                    index.Add(sign);
                    indexUpdates.Add(sign);
                }
                
            }
        }

        public void UnloadForDevice(string serial)
        {
            DeviceIndices?.Remove(serial);
        }

        //update

        struct FileSign
        {
            public static FileSign fromString(string line)
            {
                //error handling
                var lr = line.Split(':');
                return new FileSign(
                    uint.Parse(lr[0]),
                    DateTime.FromFileTimeUtc(long.Parse(lr[1]))
                );
            }

            public FileSign(ulong size, DateTime creationTime)
            {
                Size = size;
                CreationTime = creationTime;
            }

            ulong Size;
            DateTime CreationTime;
        }
    }
}
