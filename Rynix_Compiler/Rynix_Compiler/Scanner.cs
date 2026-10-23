
using System;
using System.Collections.Generic;

namespace Rynix_Compiler
{
    public enum Token_Class
    {
        Int, Float, String, Bool, Char, Array, Void,

        LineComment, MultiLineComment,

        Identifier, number, Constant, StringLiteral, CharLiteral,

        Var, Val, Fun, Class, If, Else, While, For, In,
        Return, True, False, Null, New, Print, Input,
        Break, Continue, Init,

        PlusOp, MinusOp, DivideOp, MultiplyOp, ModulusOp,
        Assign, Less, Greater, Equal, Notequal,
        Lessorequal, Greaterorequal,

        Dot, Range, Comma, Colon,
        LParen, RParen, LBrace, RBrace,
        LBracket, RBracket
    }

    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();

        private readonly Dictionary<string, Token_Class> ReservedWords =
            new Dictionary<string, Token_Class>();

        private readonly Dictionary<string, Token_Class> Operators =
            new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("Int", Token_Class.Int);
            ReservedWords.Add("Float", Token_Class.Float);
            ReservedWords.Add("String", Token_Class.String);
            ReservedWords.Add("Bool", Token_Class.Bool);
            ReservedWords.Add("Char", Token_Class.Char);
            ReservedWords.Add("Array", Token_Class.Array);
            ReservedWords.Add("Void", Token_Class.Void);

            ReservedWords.Add("var", Token_Class.Var);
            ReservedWords.Add("val", Token_Class.Val);
            ReservedWords.Add("fun", Token_Class.Fun);
            ReservedWords.Add("class", Token_Class.Class);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("while", Token_Class.While);
            ReservedWords.Add("for", Token_Class.For);
            ReservedWords.Add("in", Token_Class.In);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("true", Token_Class.True);
            ReservedWords.Add("false", Token_Class.False);
            ReservedWords.Add("null", Token_Class.Null);
            ReservedWords.Add("new", Token_Class.New);
            ReservedWords.Add("print", Token_Class.Print);
            ReservedWords.Add("input", Token_Class.Input);
            ReservedWords.Add("break", Token_Class.Break);
            ReservedWords.Add("continue", Token_Class.Continue);
            ReservedWords.Add("init", Token_Class.Init);

            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("%", Token_Class.ModulusOp);
            Operators.Add("=", Token_Class.Assign);
            Operators.Add("<", Token_Class.Less);
            Operators.Add(">", Token_Class.Greater);
            Operators.Add("==", Token_Class.Equal);
            Operators.Add("!=", Token_Class.Notequal);
            Operators.Add("<=", Token_Class.Lessorequal);
            Operators.Add(">=", Token_Class.Greaterorequal);

