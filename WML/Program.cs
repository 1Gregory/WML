using System;
using System.IO;
using System.Collections.Generic;

namespace WML
{
    class Token
    {
        int type;
        int[] value; // array ith 1 element if it is tab and more than 1 if string

        void set_without(int type)
        {
            this.type = type;
        }
        void set_one(int type, int value)
        {
            this.set_without(type);
            this.value = new int[1] {value};
        }
        void set_much(int type, int[] value)
        {
            this.set_without(type);
            this.value = value;
        }
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
                    Token[] tokens = Lexer(WML_code_reader);
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

        static Token[] Lexer(StreamReader WML_code_reader)
        {
            List<Token> tk_list = new List<Token>();
            while (!WML_code_reader.EndOfStream) // WML_code_reader.Read() == -1
            {
                char ch = (char)WML_code_reader.Read();

                
            }
            return tk_list.ToArray(); // I created this list just for test
        }
    }
}
