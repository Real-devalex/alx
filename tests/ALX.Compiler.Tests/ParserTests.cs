using ALX.Compiler.AST;
using ALX.Compiler.Diagnostics;
using ALX.Compiler.Lexer;
using Xunit;

namespace ALX.Compiler.Tests;

public class ParserTests
{
    [Fact]
    public void Parse_EmptySource_ReturnsEmptyProgram()
    {
        var ast = Parse("");
        Assert.Empty(ast.Statements);
    }

    [Fact]
    public void Parse_PrintString()
    {
        var ast = Parse("print(\"hello\")");
        Assert.Single(ast.Statements);
        var printStmt = Assert.IsType<PrintStatement>(ast.Statements[0]);
        var strExpr = Assert.IsType<StringExpression>(printStmt.Expression);
        Assert.Equal("hello", strExpr.Value);
    }

    [Fact]
    public void Parse_VariableDeclaration()
    {
        var ast = Parse("x = 42");
        Assert.Single(ast.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(ast.Statements[0]);
        var assign = Assert.IsType<AssignmentExpression>(stmt.Expression);
        Assert.Equal("x", assign.Name);
    }

    [Fact]
    public void Parse_BinaryExpression()
    {
        var ast = Parse("print(10 + 5)");
        var printStmt = Assert.IsType<PrintStatement>(ast.Statements[0]);
        var binary = Assert.IsType<BinaryExpression>(printStmt.Expression);
        Assert.Equal(TokenType.Plus, binary.Operator);
    }

    [Fact]
    public void Parse_OperatorPrecedence()
    {
        var ast = Parse("print(10 + 5 * 2)");
        var printStmt = Assert.IsType<PrintStatement>(ast.Statements[0]);
        var add = Assert.IsType<BinaryExpression>(printStmt.Expression);
        Assert.Equal(TokenType.Plus, add.Operator);
        var mul = Assert.IsType<BinaryExpression>(add.Right);
        Assert.Equal(TokenType.Star, mul.Operator);
    }

    [Fact]
    public void Parse_IfStatement()
    {
        var ast = Parse("if true {\n  print(\"yes\")\n}");
        Assert.Single(ast.Statements);
        var ifStmt = Assert.IsType<IfStatement>(ast.Statements[0]);
        Assert.NotNull(ifStmt.ThenBranch);
        Assert.Null(ifStmt.ElseBranch);
    }

    [Fact]
    public void Parse_IfElseStatement()
    {
        var ast = Parse("if true {\n  print(\"yes\")\n} else {\n  print(\"no\")\n}");
        var ifStmt = Assert.IsType<IfStatement>(ast.Statements[0]);
        Assert.NotNull(ifStmt.ElseBranch);
    }

    [Fact]
    public void Parse_WhileLoop()
    {
        var ast = Parse("while x > 0 {\n  x = x - 1\n}");
        Assert.Single(ast.Statements);
        var whileStmt = Assert.IsType<WhileStatement>(ast.Statements[0]);
        Assert.IsType<BinaryExpression>(whileStmt.Condition);
        Assert.IsType<BlockStatement>(whileStmt.Body);
    }

    [Fact]
    public void Parse_FunctionDeclaration()
    {
        var ast = Parse("function add(a, b) {\n  return a + b\n}");
        Assert.Single(ast.Statements);
        var funcDecl = Assert.IsType<FunctionDeclaration>(ast.Statements[0]);
        Assert.Equal("add", funcDecl.Name);
        Assert.Equal(2, funcDecl.Parameters.Count);
    }

    [Fact]
    public void Parse_FunctionCall()
    {
        var ast = Parse("greet(\"World\")");
        Assert.Single(ast.Statements);
        var stmt = Assert.IsType<ExpressionStatement>(ast.Statements[0]);
        var call = Assert.IsType<CallExpression>(stmt.Expression);
        Assert.Equal("greet", ((IdentifierExpression)call.Callee).Name);
        Assert.Single(call.Arguments);
    }

    [Fact]
    public void Parse_ReturnStatement()
    {
        var ast = Parse("function f() {\n  return 42\n}");
        var funcDecl = Assert.IsType<FunctionDeclaration>(ast.Statements[0]);
        var returnStmt = Assert.IsType<ReturnStatement>(funcDecl.Body.Statements[0]);
        Assert.NotNull(returnStmt.Value);
    }

    // ===== NEW PARSE TESTS (0.2.0) =====

    [Fact]
    public void Parse_ForLoop()
    {
        var ast = Parse("for i in 1..5 {\n  print(i)\n}");
        Assert.Single(ast.Statements);
        var forStmt = Assert.IsType<ForStatement>(ast.Statements[0]);
        Assert.Equal("i", forStmt.VariableName);
        Assert.IsType<RangeExpression>(forStmt.Iterable);
        Assert.IsType<BlockStatement>(forStmt.Body);
    }

    [Fact]
    public void Parse_RangeExpression()
    {
        var ast = Parse("print(1..10)");
        var printStmt = Assert.IsType<PrintStatement>(ast.Statements[0]);
        var range = Assert.IsType<RangeExpression>(printStmt.Expression);
        Assert.IsType<IntegerExpression>(range.Start);
        Assert.IsType<IntegerExpression>(range.End);
    }

    [Fact]
    public void Parse_BreakStatement()
    {
        var ast = Parse("break");
        Assert.Single(ast.Statements);
        Assert.IsType<BreakStatement>(ast.Statements[0]);
    }

    [Fact]
    public void Parse_ContinueStatement()
    {
        var ast = Parse("continue");
        Assert.Single(ast.Statements);
        Assert.IsType<ContinueStatement>(ast.Statements[0]);
    }

    [Fact]
    public void Parse_BreakInLoop()
    {
        var ast = Parse("while true {\n  break\n}");
        var whileStmt = Assert.IsType<WhileStatement>(ast.Statements[0]);
        var block = Assert.IsType<BlockStatement>(whileStmt.Body);
        Assert.IsType<BreakStatement>(block.Statements[0]);
    }

    [Fact]
    public void Parse_ContinueInLoop()
    {
        var ast = Parse("for i in 1..5 {\n  continue\n}");
        var forStmt = Assert.IsType<ForStatement>(ast.Statements[0]);
        var block = Assert.IsType<BlockStatement>(forStmt.Body);
        Assert.IsType<ContinueStatement>(block.Statements[0]);
    }

    [Fact]
    public void Parse_InterpolatedString()
    {
        var ast = Parse("print(\"Hello, {name}!\")");
        var printStmt = Assert.IsType<PrintStatement>(ast.Statements[0]);
        var interp = Assert.IsType<InterpolatedStringExpression>(printStmt.Expression);
        Assert.Equal(3, interp.Parts.Count); // "Hello, ", name, "!"
        Assert.IsType<StringExpression>(interp.Parts[0]);
        Assert.IsType<IdentifierExpression>(interp.Parts[1]);
        Assert.IsType<StringExpression>(interp.Parts[2]);
    }

    [Fact]
    public void Parse_InterpolatedString_MathExpression()
    {
        var ast = Parse("print(\"{x + y}\")");
        var printStmt = Assert.IsType<PrintStatement>(ast.Statements[0]);
        var interp = Assert.IsType<InterpolatedStringExpression>(printStmt.Expression);
        Assert.Single(interp.Parts);
        Assert.IsType<BinaryExpression>(interp.Parts[0]);
    }

    [Fact]
    public void Parse_ConstantDeclaration()
    {
        var ast = Parse("const MAX = 100");
        Assert.Single(ast.Statements);
        var stmt = Assert.IsType<VariableDeclaration>(ast.Statements[0]);
        Assert.Equal("MAX", stmt.Name);
        Assert.True(stmt.IsConstant);
    }

    [Fact]
    public void Parse_ComplexExpression()
    {
        var ast = Parse("print((10 + 5) * 2)");
        var printStmt = Assert.IsType<PrintStatement>(ast.Statements[0]);
        var mul = Assert.IsType<BinaryExpression>(printStmt.Expression);
        Assert.Equal(TokenType.Star, mul.Operator);
        var add = Assert.IsType<BinaryExpression>(mul.Left);
        Assert.Equal(TokenType.Plus, add.Operator);
    }

    [Fact]
    public void Parse_LambdaExpression()
    {
        var ast = Parse("f = lambda(x) { x * 2 }");
        var stmt = Assert.IsType<ExpressionStatement>(ast.Statements[0]);
        var assign = Assert.IsType<AssignmentExpression>(stmt.Expression);
        var lambda = Assert.IsType<LambdaExpression>(assign.Value);
        Assert.Single(lambda.Parameters);
        Assert.Equal("x", lambda.Parameters[0]);
        Assert.IsType<BlockStatement>(lambda.Body);
    }

    [Fact]
    public void Parse_LambdaTwoParams()
    {
        var ast = Parse("f = lambda(a, b) { a + b }");
        var stmt = Assert.IsType<ExpressionStatement>(ast.Statements[0]);
        var assign = Assert.IsType<AssignmentExpression>(stmt.Expression);
        var lambda = Assert.IsType<LambdaExpression>(assign.Value);
        Assert.Equal(2, lambda.Parameters.Count);
    }

    private static ProgramNode Parse(string source, DiagnosticBag? diagnostics = null)
    {
        diagnostics ??= new DiagnosticBag();
        var lexer = new ALX.Compiler.Lexer.Lexer(source, "test.alx", diagnostics);
        var tokens = lexer.Tokenize();
        var parser = new ALX.Compiler.Parser.Parser(tokens, "test.alx", diagnostics);
        return parser.Parse();
    }
}
