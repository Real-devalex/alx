using ALX.Compiler.Diagnostics;
using ALX.Compiler.Lexer;
using Xunit;

namespace ALX.Compiler.Tests;

public class LexerTests
{
    [Fact]
    public void Tokenize_EmptySource_ReturnsOnlyEof()
    {
        var tokens = Tokenize("");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Eof, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_SingleLineComment_IgnoresComment()
    {
        var tokens = Tokenize("// this is a comment");
        Assert.Single(tokens);
        Assert.Equal(TokenType.Eof, tokens[0].Type);
    }

    [Theory]
    [InlineData("print", TokenType.Print)]
    [InlineData("function", TokenType.Function)]
    [InlineData("if", TokenType.If)]
    [InlineData("else", TokenType.Else)]
    [InlineData("while", TokenType.While)]
    [InlineData("for", TokenType.For)]
    [InlineData("in", TokenType.In)]
    [InlineData("return", TokenType.Return)]
    [InlineData("true", TokenType.True)]
    [InlineData("false", TokenType.False)]
    [InlineData("null", TokenType.NullLiteral)]
    [InlineData("and", TokenType.And)]
    [InlineData("or", TokenType.Or)]
    [InlineData("not", TokenType.Not)]
    [InlineData("break", TokenType.Break)]
    [InlineData("continue", TokenType.Continue)]
    public void Tokenize_Keywords(string keyword, TokenType expectedType)
    {
        var tokens = Tokenize(keyword);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
        Assert.Equal(keyword, tokens[0].Lexeme);
    }

    [Theory]
    [InlineData("x", "x")]
    [InlineData("myVar", "myVar")]
    [InlineData("_private", "_private")]
    [InlineData("name123", "name123")]
    public void Tokenize_Identifiers(string input, string expectedLexeme)
    {
        var tokens = Tokenize(input);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Identifier, tokens[0].Type);
        Assert.Equal(expectedLexeme, tokens[0].Lexeme);
    }

