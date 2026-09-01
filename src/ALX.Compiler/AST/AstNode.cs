namespace ALX.Compiler.AST;

/// <summary>
/// Base class for all AST nodes in the ALX language.
/// </summary>
public abstract class AstNode
{
    public int Line { get; }
    public int Column { get; }
    public string SourceFile { get; }

    protected AstNode(int line, int column, string sourceFile = "")
    {
        Line = line;
        Column = column;
        SourceFile = sourceFile;
    }
}
