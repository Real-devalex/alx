using ALX.Compiler.Diagnostics;

namespace ALX.Compiler.Lexer;

/// <summary>
/// Tokenizes ALX source code into a list of tokens.
/// </summary>
public class Lexer
{
    private readonly string _source;
    private readonly string _sourceFile;
    private readonly DiagnosticBag _diagnostics;
    private readonly List<Token> _tokens = new();
    private int _start;
    private int _current;
    private int _line = 1;
    private int _column = 1;
    private int _startColumn = 1;

    private static readonly Dictionary<string, TokenType> Keywords = new()
    {
        ["function"] = TokenType.Function,
        ["if"] = TokenType.If,
        ["else"] = TokenType.Else,
        ["while"] = TokenType.While,
        ["for"] = TokenType.For,
        ["in"] = TokenType.In,
        ["return"] = TokenType.Return,
        ["true"] = TokenType.True,
        ["false"] = TokenType.False,
        ["null"] = TokenType.NullLiteral,
        ["const"] = TokenType.Const,
        ["and"] = TokenType.And,
        ["or"] = TokenType.Or,
        ["not"] = TokenType.Not,
        ["print"] = TokenType.Print,
        ["break"] = TokenType.Break,
        ["continue"] = TokenType.Continue,
    };

    public Lexer(string source, string sourceFile = "", DiagnosticBag? diagnostics = null)
    {
        _source = source;
        _sourceFile = sourceFile;
        _diagnostics = diagnostics ?? new DiagnosticBag();
    }

    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            _startColumn = _column;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.Eof, "", null, _line, _column, _sourceFile));
        return _tokens;
    }

    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            case '(': AddToken(TokenType.LeftParen); break;
            case ')': AddToken(TokenType.RightParen); break;
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;
            case '[': AddToken(TokenType.LeftBracket); break;
            case ']': AddToken(TokenType.RightBracket); break;
            case ',': AddToken(TokenType.Comma); break;
            case ':': AddToken(TokenType.Colon); break;
            case ';': AddToken(TokenType.Semicolon); break;

            // Dot or DotDot (range)
            case '.':
                if (Match('.'))
                    AddToken(TokenType.DotDot);
                else
                    AddToken(TokenType.Dot);
                break;

            case '+': AddToken(TokenType.Plus); break;
            case '-': AddToken(TokenType.Minus); break;
            case '*': AddToken(TokenType.Star); break;
            case '/':
                if (Match('/'))
                {
                    while (!IsAtEnd() && Peek() != '\n')
                        Advance();
                }
                else
                {
                    AddToken(TokenType.Slash);
                }
                break;
            case '%': AddToken(TokenType.Percent); break;

            case '=':
                AddToken(Match('=') ? TokenType.Equal : TokenType.Assign);
                break;
            case '!':
                AddToken(Match('=') ? TokenType.NotEqual : TokenType.Not);
                break;
            case '<':
                AddToken(Match('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(Match('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;

            case ' ':
            case '\r':
            case '\t':
                break;

            case '\n':
                AddToken(TokenType.Newline);
                break;

            case '"':
                ScanString('"');
                break;
            case '\'':
                ScanString('\'');
                break;

            default:
                if (char.IsDigit(c))
                {
                    ScanNumber();
                }
                else if (char.IsLetter(c) || c == '_')
                {
                    ScanIdentifier();
                }
                else
                {
                    _diagnostics.ReportUnexpectedCharacter(_sourceFile, _line, _startColumn, c);
                }
                break;
        }
    }

    private void ScanString(char quote)
    {
        int startLine = _line;
        int startCol = _startColumn;
        var value = new System.Text.StringBuilder();
        bool hasInterpolation = false;

        while (!IsAtEnd() && Peek() != quote)
        {
            if (Peek() == '\n')
            {
                _diagnostics.ReportUnterminatedString(_sourceFile, startLine, startCol);
                return;
            }

            if (Peek() == '{')
            {
                hasInterpolation = true;
                value.Append(Advance());
                continue;
            }

            if (Peek() == '\\')
            {
                Advance();
                char escaped = Advance();
                switch (escaped)
                {
                    case 'n': value.Append('\n'); break;
                    case 't': value.Append('\t'); break;
                    case '\\': value.Append('\\'); break;
                    case '"': value.Append('"'); break;
                    case '\'': value.Append('\''); break;
                    default:
                        value.Append('\\');
                        value.Append(escaped);
                        break;
                }
            }
            else
            {
                value.Append(Advance());
            }
        }

        if (IsAtEnd())
        {
            _diagnostics.ReportUnterminatedString(_sourceFile, startLine, startCol);
            return;
        }

        Advance(); // Consume closing quote

        string lexeme = _source.Substring(_start, _current - _start);
        string rawValue = value.ToString();

        if (hasInterpolation)
        {
            _tokens.Add(new Token(TokenType.InterpolatedString, lexeme, rawValue, startLine, startCol, _sourceFile));
        }
        else
        {
            _tokens.Add(new Token(TokenType.String, lexeme, rawValue, startLine, startCol, _sourceFile));
        }
    }

    private void ScanNumber()
    {
        bool isFloat = false;

        while (!IsAtEnd() && char.IsDigit(Peek()))
            Advance();

        if (!IsAtEnd() && Peek() == '.' && PeekNext() != null && char.IsDigit(PeekNext()!.Value))
        {
            isFloat = true;
            Advance();
            while (!IsAtEnd() && char.IsDigit(Peek()))
                Advance();
        }

        string lexeme = _source.Substring(_start, _current - _start);

        if (isFloat)
        {
            if (double.TryParse(lexeme, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
            {
                AddToken(TokenType.Float, doubleValue);
            }
            else
            {
                _diagnostics.ReportInvalidNumber(_sourceFile, _line, _startColumn, lexeme, "float");
            }
        }
        else
        {
            if (long.TryParse(lexeme, out long intValue))
            {
                AddToken(TokenType.Integer, intValue);
            }
            else
            {
                _diagnostics.ReportInvalidNumber(_sourceFile, _line, _startColumn, lexeme, "integer");
            }
        }
    }

    private void ScanIdentifier()
    {
        while (!IsAtEnd() && IsAlphaNumeric(Peek()))
            Advance();

        string lexeme = _source.Substring(_start, _current - _start);

        if (Keywords.TryGetValue(lexeme, out TokenType keywordType))
        {
            AddToken(keywordType);
        }
        else
        {
            AddToken(TokenType.Identifier, lexeme);
        }
    }

    private void AddToken(TokenType type, object? literal = null)
    {
        string lexeme = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, lexeme, literal, _line, _startColumn, _sourceFile));
    }

    private char Advance()
    {
        char c = _source[_current++];
        if (c == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
        return c;
    }

    private bool Match(char expected)
    {
        if (IsAtEnd() || _source[_current] != expected) return false;
        _current++;
        _column++;
        return true;
    }

    private char Peek()
    {
        return IsAtEnd() ? '\0' : _source[_current];
    }

    private char? PeekNext()
    {
        return _current + 1 < _source.Length ? _source[_current + 1] : null;
    }

    private bool IsAtEnd()
    {
        return _current >= _source.Length;
    }

    private static bool IsAlphaNumeric(char c)
    {
        return char.IsLetterOrDigit(c) || c == '_';
    }
}
