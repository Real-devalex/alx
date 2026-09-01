namespace ALX.Compiler.Diagnostics;

/// <summary>
/// Represents a single diagnostic message produced by the compiler.
/// </summary>
public class Diagnostic
{
    public string Code { get; }
    public string Message { get; }
    public string SourceFile { get; }
    public int Line { get; }
    public int Column { get; }
    public string? SourceLine { get; }
    public string? Hint { get; }

    public Diagnostic(string code, string message, string sourceFile, int line, int column, string? sourceLine = null, string? hint = null)
    {
        Code = code;
        Message = message;
        SourceFile = sourceFile;
        Line = line;
        Column = column;
        SourceLine = sourceLine;
        Hint = hint;
    }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine();
        sb.AppendLine($"ALX Error {Code}");
        sb.AppendLine();
        sb.AppendLine($"    {SourceFile}:{Line}:{Column}");
        sb.AppendLine();
        sb.AppendLine($"    {Message}");

        if (SourceLine != null)
        {
            sb.AppendLine();
            sb.AppendLine($"    {SourceLine}");
            sb.AppendLine($"    {new string(' ', Column - 1)}^");
        }

        if (Hint != null)
        {
            sb.AppendLine();
            sb.AppendLine($"    {Hint}");
        }

        return sb.ToString();
    }
}
