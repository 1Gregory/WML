using System;
using System.IO;
using System.Collections.Generic;

namespace WML
{
    class Token
    {
        int type;
        int[] value; // array ith 1 element if it is tab and more than 1 if string

        public void set_without(int type)
        {
            this.type = type;
        }
        public void set_one(int type, int value)
        {
            this.set_without(type);
            this.value = new int[1] {value};
        }
        public void set_much(int type, int[] value)
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
                    Console.WriteLine("    WML can't find this file!");
                }
            }
            else
            {
                Console.WriteLine("    Usage:\n\n    WML <.wml> <.html>");
            }
        }

        string[] dont_close_them = new string[19] {"area", "base", "basefont", "bgsound", "br",
                                                    "col", "command", "embed", "hr", "img",
                                                    "input", "isindex", "keygen", "link", "meta",
                                                    "param", "source", "track", "wbr"};

        static Token[] Lexer(StreamReader WML_code_reader)
        {
            List<Token> tk_list = new List<Token>();
            bool do_we_have_letters = false;
            int sp_before_letters = 0;

            while (!WML_code_reader.EndOfStream) // WML_code_reader.Read() == -1
            {
                char ch = (char)WML_code_reader.Read();

                if (do_we_have_letters)
                {
                    if (ch == '\n')
                    {
                        Token new_l_tk = new Token();
                        new_l_tk.set_without(0); // \n
                        tk_list.Add(new_l_tk);
                        do_we_have_letters = false;
                        sp_before_letters = 0;
                    }
                }
                else
                {
                    if (ch == ' ')
                    {
                        sp_before_letters++;
                    }
                    else if (ch == '\t')
                    {
                        sp_before_letters += 4;
                    }
                    else if(ch == '\n')
                    {
                        //Empty lines skipping
                        do_we_have_letters = false;
                        sp_before_letters = 0;
                    }
                    else
                    {
                        do_we_have_letters = true;
                        if (sp_before_letters % 4 == 0)
                        {
                            Token indent_tk = new Token();
                            indent_tk.set_one(1, sp_before_letters / 4);

                            //TODO: 
                        }
                        else
                        {
                            Console.WriteLine("    Syntax Erorr: wrong indent");
                            throw new Exception();
                        }
                    }
                }
                
            }
            return tk_list.ToArray(); // I created this list just for test
        }
    }
}
