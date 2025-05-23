using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace task
{
    public class Token
    {
        public char Type { get; set; }
        public string Value { get; set; }
        public int Number { get; set; }

        public Token(char type, string value, int number = -1)
        {
            Type = type;
            Value = value;
            Number = number;
        }

        public override string ToString()
        {
            return $"({Type}, {Number})";
        }
    }

    public class LexAn
    {
        enum State { S, I, D, R, C }

        private static readonly string[] Terminals = { "fun", "main", "var", "val", "Int", "Char", "in", "else", "when" };
        private static readonly string[] SingleSeparators = { "{", "}", "(", ")", ":", ";", "," };
        private static readonly string[] PairedSeparators = { "+", "-", "*", "/", "%", "=", "." };
        private static readonly string[] DoubleSeparators = { "+=", "-=", "*=", "/=", "%=", "->", ".." };

        private static readonly List<string> Identifiers = new List<string>();
        private static readonly List<string> Literals = new List<string>();

        private static bool IsEnglishLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private static bool IsLetterOrDigit(char c)
        {
            return IsEnglishLetter(c) || char.IsDigit(c);
        }

        private static List<(ErrorType Type, string Message)> LexicalErrors =
                   new List<(ErrorType Type, string Message)>();

        public static List<Token> InterpString(string inputString,
                                               out List<(ErrorType Type, string Message)> errors)
        {
            List<Token> Tokens = new List<Token>();
            LexicalErrors.Clear();

            Identifiers.Clear();
            Literals.Clear();

            State state = State.S;
            string buffer = "";
            int repeatState = 1;

            for (int i = 0; i < inputString.Length; i++)
            {
                char c = inputString[i];

                if (repeatState == 1) repeatState = 0;

                for (int j = -1; j < repeatState; ++j)
                {

                    switch (state)
                    {
                        case State.S:
                            if (IsEnglishLetter(c))
                            {
                                state = State.I;
                                buffer += c;
                            }
                            else if (char.IsDigit(c))
                            {
                                state = State.D;
                                buffer += c;
                            }
                            else if (c == '\'')         
                            {
                                state = State.C;
                                buffer += c;         
                            }
                            else if (char.IsWhiteSpace(c)) { }
                            else if (SingleSeparators.Contains(c.ToString()))
                            {
                                Tokens.Add(new Token('S', c.ToString(), Array.IndexOf(SingleSeparators, c.ToString())));
                            }
                            else if (PairedSeparators.Contains(c.ToString()))
                            {
                                state = State.R;
                                buffer += c;
                            }
                            else
                            {
                                LexicalErrors.Add((ErrorType.Lexical, $"ошибка: Неизвестный символ '{c}'"));
                            }

                            break;

                        case State.I:
                            if (IsLetterOrDigit(c))
                            {
                                buffer += c;
                            }
                            else
                            {
                                if (buffer.Length > 8)
                                {
                                    string id_before = buffer;
                                    buffer = buffer.Substring(0, 8);
                                    LexicalErrors.Add((ErrorType.Lexical, $"ошибка: Идентификатор '{id_before}' был усечен до '{buffer}'"));
                                }
                                if (Terminals.Contains(buffer))
                                {
                                    Tokens.Add(new Token('T', buffer, Array.IndexOf(Terminals, buffer)));
                                }
                                else
                                {
                                    if (!Identifiers.Contains(buffer))
                                    {
                                        Identifiers.Add(buffer);
                                    }
                                    Tokens.Add(new Token('I', buffer, Identifiers.IndexOf(buffer)));
                                }
                                buffer = "";
                                state = State.S;
                                repeatState = 1;
                            }
                            break;

                        case State.D:
                            if (char.IsDigit(c))
                            {
                                buffer += c;
                            }
                            else
                            {
                                if (!Literals.Contains(buffer))
                                {
                                    Literals.Add(buffer);
                                }
                                Tokens.Add(new Token('L', buffer, Literals.IndexOf(buffer)));
                                buffer = "";
                                state = State.S;
                                repeatState = 1;
                            }
                            break;

                        case State.R:
                            if (DoubleSeparators.Contains(buffer + c))
                            {
                                buffer += c;
                                Tokens.Add(new Token('S', buffer, Array.IndexOf(DoubleSeparators, buffer) + SingleSeparators.Length + PairedSeparators.Length));
                                buffer = "";
                                state = State.S;
                            }
                            else
                            {
                                Tokens.Add(new Token('S', buffer, Array.IndexOf(PairedSeparators, buffer) + SingleSeparators.Length));
                                buffer = "";
                                state = State.S;
                                repeatState = 1;
                            }
                            break;

                        case State.C:
                            if (c == '\'')
                            {
                                buffer += c;
                                int contentLength = buffer.Length - 2;        

                                if (contentLength == 0 || contentLength == 1)
                                {
                                    if (!Literals.Contains(buffer))
                                        Literals.Add(buffer);
                                    Tokens.Add(new Token('L', buffer, Literals.IndexOf(buffer)));
                                }
                                else
                                {
                                    LexicalErrors.Add((ErrorType.Lexical, $"ошибка: Неверный Char литерал: {buffer}"));
                                }

                                buffer = "";
                                state = State.S;
                            }
                            else
                            {
                                if (buffer.Length == 1)
                                {
                                    buffer += c;
                                }
                                else
                                {
                                    LexicalErrors.Add((ErrorType.Lexical, $"ошибка: Символьный литерал слишком длинный или незавершённый," +
                                                                          $"начинается с {buffer}"));

                                    if (!Literals.Contains(buffer))
                                        Literals.Add(buffer);
                                    Tokens.Add(new Token('L', buffer, Literals.IndexOf(buffer)));

                                    buffer = "";
                                    state = State.S;
                                    repeatState = 1;
                                }
                            }
                            break;

                    }
                }
            }

            errors = LexicalErrors;
            return Tokens;
        }
    }
}
