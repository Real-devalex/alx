using ALX.Compiler.Diagnostics;
using ALX.Compiler.Lexer;
using ALX.Compiler.Parser;
using ALX.Runtime;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;

namespace ALX.CLI;

/// <summary>
/// ALX command-line interface.
/// Usage: alx <file.alx>
/// </summary>
public class Program
{
    private const string Version = "0.6.0";
    private const string LanguageName = "ALEXION LANGUAGE";
    private const string StudioName = "ALEXION STUDIOS";
    private const string GitHubRepo = "Real-devalex/alx";
    private const string GitHubApiUrl = $"https://api.github.com/repos/{GitHubRepo}/releases/latest";
    private const string DocsUrl = "https://real-devalex.github.io/alx/";

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

            case "run":
                if (args.Length < 2)
                {
                    Console.Error.WriteLine("Usage: alx run <file.alx>");
                    return 1;
                }
                return RunFile(args[1]);

            case "update":
                return RunUpdate();

            case "check":
            case "build":
                Console.Error.WriteLine($"The '{command}' command is not yet implemented.");
                Console.Error.WriteLine("Coming in a future version of ALX.");
                return 1;

            default:
                // Treat as a file path
                string filePath = args[0];
                return RunFile(filePath);
        }
    }

    // ===== UPDATE COMMAND =====

    private static int RunUpdate()
    {
        Console.WriteLine("Checking for updates...");
        Console.WriteLine();

        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", $"ALX-{Version}");

            // Get latest release info
            var response = client.GetAsync(GitHubApiUrl).Result;
            if (!response.IsSuccessStatusCode)
            {
                Console.Error.WriteLine($"Failed to check for updates: {response.StatusCode}");
                Console.Error.WriteLine("Check your internet connection.");
                return 1;
            }

            var json = response.Content.ReadAsStringAsync().Result;

            // Parse version and zip URL (minimal JSON parsing without dependencies)
            string latestVersion = ExtractJsonString(json, "tag_name");
            string zipUrl = FindZipUrl(json);

            if (string.IsNullOrEmpty(latestVersion) || string.IsNullOrEmpty(zipUrl))
            {
                Console.Error.WriteLine("Could not parse release information.");
                return 1;
            }

            // Clean version tag (remove 'v' prefix)
            string cleanVersion = latestVersion.TrimStart('v');

            Console.WriteLine($"Current version:  {Version}");
            Console.WriteLine($"Latest version:   {cleanVersion}");
            Console.WriteLine();

            if (cleanVersion == Version)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("You are already on the latest version!");
                Console.ResetColor();
                return 0;
            }

            Console.WriteLine($"Updating to {cleanVersion}...");

            // Download zip
            string tempZip = Path.Combine(Path.GetTempPath(), $"alx-{cleanVersion}.zip");
            string tempDir = Path.Combine(Path.GetTempPath(), $"alx-update-{cleanVersion}");

            Console.Write("  Downloading... ");
            var zipBytes = client.GetByteArrayAsync(zipUrl).Result;
            File.WriteAllBytes(tempZip, zipBytes);
            Console.WriteLine("done!");

            // Extract
            Console.Write("  Extracting... ");
            if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
            ZipFile.ExtractToDirectory(tempZip, tempDir);
            Console.WriteLine("done!");

            // Find install directory
            string installDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ALX"
            );

            // Backup current version
            if (Directory.Exists(installDir))
            {
                Console.Write("  Backing up current version... ");
                string backupDir = Path.Combine(Path.GetTempPath(), $"alx-backup-{Version}");
                if (Directory.Exists(backupDir)) Directory.Delete(backupDir, true);
                Directory.Move(installDir, backupDir);
                Console.WriteLine("done!");
            }

            // Move new version into place
            Console.Write("  Installing... ");
            Directory.Move(tempDir, installDir);
            Console.WriteLine("done!");

            // Cleanup
            try { File.Delete(tempZip); } catch { }
            try { Directory.Delete(tempDir, true); } catch { }

            // Verify
            string exePath = Path.Combine(installDir, "ALX.CLI.exe");
            if (File.Exists(exePath))
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"ALX updated to {cleanVersion}!");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Restart your terminal to use the new version.");
                Console.WriteLine();
                return 0;
            }
            else
            {
                Console.Error.WriteLine("Update completed but ALX.CLI.exe not found.");
                Console.Error.WriteLine("You may need to re-run install.ps1.");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Update failed: {ex.Message}");
            Console.Error.WriteLine("You can download manually from:");
            Console.Error.WriteLine($"  https://github.com/{GitHubRepo}/releases/latest");
            return 1;
        }
    }

    /// <summary>
    /// Minimal JSON string extractor (no external dependencies).
    /// Finds "key": "value" patterns.
    /// </summary>
    private static string ExtractJsonString(string json, string key)
    {
        // Try both with and without space after colon
        string[] searches = { $"\"{key}\": \"", $"\"{key}\":\"" };
        foreach (var search in searches)
        {
            int start = json.IndexOf(search, StringComparison.Ordinal);
            if (start != -1)
            {
                start += search.Length;
                int end = json.IndexOf('"', start);
                if (end != -1)
                    return json.Substring(start, end - start);
            }
        }
        return "";
    }

    private static string FindZipUrl(string json)
    {
        // Find the first .zip URL in the release assets
        int idx = json.IndexOf(".zip", StringComparison.Ordinal);
        if (idx == -1) return "";

        // Walk backwards to find the URL start
        int urlStart = json.LastIndexOf("https://", idx, StringComparison.Ordinal);
        if (urlStart == -1) return "";

        // Walk forward to find the URL end (closing quote)
        int urlEnd = json.IndexOf('"', idx + 4);
        if (urlEnd == -1) return "";

        return json.Substring(urlStart, urlEnd - urlStart);
    }

    // ===== FILE EXECUTION =====

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
        Console.WriteLine("  alx update            Update ALX to the latest release");
        Console.WriteLine("  alx help              Show this help message");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  alx hello.alx");
        Console.WriteLine("  alx run hello.alx");
        Console.WriteLine("  alx update");
        Console.WriteLine();
        Console.WriteLine($"Docs: {DocsUrl}");
        Console.WriteLine();
        Console.WriteLine($"ALX {Version} - {StudioName}");
    }
}