            Operators.Add("..", Token_Class.Range);
            Operators.Add(".", Token_Class.Dot);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add(":", Token_Class.Colon);
            Operators.Add("(", Token_Class.LParen);
            Operators.Add(")", Token_Class.RParen);
            Operators.Add("{", Token_Class.LBrace);
            Operators.Add("}", Token_Class.RBrace);
            Operators.Add("[", Token_Class.LBracket);
            Operators.Add("]", Token_Class.RBracket);
        }

        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();
            Errors.Error_List.Clear();

            if (SourceCode == null)
                SourceCode = "";

            int i = 0;

            while (i < SourceCode.Length)
            {
                char current = SourceCode[i];

                if (char.IsWhiteSpace(current))
                {
                    i++;
                    continue;
                }

                // Comments
                if (current == '/' && i + 1 < SourceCode.Length)
                {
                    if (SourceCode[i + 1] == '/')
                    {
                        int start = i;
                        i += 2;

                        while (i < SourceCode.Length &&
                               SourceCode[i] != '\n' &&
                               SourceCode[i] != '\r')
                        {
                            i++;
                        }

                        Tokens.Add(new Token
                        {
                            lex = SourceCode.Substring(start, i - start),
                            token_type = Token_Class.LineComment
                        });

                        continue;
                    }

                    if (SourceCode[i + 1] == '*')
                    {
                        int start = i;
                        i += 2;
                        bool closed = false;

                        while (i + 1 < SourceCode.Length)
                        {
                            if (SourceCode[i] == '*' &&
                                SourceCode[i + 1] == '/')
                            {
                                i += 2;
                                closed = true;
                                break;
                            }

                            i++;
                        }

                        if (!closed)
                        {
                            AddError(
                                "Unclosed multiline comment",
                                SourceCode,
                                start);

                            // The rest of the file belongs to this comment.
                            break;
                        }

                        Tokens.Add(new Token
                        {
                            lex = SourceCode.Substring(start, i - start),
                            token_type = Token_Class.MultiLineComment
                        });

                        continue;
                    }
                }

                // Identifiers and reserved words
                if (IsLetter(current) || current == '_')
                {
                    int start = i;
                    i++;

                    while (i < SourceCode.Length &&
                           (IsLetter(SourceCode[i]) ||
                            char.IsDigit(SourceCode[i]) ||
                            SourceCode[i] == '_'))
                    {
                        i++;
                    }

                    string lexeme =
                        SourceCode.Substring(start, i - start);

                    FindTokenClass(lexeme, SourceCode, start);
                    continue;
                }

                // Integer and Float literals
                if (char.IsDigit(current))
                {
                    int start = i;

                    while (i < SourceCode.Length &&
                           char.IsDigit(SourceCode[i]))
                    {
                        i++;
                    }

                    if (i + 1 < SourceCode.Length &&
                        SourceCode[i] == '.' &&
                        char.IsDigit(SourceCode[i + 1]))
                    {
                        i++;

                        while (i < SourceCode.Length &&
                               char.IsDigit(SourceCode[i]))
                        {
                            i++;
                        }
                    }

                    if (i < SourceCode.Length &&
                        (IsLetter(SourceCode[i]) ||
                         SourceCode[i] == '_'))
                    {
                        while (i < SourceCode.Length &&
                               (IsLetter(SourceCode[i]) ||
                                char.IsDigit(SourceCode[i]) ||
                                SourceCode[i] == '_'))
                        {
                            i++;
                        }

                        AddError(
                            "Invalid identifier or number: " +
                            SourceCode.Substring(start, i - start),
                            SourceCode,
                            start);

                        continue;
                    }

                    string number =
                        SourceCode.Substring(start, i - start);

                    if (IsNumber(number))
                    {
                        Tokens.Add(new Token
                        {
                            lex = number,
                            token_type = Token_Class.number
                        });
                    }
                    else
                    {
                        AddError(
                            "Invalid number: " + number,
                            SourceCode,
                            start);
                    }

                    continue;
                }

                // String literals
                if (current == '"')
                {
                    int start = i;
                    i++;
                    bool closed = false;

                    while (i < SourceCode.Length)
                    {
                        char ch = SourceCode[i];

                        if (ch == '\n' || ch == '\r')
                            break;

                        if (ch == '\\')
                        {
                            if (i + 1 < SourceCode.Length &&
                                SourceCode[i + 1] != '\n' &&
                                SourceCode[i + 1] != '\r')
                            {
                                i += 2;
                                continue;
                            }

                            i++;
                            break;
                        }

                        if (ch == '"')
                        {
                            i++;
                            closed = true;
                            break;
                        }

                        i++;
                    }

                    string lexeme =
                        SourceCode.Substring(start, i - start);

                    if (closed && IsString(lexeme))
                    {
                        Tokens.Add(new Token
                        {
                            lex = lexeme,
                            token_type = Token_Class.StringLiteral
                        });
                    }
                    else
                    {
                        AddError(
                            "Invalid or unclosed string: " + lexeme,
                            SourceCode,
                            start);
                    }

                    continue;
                }

                // Character literals
                if (current == '\'')
                {
                    int start = i;
                    i++;

                    if (i < SourceCode.Length &&
                        SourceCode[i] != '\n' &&
                        SourceCode[i] != '\r' &&
                        SourceCode[i] != '\'')
                    {
                        if (SourceCode[i] == '\\')
                        {
                            if (i + 1 < SourceCode.Length &&
                                SourceCode[i + 1] != '\n' &&
                                SourceCode[i + 1] != '\r')
                            {
                                i += 2;
                            }
                            else
                            {
                                i++;
                            }
                        }
                        else
                        {
                            i++;
                        }
                    }

                    bool closed =
                        i < SourceCode.Length &&
                        SourceCode[i] == '\'';

                    if (closed)
                    {
                        i++;
                    }
                    else
                    {
                        // Recover to the next quote or line break.
                        while (i < SourceCode.Length &&
                               SourceCode[i] != '\'' &&
                               SourceCode[i] != '\n' &&
                               SourceCode[i] != '\r')
                        {
                            i++;
                        }

                        if (i < SourceCode.Length &&
                            SourceCode[i] == '\'')
                        {
                            i++;
                        }
                    }

                    string lexeme =
                        SourceCode.Substring(start, i - start);

                    if (closed && IsChar(lexeme))
                    {
                        Tokens.Add(new Token
                        {
                            lex = lexeme,
                            token_type = Token_Class.CharLiteral
                        });
                    }
                    else
                    {
                        AddError(
                            "Invalid or unclosed character: " + lexeme,
                            SourceCode,
                            start);
                    }

                    continue;
                }

                // Two-character operators and separators
                if (i + 1 < SourceCode.Length)
                {
                    string twoChars = SourceCode.Substring(i, 2);

                    if (Operators.ContainsKey(twoChars))
                    {
                        FindTokenClass(twoChars, SourceCode, i);
                        i += 2;
                        continue;
                    }
                }

                // One-character operators and separators
                string oneChar = current.ToString();

                if (Operators.ContainsKey(oneChar))
                {
                    FindTokenClass(oneChar, SourceCode, i);
                }
                else
                {
                    AddError(
                        "Unrecognized symbol: " + oneChar,
                        SourceCode,
                        i);
                }

                i++;
            }
        }

        private void AddError(
            string message,
            string sourceCode,
            int position)
        {
            int line = 1;
            int column = 1;

            for (int j = 0;
                 j < position && j < sourceCode.Length;
                 j++)
            {
                if (sourceCode[j] == '\n')
                {
                    line++;
                    column = 1;
                }
                else if (sourceCode[j] != '\r')
                {
                    column++;
                }
            }

            Errors.Error_List.Add(
                $"Line {line}, Column {column}: {message}");
        }

        private void FindTokenClass(
            string lexeme,
            string sourceCode,
            int position)
        {
            Token token = new Token();
            token.lex = lexeme;

            if (ReservedWords.TryGetValue(
                lexeme, out Token_Class reservedType))
            {
                token.token_type = reservedType;
                Tokens.Add(token);
            }
            else if (IsIdentifier(lexeme))
            {
                token.token_type = Token_Class.Identifier;
                Tokens.Add(token);
            }
            else if (Operators.TryGetValue(
                lexeme, out Token_Class operatorType))
            {
                token.token_type = operatorType;
                Tokens.Add(token);
            }
            else
            {
                AddError(
                    "Unrecognized lexeme: " + lexeme,
                    sourceCode,
                    position);
            }
        }

        private bool IsIdentifier(string lexeme)
        {
            if (string.IsNullOrEmpty(lexeme))
                return false;

            if (!IsLetter(lexeme[0]) && lexeme[0] != '_')
                return false;

            for (int i = 1; i < lexeme.Length; i++)
            {
                if (!IsLetter(lexeme[i]) &&
                    !char.IsDigit(lexeme[i]) &&
                    lexeme[i] != '_')
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsLetter(char c)
        {
            return (c >= 'A' && c <= 'Z') ||
                   (c >= 'a' && c <= 'z');
        }

        private bool IsNumber(string lexeme)
        {
            if (string.IsNullOrEmpty(lexeme))
                return false;

            int i = 0;
            bool hasIntegerDigit = false;
            bool hasFractionDigit = false;

            while (i < lexeme.Length &&
                   char.IsDigit(lexeme[i]))
            {
                hasIntegerDigit = true;
                i++;
            }

            if (i < lexeme.Length && lexeme[i] == '.')
            {
                i++;

                while (i < lexeme.Length &&
                       char.IsDigit(lexeme[i]))
                {
                    hasFractionDigit = true;
                    i++;
                }
            }

            return i == lexeme.Length &&
                   hasIntegerDigit &&
                   (lexeme.IndexOf('.') == -1 ||
                    hasFractionDigit);
        }

        private bool IsString(string lexeme)
        {
            return lexeme.Length >= 2 &&
                   lexeme[0] == '"' &&
                   lexeme[lexeme.Length - 1] == '"';
        }

        private bool IsChar(string lexeme)
        {
            if (lexeme.Length < 3 ||
                lexeme[0] != '\'' ||
                lexeme[lexeme.Length - 1] != '\'')
            {
                return false;
            }

            string content =
                lexeme.Substring(1, lexeme.Length - 2);

            return content.Length == 1 ||
                   (content.Length == 2 && content[0] == '\\');
        }
    }
}
