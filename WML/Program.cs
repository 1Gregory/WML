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
         2 - attr
         3 - text
         4 - word
         5 - =
         6 - #
         7 - attr without quotes
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
        public Token(int type, string value) // Not the best solurion :(
        {
            this.type = type;
            this.value = new int[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                this.value[i] = value[i]; // Int
            }
        }
    }

    class Tokenizer
    {
        static char[] white_spaces = new char[3] {' ', '\t', '\n'};
         
        private void SplitWordToken()
        { // Firstly split (to organize the order of tokens)
            if (collecting_word)
            {
                my_parser.SendToken(new Token(4, text));
                text = "";
                collecting_word = false;
            }
        }

        private void work_with_symb()
        {
            if (ch == '\'')
            {
                indifference[1] = true;
                SplitWordToken();
            }
            else if (ch == '"')
            {
                SplitWordToken();
                indifference[2] = true;
            }
            else if (ch == '/')
            {
                if (last_ch == '/')
                {
                    one_l_comment = true;
                }
                return;
            }
            else if (ch == '{')
            {
                indifference[0] = true;
                SplitWordToken();
            }
            else if (ch == '=')
            {
                SplitWordToken();
                my_parser.SendToken(new Token(5));
            }
            else if (ch == '#')
            {
                SplitWordToken();
                my_parser.SendToken(new Token(6));
            }
            else if (ch == '\n')
            {
                my_parser.SendToken(new Token(0));
            }
            //part with letters
            else if ((int)ch >= 97 && (int)ch <= 122) // 97[a] <= (int)ch <= 122[z]
            {
                text += ch;
                collecting_word = true;
            }
            else if ((int)ch >= 65 && (int)ch <= 90) // 65[A] <= (int)ch <= 90[Z] {-32}
            {
                text += (char)((int)ch - 32);
                collecting_word = true;
            }
            else
            {
                Console.WriteLine("    Syntax error: unknown symbol");
                throw new Exception();
            }

            if (!do_we_have_letters)
            {
                if (sp_before_letters % 4 == 0)
                {
                    my_parser.SendToken(new Token(1, sp_before_letters / 4));
                }
                else
                {
                    Console.WriteLine("    Syntax error: wrong indint");
                }
            }
        }

        Parser my_parser;
        char ch;
        bool do_we_have_letters = false;
        int sp_before_letters = 0;
        char last_ch = ' '; // Now i declarated it as space (becouse space would not be used)
        bool one_l_comment = false;
        //in_figure_brackets, one_quotation_mark, two_quotation_marks
        bool[] indifference = new bool[3] { false, false, false };
        string text = "";  //TODO: Write a code that cleans the 'text' and SENDS TOKENS TO PARSER!!! BOMBIT!!!!
        bool was_defeated = false;
        bool collecting_word = false;
        bool was_attr_splited = false;

        private void indifference_check(char symb, int state) // In future, i will create a small array of 'symbs'
        {
            if (ch == '\\')
            {
                if (was_defeated) // Not a crutch!!!
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
                if (ch == symb)
                {
                    if (last_ch != '\\' || was_defeated)
                    {
                        // Exit from indifferene is just there
                        indifference[state] = false;
                        if (ch == '}')
                        {
                            my_parser.SendToken(new Token(3, text));
                        }
                        else if (was_attr_splited)
                        {
                            my_parser.SendToken(new Token(7, text));
                            was_attr_splited = false;
                        }
                        else
                        {
                            my_parser.SendToken(new Token(2, text));
                        }
                        text = "";
                    }
                    else
                    {
                        text += symb;
                    }
                }
                else if (ch == '<')
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
                else if (Array.Exists(white_spaces, el => el == ch))
                {
                    if (symb == '}') // cruuuutch!!!!!!!!
                    {
                        if (!Array.Exists(white_spaces, el => el == last_ch))
                        {
                            text += ch;
                        }
                    }
                    else
                    {
                        was_attr_splited = true;
                        text += ch;
                    }
                }
                else {
                    text += ch;
                }       
                was_defeated = false;
            }
        }

        public void Lexer(StreamReader WML_code_reader, StreamWriter HTML_code_writer)
        {
            my_parser = new Parser(HTML_code_writer);
            while (!WML_code_reader.EndOfStream) // WML_code_reader.Read() == -1
            {
                ch = (char)WML_code_reader.Read();

                //There are two ways how to "format" text (in lexer or in parser)
                if (one_l_comment)
                {
                    if (ch == '\n')
                    {
                        one_l_comment = false;
                        sp_before_letters = 0;
                        my_parser.SendToken(new Token(0));
                    }
                }
                else if (indifference[0])
                {
                    indifference_check('}', 0);
                }
                else if (indifference[1])
                {
                    indifference_check('"', 1);
                }
                else if (indifference[2])
                {
                    indifference_check('\'', 2);
                }
                else
                {
                    if (do_we_have_letters)
                    {
                        work_with_symb();
                    }
                    else
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
                        }
                        else // A crutch
                        {
                            work_with_symb();
                        }
                    }
                }
                last_ch = ch;
            }
        }
    }

    class Parser
    {
        Composer my_composer;

        public Parser(StreamWriter HTML_code_writer)
        {
            my_composer = new Composer(HTML_code_writer);
        }

        string[] dont_close_them = {"area", "base", "basefont", "bgsound", "br",
                                                    "col", "command", "embed", "hr", "img",
                                                    "input", "isindex", "keygen", "link", "meta",
                                                    "param", "source", "track", "wbr"};

        string[] dont_format = {"pre", "code"};

        public void SendToken(Token tok)
        {

        }
        // For future development
    }

    class Composer
    {
        public Composer(StreamWriter HTML_code_writer)
        {

        }
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
                    FileStream HTML_code_stream = new FileStream(argvs[1], FileMode.OpenOrCreate);
                    StreamWriter HTML_code_writer = new StreamWriter(HTML_code_stream);
                    Tokenizer my_lexer = new Tokenizer();
                    my_lexer.Lexer(WML_code_reader, HTML_code_writer);
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
