using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum Token_Class
{
    Int, Float, String, Bool, Char, Array,

    // Punctuation and Delimiters
    Dot, RangeOp, Semicolon, Comma, Colon,
    LParanthesis, RParanthesis,
    LBrace, RBrace, LBracket, RBracket,
    LessThanOp, GreaterThanOp, EqualOp, PlusOp, MinusOp, MultiplyOp, DivideOp,

    LineComment, MultiLineComment, Idenifier, Constant
}

namespace Rynix_Compiler
{
    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {

            ReservedWords.Add("INT", Token_Class.Int);
            ReservedWords.Add("FLOAT", Token_Class.Float);
            ReservedWords.Add("STRING", Token_Class.String);
            ReservedWords.Add("BOOL", Token_Class.Bool);
            ReservedWords.Add("CHAR", Token_Class.Char);
            ReservedWords.Add("ARRAY", Token_Class.Array);

            Operators.Add(".", Token_Class.Dot);
            Operators.Add("..", Token_Class.RangeOp);
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add(":", Token_Class.Colon);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LBrace);
            Operators.Add("}", Token_Class.RBrace);
            Operators.Add("[", Token_Class.LBracket);
            Operators.Add("]", Token_Class.RBracket);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
        }

        public void StartScanning(string SourceCode)
        {
            Tokens.Clear();

            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];


                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n' || CurrentChar == '\t')
                    continue;

                if (CurrentChar == '/')
                {

                    if (i + 1 < SourceCode.Length && SourceCode[i + 1] == '/')
                    {
                        string commentLex = "//";
                        i += 2;
                        while (i < SourceCode.Length && SourceCode[i] != '\n')
                        {
                            commentLex += SourceCode[i];
                            i++;
                        }
                        Tokens.Add(new Token { lex = commentLex, token_type = Token_Class.LineComment });
                        continue;
                    }

                    else if (i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                    {
                        string commentLex = "/*";
                        i += 2;
                        while (i + 1 < SourceCode.Length && !(SourceCode[i] == '*' && SourceCode[i + 1] == '/'))
                        {
                            commentLex += SourceCode[i];
                            i++;
                        }
                        commentLex += "*/";
                        i += 2; 
                        Tokens.Add(new Token { lex = commentLex, token_type = Token_Class.MultiLineComment });
                        continue;
                    }
                }


                if ((CurrentChar >= 'A' && CurrentChar <= 'Z') || (CurrentChar >= 'a' && CurrentChar <= 'z') || CurrentChar == '_')
                {
                    string CurrentLexeme = CurrentChar.ToString();
                    j = i + 1;
                    while (j < SourceCode.Length && ((SourceCode[j] >= 'A' && SourceCode[j] <= 'Z') ||
                                                      (SourceCode[j] >= 'a' && SourceCode[j] <= 'z') ||
                                                      (SourceCode[j] >= '0' && SourceCode[j] <= '9') ||
                                                      SourceCode[j] == '_'))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme);
                }

                else if (CurrentChar >= '0' && CurrentChar <= '9')
                {
                    string CurrentLexeme = CurrentChar.ToString();
                    j = i + 1;
                    while (j < SourceCode.Length && (SourceCode[j] >= '0' && SourceCode[j] <= '9'))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;
                    Tokens.Add(new Token { lex = CurrentLexeme, token_type = Token_Class.Constant });
                }

                else
                {
                    string CurrentLexeme = CurrentChar.ToString();

                    if (CurrentChar == '.' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '.')
                    {
                        CurrentLexeme = "..";
                        i++;
                    }

                    FindTokenClass(CurrentLexeme);
                }
            }

        }

        void FindTokenClass(string Lex)
        {
            Token Tok = new Token();
            Tok.lex = Lex;

            if (ReservedWords.ContainsKey(Lex.ToUpper()))
            {
                Tok.token_type = ReservedWords[Lex.ToUpper()];
            }

            else if (Operators.ContainsKey(Lex))
            {
                Tok.token_type = Operators[Lex];
            }

            else if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Idenifier;
            }

            else if (isConstant(Lex))
            {
                Tok.token_type = Token_Class.Constant;
            }

            else
            {
                Tok.token_type = Token_Class.Idenifier;
            }

            Tokens.Add(Tok);
        }

        bool isIdentifier(string lex)
        {
            if (string.IsNullOrEmpty(lex)) return false;
            char first = lex[0];
            if (!((first >= 'A' && first <= 'Z') || (first >= 'a' && first <= 'z') || first == '_'))
                return false;

            for (int i = 1; i < lex.Length; i++)
            {
                char c = lex[i];
                if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '_'))
                    return false;
            }

            return true;
        }

        bool isConstant(string lex)
        {
            if (string.IsNullOrEmpty(lex)) return false;
            foreach (char c in lex)
            {
                if (c < '0' || c > '9') return false;
            }
            return true;
        }
    }
}