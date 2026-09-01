namespace ALX.Compiler.Lexer;

/// <summary>
/// Represents a single token produced by the ALX lexer.
/// </summary>
public class Token
{
    public TokenType Type { get; }
    public string Lexeme { get; }
    public object? Literal { get; }
    public int Line { get; }
    public int Column { get; }
    public string SourceFile { get; }

    public Token(TokenType type, string lexeme, object? literal, int line, int column, string sourceFile = "")
    {
        Type = type;
        Lexeme = lexeme;
        Literal = literal;
        Line = line;
        Column = column;
        SourceFile = sourceFile;
    }

    public override string ToString()
    {
        return $"[{Type}] '{Lexeme}' @ {Line}:{Column}";
    }
}
