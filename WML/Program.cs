using System;
using System.IO;
/*
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
*/

namespace WML
{
    class Program
    {
        static void Main(string[] argvs)
        {
            if (argvs.Length == 2)
            {

            }
            else
            {
                Console.WriteLine("    Usage:\n\n    WML <.wml> <.html>\n    To continue press any button...");
                Console.ReadKey();
            }
        }
    }
}
