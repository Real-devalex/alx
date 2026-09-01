using ALX.Compiler.Diagnostics;
using ALX.Compiler.Lexer;
using ALX.Compiler.Parser;
using ALX.Runtime;

namespace ALX.CLI;

/// <summary>
/// ALX command-line interface.
/// Usage: alx <file.alx>
/// </summary>
public class Program
{
    private const string Version = "0.3.0";
    private const string LanguageName = "ALEXION LANGUAGE";
    private const string StudioName = "ALEXION STUDIOS";

    public static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return 0;
        }

        string command = args[0].ToLower();

        switch (command)
        {
            case "--version":
            case "version":
                PrintVersion();
                return 0;

            case "--help":
            case "help":
                PrintHelp();
                return 0;

            default:
                // Treat as a file path
                string filePath = args[0];
                return RunFile(filePath);
        }
    }

    private static int RunFile(string filePath)
    {
        // Resolve the file path
        string fullPath = Path.GetFullPath(filePath);

        if (!File.Exists(fullPath))
        {
            Console.Error.WriteLine($"Error: File not found: {filePath}");
            Console.Error.WriteLine($"  Tried: {fullPath}");
            return 1;
        }

        if (!fullPath.EndsWith(".alx"))
        {
            Console.Error.WriteLine($"Error: Expected an .alx file, got: {filePath}");
            return 1;
        }

        string source = File.ReadAllText(fullPath);
        string sourceFile = Path.GetFileName(fullPath);

        return RunSource(source, sourceFile);
    }

    private static int RunSource(string source, string sourceFile = "<input>")
    {
        var diagnostics = new DiagnosticBag();

        // Step 1: Tokenize
        var lexer = new Lexer(source, sourceFile, diagnostics);
        var tokens = lexer.Tokenize();

        if (diagnostics.HasErrors)
        {
            PrintDiagnostics(diagnostics);
            return 1;
        }

        // Step 2: Parse
        var parser = new Parser(tokens, sourceFile, diagnostics);
        var ast = parser.Parse();

        if (diagnostics.HasErrors)
        {
            PrintDiagnostics(diagnostics);
            return 1;
        }

        // Step 3: Interpret
        var interpreter = new Interpreter(diagnostics);

        try
        {
            interpreter.Execute(ast);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"\nRuntime Error: {ex.Message}");
            return 1;
        }

        if (diagnostics.HasErrors)
        {
            PrintDiagnostics(diagnostics);
            return 1;
        }

        return 0;
    }

    private static void PrintDiagnostics(DiagnosticBag diagnostics)
    {
        foreach (var diagnostic in diagnostics.Diagnostics)
        {
            Console.Error.WriteLine(diagnostic.ToString());
        }
    }

    private static void PrintVersion()
    {
        Console.WriteLine($"ALX {Version}");
        Console.WriteLine($"{LanguageName}");
        Console.WriteLine($"{StudioName}");
    }

    private static void PrintHelp()
    {
        Console.WriteLine("ALX - Alexion Language");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  alx <file.alx>        Run an ALX source file");
        Console.WriteLine("  alx run <file.alx>    Run an ALX source file (explicit)");
        Console.WriteLine("  alx version           Show ALX version");
        Console.WriteLine("  alx help              Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  alx hello.alx");
        Console.WriteLine("  alx version");
        Console.WriteLine();
        Console.WriteLine($"ALX {Version} - {StudioName}");
    }
}
