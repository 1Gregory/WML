using System;
using System.IO;
using System.Collections.Generic;

namespace WML
{
    enum tk_types : int
    {
        new_line,
        indent,
        attr,
        text,
        word,
        equality,
        hashtag,
        attr_without_quotes,
        eof,
        tere
    }

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
         8 - EOF (1)
         9 - - ))))000(1)
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
        //StreamReader WML_code_reader;
        Parser my_parser;
        char ch;
        bool do_we_have_letters = false;
        int sp_before_letters = 0;
        char last_ch;
        bool one_l_comment = false;
        //in_figure_brackets, one_quotation_mark, two_quotation_marks
        bool[] indifference = new bool[3] { false, false, false };
        string text = "";
        bool was_defeated = false;
        bool collecting_word = false;
        bool was_attr_splited = false;
        bool need_formating_text = true;

        static char[] white_spaces_1 = {' ', '\t'};
        static char[] white_spaces_2 = {'\n', '\r'};

        public void Lexer(StreamReader WML_code_reader, FileStream HTML_code_stream , StreamWriter HTML_code_writer)
        {
            //this.WML_code_reader = WML_code_reader;
            my_parser = new Parser(HTML_code_stream, HTML_code_writer);
            while (!WML_code_reader.EndOfStream) // WML_code_reader.Read() == -1
            {
                ch = (char)WML_code_reader.Read();

                //There are two ways how to "format" text (in lexer or in parser)
                if (one_l_comment)
                {
                    if (ch == '\n')
                    {
                        if (do_we_have_letters)
                        {
                            Send_Token(new Token(0));
                            do_we_have_letters = false;
                        }
                        one_l_comment = false;
                        sp_before_letters = 0;

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

                else if (ch == '\r') { }
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
                }
                else // A crutch
                {
                    work_with_symb();
                }
                last_ch = ch;
            }
            SplitWordToken(); /* eto norma (no ne fact chto rabotaet(()
            P.S.: eto rabotaet no yavno ne norma
            */
            Send_Token(new Token(8));
        }

        private void work_with_symb()
        {
            if (ch == '"')
            {
                Quartering();
                SplitWordToken();
                indifference[1] = true;
            }
            else if (ch == '\'')
            {
                Quartering();
                SplitWordToken();
                indifference[2] = true;
            }
            else if (ch == '{')
            {
                Quartering();
                SplitWordToken();
                indifference[0] = true;
            }
            else if (ch == '/')
            {
                if (last_ch == '/')
                {
                    one_l_comment = true;
                }
            }

            else if (ch == '-')
            {
                SplitWordToken();
                Send_Token(new Token(9));
            }
            else if (ch == '=')
            {
                Quartering();
                SplitWordToken();
                Send_Token(new Token(5));
            }
            else if (ch == '#')
            {
                // I don't need quatrering and splitwordtoken there but i think yetyo normya
                Quartering();
                SplitWordToken();
                Send_Token(new Token(6));
            }
            else if (ch == '\n')
            {
                do_we_have_letters = false;
                sp_before_letters = 0;
                SplitWordToken();
                Send_Token(new Token(0));
            }
            else if (Array.Exists(white_spaces_1, el => el == ch))
            {
                SplitWordToken();
            }
            //part with letters
            else if ((int)ch >= 97 && (int)ch <= 122) // 97[a] <= (int)ch <= 122[z]
            {
                EnglishLetters(ch);
            }
            else if ((int)ch >= 65 && (int)ch <= 90) // 65[A] <= (int)ch <= 90[Z] {-32}
            {
                EnglishLetters((char)((int)ch - 32));
            }
            else
            {
                Console.WriteLine("    Syntax error: unknown symbol");
                Console.WriteLine("    Symbol code: " + Convert.ToString((int)ch));
                //my_parser.my_composer.HTML_code_writer.Close();
                //WML_code_reader.Close();
                throw new Exception();
            }
        }

        private void SplitWordToken()
        { // Firstly split (to organize the order of tokens)
            if (collecting_word)
            {
                Send_Token(new Token(4, text));
                text = "";
                collecting_word = false;
            }
        }

        /*Repeating parts of code*/

        private void EnglishLetters(char text_char)
        {
            Quartering();
            text += text_char;
            collecting_word = true;
        }

        private void Quart_and_Split()
        {
            Quartering();
            SplitWordToken();
        }

        private void Quartering()
        {
            if (!do_we_have_letters)
            {
                if (sp_before_letters % 4 == 0)
                {
                    Send_Token(new Token(1, sp_before_letters / 4));
                    do_we_have_letters = true;
                }
                else
                {
                    Console.WriteLine("    Syntax error: wrong indint");
                    Console.WriteLine("    Indent: " + Convert.ToString(sp_before_letters));
                    //my_parser.my_composer.HTML_code_writer.Close();
                    //WML_code_reader.Close();
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

        private void WhiteSpacesCheck(char to_add)
        {
            if (!(Array.Exists(white_spaces_1, el => el == last_ch) ||
                        Array.Exists(white_spaces_2, el => el == last_ch)))
            {
                text += to_add;
            }
        }

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
                            Send_Token(new Token(3, text));
                        }
                        else if (was_attr_splited)
                        {
                            Send_Token(new Token(7, text));
                            was_attr_splited = false;
                        }
                        else
                        {
                            Send_Token(new Token(2, text));
                        }
                        text = "";
                    }
                    else
                    {
                        // Maybe it is not a good soluton but
                        if (symb == '"')
                        {
                            text += "&quot";
                        }
                        else if (symb == '\'')
                        {
                            text += "&apos";
                        }
                        else
                        {
                            text += '}';
                        }
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
                /*Crutch Crutch Crutch Crutch Crutch Crutch Crutch Crutch 
                 Crutch Crutch Crutch Crutch Crutch Crutch Crutch Crutch 
                 Crutch Crutch Crutch Crutch Crutch Crutch Crutch Crutch 
                 Crutch Crutch Crutch Crutch Crutch Crutch Crutch Crutch 
                 Crutch Crutch Crutch Crutch Crutch Crutch Crutch Crutch 
                 Crutch Crutch Crutch Crutch Crutch Crutch Crutch Crutch 
                 Crutch Crutch Crutch Crutch Crutch Crutch Crutch Crutch*/
                else if (Array.Exists(white_spaces_1, el=> el == ch) && need_formating_text)
                {
                    WhiteSpacesCheck(ch);
                }
                else if (Array.Exists(white_spaces_2, el => el == ch) && need_formating_text)
                {
                    WhiteSpacesCheck(' ');
                }
                else {
                    text += ch;
                }       
                was_defeated = false;
            }
        }
    }

    class Parser
    {
        public Composer my_composer;

        bool in_header = false;
        int last_tk_type;
        int this_indent = 0;
        bool in_attr = false;
        private int LastVerifiedIndent;

        public Parser(FileStream HTML_code_stream, StreamWriter HTML_code_writer)
        {
            my_composer = new Composer(HTML_code_stream, HTML_code_writer);
        }

        static string[] dont_format = {"pre", "code", "style", "script"}; // I think, it is normal to add last two

        /*
         How to distinguish attribute from tag?

             */

        public bool[] SendToken(Token tok)
        {
            /*if (tok.type == 0)
            {

            }
            else if (tok.type == 1)
            {
                in_header = false;
            }
            else if (tok.type == 2)
            {

            }
            else if (tok.type == 3)
            {

            }
            else if (tok.type == 4)
            {

            }
            else if (tok.type == 5)
            {
                return new bool[1]{false}; // to distinguish attribute from tag
            }
            else if (tok.type == 6)
            {
                in_header = true;
            }
            else if (tok.type == 7)
            {

            }
            else if (tok.type == 8)
            {

            }
            else // Tok.type == 9
            {

            }*/

            if (last_tk_type == (int)tk_types.word)
            {
                if (tok.type == (int)tk_types.equality)
                {
                    in_attr = true;
                }
            }

            last_tk_type = tok.type;
            return new bool[]{false};
            /*
            if (tok.type == 1)
            {
                Console.WriteLine("type: 1, value: " + Convert.ToString(tok.value));
            }
            else if (tok.type == 0 || tok.type == 5 || tok.type == 6 || tok.type == 8)
            {
                Console.WriteLine("type: " + Convert.ToString(tok.type));
            }
            else
            {
                Console.WriteLine("type: " + Convert.ToString(tok.type) + ", value: " + tok.value_2);
            }
            */
        }
    }

    class Level
    {
        public int insideinside_pos = 0;
        public int inside_pos = 0;
        public int after_pos = 0;
    }

    class Composer
    {
        public FileStream HTML_code_stream;
        public StreamWriter HTML_code_writer;

        List<Level> level_list = new List<Level>();

        static string[] dont_close_them = {"area", "base", "basefont", "bgsound", "br",
                                                    "col", "command", "embed", "hr", "img",
                                                    "input", "isindex", "keygen", "link", "meta",
                                                    "param", "source", "track", "wbr"};

        public Composer(FileStream HTML_code_stream, StreamWriter HTML_code_writer)
        {
            this.HTML_code_stream = HTML_code_stream;
            this.HTML_code_writer = HTML_code_writer;
        }
        public void AppendText(string html_text, int level, int pos)
        {
            /*
            0 - inside 2
            1 - inside
            2 - outside
            */

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
                    my_lexer.Lexer(WML_code_reader, HTML_code_stream, HTML_code_writer);
                    //WML_code_reader.Close();
                    //HTML_code_writer.Close();
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
