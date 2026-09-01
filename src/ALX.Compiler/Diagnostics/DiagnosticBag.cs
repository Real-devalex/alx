namespace ALX.Compiler.Diagnostics;

/// <summary>
/// Collects diagnostic messages during compilation.
/// </summary>
public class DiagnosticBag
{
    private readonly List<Diagnostic> _diagnostics = new();

    public IReadOnlyList<Diagnostic> Diagnostics => _diagnostics;
    public bool HasErrors => _diagnostics.Count > 0;

    public void Add(Diagnostic diagnostic)
    {
        _diagnostics.Add(diagnostic);
    }

    public void ReportUnexpectedCharacter(string sourceFile, int line, int column, char character)
    {
        Add(new Diagnostic(
            "ALX1001",
            $"Unexpected character '{character}'.",
            sourceFile,
            line,
            column
        ));
    }

    public void ReportUnterminatedString(string sourceFile, int line, int column)
    {
        Add(new Diagnostic(
            "ALX1002",
            "Unterminated string literal.",
            sourceFile,
            line,
            column,
            hint: "Strings must be enclosed in matching quotes."
        ));
    }

    public void ReportInvalidNumber(string sourceFile, int line, int column, string text, string type)
    {
        Add(new Diagnostic(
            "ALX1003",
            $"Invalid {type} literal '{text}'.",
            sourceFile,
            line,
            column
        ));
    }

    public void ReportUnexpectedToken(string sourceFile, int line, int column, string expected, string actual, string? sourceLine = null)
    {
        Add(new Diagnostic(
            "ALX2001",
            $"Expected {expected} but found '{actual}'.",
            sourceFile,
            line,
            column,
            sourceLine
        ));
    }

    public void ReportUndefinedVariable(string sourceFile, int line, int column, string name, string? sourceLine = null, string? hint = null)
    {
        Add(new Diagnostic(
            "ALX3001",
            $"Undefined variable '{name}'.",
            sourceFile,
            line,
            column,
            sourceLine,
            hint
        ));
    }

    public void ReportTypeMismatch(string sourceFile, int line, int column, string expected, string actual, string? sourceLine = null)
    {
        Add(new Diagnostic(
            "ALX3002",
            $"Type mismatch: expected {expected}, got {actual}.",
            sourceFile,
            line,
            column,
            sourceLine
        ));
    }

    public void ReportCannotCallNonFunction(string sourceFile, int line, int column, string name, string? sourceLine = null)
    {
        Add(new Diagnostic(
            "ALX3003",
            $"'{name}' is not a function and cannot be called.",
            sourceFile,
            line,
            column,
            sourceLine
        ));
    }

    public void ReportWrongArgumentCount(string sourceFile, int line, int column, string name, int expected, int actual, string? sourceLine = null)
    {
        Add(new Diagnostic(
            "ALX3004",
            $"'{name}' expects {expected} argument(s) but got {actual}.",
            sourceFile,
            line,
            column,
            sourceLine
        ));
    }

    public void ReportUnexpectedReturn(string sourceFile, int line, int column, string? sourceLine = null)
    {
        Add(new Diagnostic(
            "ALX3005",
            "Return statement outside of a function.",
            sourceFile,
            line,
            column,
            sourceLine
        ));
    }

    public void ReportCannotConvert(string sourceFile, int line, int column, string from, string to, string? sourceLine = null)
    {
        Add(new Diagnostic(
            "ALX3006",
            $"Cannot convert value of type {from} to {to}.",
            sourceFile,
            line,
            column,
            sourceLine
        ));
    }

    public void Clear()
    {
        _diagnostics.Clear();
    }
}
