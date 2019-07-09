using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoImporter
{
    class FileInfo
    {
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime? CreationDate { get; set; }

        public override string ToString()
        {
            return $"{Path} {Size}";
        }
    }
}
