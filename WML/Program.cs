using System;
using System.IO;
using System.Collections.Generic;

namespace WML
{
    public class Token
    {
        /*
         0 - new line (1)
         1 - indent at start (2)     
         2 - attr (3)
         3 - text (3)
         4 - word (3)
         5 - = (1)
         6 - # (1)
         7 - attr without quotes (3)
             */

        public int type;
        public int value;
        public string value_2;

        public Token(int type)
        {
            this.type = type;
        }
        public Token(int type, int value)
        {
            this.type = type;
            this.value = value;
        }
        public Token(int type, string value)
        {
            this.type = type;
            this.value_2 = value;
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
                do_we_have_letters = false;
                sp_before_letters = 0;
                SplitWordToken();
                my_parser.SendToken(new Token(0));
                curs_line++;
            }
            else if (ch == ' ' || ch == '\t')
            {
                SplitWordToken();
            }
            //part with letters
            else if ((int)ch >= 97 && (int)ch <= 122) // 97[a] <= (int)ch <= 122[z]
            {
                text += ch;
                collecting_word = true;  //Not a crutch
            }
            else if ((int)ch >= 65 && (int)ch <= 90) // 65[A] <= (int)ch <= 90[Z] {-32}
            {
                text += (char)((int)ch - 32);
                collecting_word = true;
            }
            else
            {
                Console.WriteLine("    Syntax error: unknown symbol");
                Console.WriteLine("    Symbol code: " + Convert.ToString((int)ch));
                Console.WriteLine("    Cursor line: " + Convert.ToString(curs_line));
                Console.WriteLine("    Cursor position: " + Convert.ToString(cur_pos));
                throw new Exception();
            }

            if (!do_we_have_letters)
            {
                if (sp_before_letters % 4 == 0)
                {
                    my_parser.SendToken(new Token(1, sp_before_letters / 4));
                    do_we_have_letters = true;
                }
                else
                {
                    Console.WriteLine("    Syntax error: wrong indint");
                    Console.WriteLine("    Indent: " + Convert.ToString(sp_before_letters));
                    throw new Exception();
                }
            }
        }

        void Send_Token(Token tok)
        {
            bool[] res = my_parser.SendToken(tok);
            if (res[0])
            {
                need_formating_text = !need_formating_text;
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
        bool need_formating_text = true;

        int cur_pos = 1; // Debugging
        int curs_line = 1;

        private void indifference_check(char symb, int state) // In future, i will create a small array of 'symbs'
        {
            if (ch == '\\')
            {
                if (was_defeated) // Not a crutch!!!
                {
                    // It means that last char was a back slash
                    was_defeated = false;
                }
                else if (last_ch == '\\')
                {
                    // was_defeated = false
                    text += '\\';
                    was_defeated = true;
                }
                // In other cases do nothing
            }
            else
            {
                if (ch == symb)
                {
                    if (last_ch != '\\' || was_defeated) // Maybe it is a crutch (becouse half of variants are irregular), but i don't know a solution
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
                        // I don't know compress attributes or not
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
                        my_parser.SendToken(new Token(0));
                        one_l_comment = false;
                        sp_before_letters = 0;
                        do_we_have_letters = false;
                        curs_line++;
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

                else if (do_we_have_letters)
                {
                    work_with_symb();
                }

                else if (ch == ' ')
                {
                    sp_before_letters++;
                }
                else if (ch == '\t')
                {
                    sp_before_letters += 4;
                }
                else if (ch == '\n')
                {
                    //Empty lines skipping (we don't have letters)
                    sp_before_letters = 0;
                    curs_line++;
                }
                else // A crutch
                {
                    work_with_symb();
                }
                last_ch = ch;
                cur_pos++;
            }
            SplitWordToken(); // eto norma (no ne fact chto rabotaet(()
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

        string[] dont_format = {"pre", "code", "style", "script"}; // I think, it is normal to add last two

        public bool[] SendToken(Token tok)
        {
            if (tok.type == 1)
            {
                Console.WriteLine("type: 1, value: " + Convert.ToString(tok.value));
            }
            else if (tok.type == 0 || tok.type == 5 || tok.type == 6)
            {
                Console.WriteLine("type: " + Convert.ToString(tok.type));
            }
            else
            {
                Console.WriteLine("type: " + Convert.ToString(tok.type) + ", " + Convert.ToString(tok.value_2));
            }
            return new bool[]{false};
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
