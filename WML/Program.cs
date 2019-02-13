using System;
using System.IO;
using System.Collections.Generic;

namespace WML
{
    public class Token
    {
        /*
         0 - new line
         1 - indent at start
         2 - indent at the middle
             */

        int type;
        int[] value; // array ith 1 element if it is tab and more than 1 if string

        public Token(int type)
        {
            this.type = type;
        }
        public Token(int type, int value)
        {
            this.type = type;
            this.value = new int[1] {value};
        }
        public Token(int type, int[] value)
        {
            this.type = type;
            this.value = value;
        }
    }

    class Tokenizer
    {
        private void work_with_symb()
        {

        }

        char ch;
        bool do_we_have_letters = false;
        int sp_before_letters = 0;
        char last_ch = ' '; // Now i declarated it as space (becouse space would not be used)
        bool one_l_comment = false;
        //in_figure_brackets, one_quotation_mark, two_quotation_marks
        bool[] indifference = new bool[3] { false, false, false };
        string text = "";
        bool was_defeated = false;

        private void indifference_check(char ch, char symb, int state)
        {
            if (ch == symb)
            {
                if (last_ch == '\\')
                {
                    if (was_defeated)
                    {
                        indifference[state] = false;
                        was_defeated = false;
                    }
                    else
                    {
                        text += ch;
                        was_defeated = false;
                    }
                }
                else
                {
                    indifference[state] = false;
                }
            }
            else if (ch == '\\')
            {
                if (was_defeated)
                {
                    was_defeated = false;
                }
                else
                {
                    text += '\\';
                    was_defeated = true;
                }
            }
            else
            {
                if (ch == '<')
                {
                    text += "&lt";
                }
                else if(ch == '>')
                {
                    text += "&gt";
                }
                else if (ch == '"')
                {
                    text += "&quot";
                }
                else if (ch == '\'')
                {
                    text += "&apos";
                }
                else if (ch == '&')
                {
                    if (last_ch == '\\' && !was_defeated)
                    {
                        text += '&';
                    }
                    else
                    {
                        text += "&amp";
                    }
                }
                else
                {
                    text += ch;
                }
                was_defeated = false;
            }
        }

        public void Lexer(StreamReader WML_code_reader)
        {
            List<Token> tk_list = new List<Token>();

            while (!WML_code_reader.EndOfStream) // WML_code_reader.Read() == -1
            {
                ch = (char)WML_code_reader.Read();

                //There are two ways how to "format" text (in lexer or in parser)
                if (one_l_comment && ch == '\n')
                {
                    one_l_comment = false;
                }
                else if (indifference[0])
                {
                    indifference_check(ch, '}', 0);
                }
                else if (indifference[1])
                {
                    indifference_check(ch, '"', 1);
                }
                else if (indifference[2])
                {
                    indifference_check(ch, '\'', 2);
                }
                else
                {
                    if (!do_we_have_letters)
                    {
                        if (ch == ' ')
                        {
                            sp_before_letters++;
                            continue;
                        }
                        else if (ch == '\t')
                        {
                            sp_before_letters += 4;
                            continue;
                        }
                        else if (ch == '\n')
                        {
                            //Empty lines skipping (we don't have letters)
                            sp_before_letters = 0;
                            continue;
                        }
                        work_with_symb();
                    }
                }
                last_ch = ch;
            }
        }
    }

    class Parser
    {
        string[] dont_close_them = new string[19] {"area", "base", "basefont", "bgsound", "br",
                                                    "col", "command", "embed", "hr", "img",
                                                    "input", "isindex", "keygen", "link", "meta",
                                                    "param", "source", "track", "wbr"};

        string[] dont_format = new string[2] {"pre", "code"};

        public void SendToken()
        {

        }
        // For future development
    }

    class Composer
    {
        public void SendTokens()
        {

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
                    Tokenizer my_lexer = new Tokenizer();
                    my_lexer.Lexer(WML_code_reader);
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
    }
}
