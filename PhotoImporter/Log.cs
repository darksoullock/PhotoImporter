using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoImporter
{
    class Log
    {
        public static void E(Exception e)
        {
            Console.WriteLine(e.Message);
        }

        public static void W(string message)
        {
            Console.WriteLine(message);
        }

        public static void D(string message)
        {
        }
    }
}
