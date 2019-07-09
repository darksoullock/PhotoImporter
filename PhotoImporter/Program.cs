using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoImporter
{
    class Program
    {
        
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Index index = new Index(@"E:\Pictures\Photos");
            MtpLogic.Import("", "P20 lite", index);

            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds + "ms.");
        }
    }
}