    [Theory]
    [InlineData("42", 42L)]
    [InlineData("0", 0L)]
    [InlineData("999999", 999999L)]
    public void Tokenize_Integers(string input, long expectedValue)
    {
        var tokens = Tokenize(input);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Integer, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Literal);
    }

    [Theory]
    [InlineData("3.14", 3.14)]
    [InlineData("0.0", 0.0)]
    [InlineData("100.5", 100.5)]
    public void Tokenize_Floats(string input, double expectedValue)
    {
        var tokens = Tokenize(input);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.Float, tokens[0].Type);
        Assert.Equal(expectedValue, (double)tokens[0].Literal!);
    }

    [Theory]
    [InlineData("\"hello\"", "hello")]
    [InlineData("'world'", "world")]
    [InlineData("\"line1\\nline2\"", "line1\nline2")]
    [InlineData("\"tab\\there\"", "tab\there")]
    [InlineData("\"escaped\\\"quote\"", "escaped\"quote")]
    public void Tokenize_Strings(string input, string expectedValue)
    {
        var tokens = Tokenize(input);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(TokenType.String, tokens[0].Type);
        Assert.Equal(expectedValue, tokens[0].Literal);
    }

    [Theory]
    [InlineData("+", TokenType.Plus)]
    [InlineData("-", TokenType.Minus)]
    [InlineData("*", TokenType.Star)]
    [InlineData("/", TokenType.Slash)]
    [InlineData("%", TokenType.Percent)]
    [InlineData("=", TokenType.Assign)]
    [InlineData("==", TokenType.Equal)]
    [InlineData("!=", TokenType.NotEqual)]
    [InlineData("<", TokenType.Less)]
    [InlineData(">", TokenType.Greater)]
    [InlineData("<=", TokenType.LessEqual)]
    [InlineData(">=", TokenType.GreaterEqual)]
    [InlineData("(", TokenType.LeftParen)]
    [InlineData(")", TokenType.RightParen)]
    [InlineData("{", TokenType.LeftBrace)]
    [InlineData("}", TokenType.RightBrace)]
    [InlineData("[", TokenType.LeftBracket)]
    [InlineData("]", TokenType.RightBracket)]
    [InlineData(",", TokenType.Comma)]
    [InlineData(".", TokenType.Dot)]
    [InlineData(":", TokenType.Colon)]
    public void Tokenize_OperatorsAndDelimiters(string input, TokenType expectedType)
    {
        var tokens = Tokenize(input);
        Assert.Equal(2, tokens.Count);
        Assert.Equal(expectedType, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_DotDot_RangeOperator()
    {
        var tokens = Tokenize("1..5");
        Assert.Equal(4, tokens.Count); // Integer, DotDot, Integer, Eof
        Assert.Equal(TokenType.Integer, tokens[0].Type);
        Assert.Equal(TokenType.DotDot, tokens[1].Type);
        Assert.Equal(TokenType.Integer, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_DotDot_NotConfusedWithFloat()
    {
        var tokens = Tokenize("1..10");
        Assert.Equal(TokenType.Integer, tokens[0].Type);
        Assert.Equal(TokenType.DotDot, tokens[1].Type);
        Assert.Equal(TokenType.Integer, tokens[2].Type);
        Assert.Equal(10L, tokens[2].Literal);
    }

    [Fact]
    public void Tokenize_DotDot_InContext()
    {
        var tokens = Tokenize("for i in 1..5");
        // for, i, in, 1, .., 5, Eof
        Assert.Equal(TokenType.For, tokens[0].Type);
        Assert.Equal(TokenType.Identifier, tokens[1].Type);
        Assert.Equal(TokenType.In, tokens[2].Type);
        Assert.Equal(TokenType.Integer, tokens[3].Type);
        Assert.Equal(TokenType.DotDot, tokens[4].Type);
        Assert.Equal(TokenType.Integer, tokens[5].Type);
    }

    [Fact]
    public void Tokenize_InterpolatedString()
    {
        var tokens = Tokenize("\"Hello, {name}!\"");
        Assert.Single(tokens.Where(t => t.Type != TokenType.Eof));
        var token = tokens.First(t => t.Type == TokenType.InterpolatedString);
        Assert.Equal("Hello, {name}!", token.Literal);
    }

    [Fact]
    public void Tokenize_InterpolatedString_MultipleExpressions()
    {
        var tokens = Tokenize("\"{x} + {y}\"");
        var token = tokens.First(t => t.Type == TokenType.InterpolatedString);
        Assert.Equal("{x} + {y}", token.Literal);
    }

    [Fact]
    public void Tokenize_InterpolatedString_NoBraces_IsRegularString()
    {
        var tokens = Tokenize("\"Hello World\"");
        Assert.Equal(TokenType.String, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_Break()
    {
        var tokens = Tokenize("break");
        Assert.Equal(TokenType.Break, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_Continue()
    {
        var tokens = Tokenize("continue");
        Assert.Equal(TokenType.Continue, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_PreservesLineNumbers()
    {
        var tokens = Tokenize("a\nb\nc");
        Assert.Equal(1, tokens[0].Line);
        var bToken = tokens.First(t => t.Type == TokenType.Identifier && t.Lexeme == "b");
        Assert.Equal(2, bToken.Line);
        var cToken = tokens.First(t => t.Type == TokenType.Identifier && t.Lexeme == "c");
        Assert.Equal(3, cToken.Line);
    }

    [Fact]
    public void Tokenize_UnterminatedString_ReportsDiagnostic()
    {
        var diagnostics = new DiagnosticBag();
        Tokenize("\"unterminated", diagnostics);
        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics.Diagnostics, d => d.Code == "ALX1002");
    }

    [Fact]
    public void Tokenize_ReportsErrorsForInvalidInput()
    {
        var diagnostics = new DiagnosticBag();
        Tokenize("@", diagnostics);
        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics.Diagnostics, d => d.Code == "ALX1001");
    }

    [Fact]
    public void Tokenize_CompleteProgram()
    {
        var source = @"name = ""ALEXION STUDIOS""
version = 1
print(name)";
        var tokens = Tokenize(source);
        Assert.DoesNotContain(tokens, t => t.Type == TokenType.Error);
        Assert.Equal(TokenType.Eof, tokens[^1].Type);
    }

    private static List<Token> Tokenize(string source, DiagnosticBag? diagnostics = null)
    {
        var lexer = new ALX.Compiler.Lexer.Lexer(source, "test.alx", diagnostics);
        return lexer.Tokenize();
    }
}
