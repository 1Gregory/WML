﻿using System;
using System.IO;
using System.Collections.Generic;

namespace WML
{
    class Token
    {
        /*
         0 - new line
         1 - indent at start
         2 - indent at the middle
             */

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

    class Tokenizer
    {
        /*string[] dont_close_them = new string[19] {"area", "base", "basefont", "bgsound", "br",
                                                    "col", "command", "embed", "hr", "img",
                                                    "input", "isindex", "keygen", "link", "meta",
                                                    "param", "source", "track", "wbr"};*/

        private void work_with_symb(char ch)
        {

        }

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
                text += ch;
                was_defeated = false;
            }
        }

        public Token[] Lexer(StreamReader WML_code_reader)
        {
            List<Token> tk_list = new List<Token>();

            while (!WML_code_reader.EndOfStream) // WML_code_reader.Read() == -1
            {
                char ch = (char)WML_code_reader.Read();

                //There are two ways how to "format" text (in lexer or in parser)
                if (indifference[0])
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
                    if (do_we_have_letters)
                    {
                        /*if (ch == '\n')
                        {
                            Token new_l_tk = new Token();
                            new_l_tk.set_without(0); // \n
                            tk_list.Add(new_l_tk);
                            do_we_have_letters = false;
                            sp_before_letters = 0;
                        }
                        else
                        {
                            work_with_symb(ch);
                        }
                        */
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
                        else if (ch == '\n')
                        {
                            //Empty lines skipping (we don't have letters)
                            sp_before_letters = 0;
                            one_l_comment = false;
                        }
                        else
                        {
                            work_with_symb(ch);
                        }
                        /*else if (ch == '/')
                        {
                            if (last_ch == '/')
                            {
                                one_l_comment = true; // When i adding new char, i need to check this variable
                            }
                        }
                        else
                        {
                            if (!one_l_comment)
                            {
                                do_we_have_letters = true;
                                if (sp_before_letters % 4 == 0)
                                {
                                    Token indent_tk = new Token();
                                    indent_tk.set_one(1, sp_before_letters / 4);
                                    tk_list.Add(indent_tk);

                                    //TODO: 
                                }
                                else
                                {
                                    Console.WriteLine("    Syntax Erorr: wrong indent");
                                    throw new Exception();
                                }
                            }

                        }*/
                    }
                }
                last_ch = ch;
            }
            return tk_list.ToArray(); // I created this list just for test
        }
    }

    class Parser
    {
        public void SendToken()
        {

        }
        // For future development
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
                    Token[] tk_arr = my_lexer.Lexer(WML_code_reader);
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
