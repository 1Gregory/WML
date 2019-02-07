using System;
using System.IO;

namespace WML
{
    class Token
    {

    }

    class Program
    {
        static void Main(string[] argvs)
        {
            if (argvs.Length == 2)
            {
                if (File.Exists(argvs[0]))
                {
                    FileStream WML_code_stream = new FileStream(argvs[0], FileMode.Open);
                    StreamReader WML_code_reader = new StreamReader(WML_code_stream);
                    string WML_code = WML_code_reader.ReadToEnd();
                    Token[] tokens = Lexer(WML_code);
                }
                else
                {
                    Console.WriteLine("    WML can't find this file!\n    To continue press any button...");
                    Console.ReadKey();
                }
            }
            else
            {
                Console.WriteLine("    Usage:\n\n    WML <.wml> <.html>\n    To continue press any button...");
                Console.ReadKey();
            }
        }

        string[] dont_close_them = new string[19] {"area", "base", "basefont", "bgsound", "br",
                                                    "col", "command", "embed", "hr", "img",
                                                    "input", "isindex", "keygen", "link", "meta",
                                                    "param", "source", "track", "wbr"};

        static Token[] Lexer(string wml_code)
        {
            foreach (char ch in wml_code)
            {

            }
        }
    }
}
