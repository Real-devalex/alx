namespace ALX.Compiler.AST;

public abstract class Statement : AstNode
{
    protected Statement(int line, int column, string sourceFile) : base(line, column, sourceFile) { }
}

public class ProgramNode : AstNode
{
    public List<Statement> Statements { get; }
    public ProgramNode(List<Statement> statements, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Statements = statements; }
}

public class ExpressionStatement : Statement
{
    public Expression Expression { get; }
    public ExpressionStatement(Expression expression, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Expression = expression; }
}

public class VariableDeclaration : Statement
{
    public string Name { get; }
    public Expression Initializer { get; }
    public bool IsConstant { get; }
    public VariableDeclaration(string name, Expression initializer, bool isConstant, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Name = name; Initializer = initializer; IsConstant = isConstant; }
}

public class BlockStatement : Statement
{
    public List<Statement> Statements { get; }
    public BlockStatement(List<Statement> statements, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Statements = statements; }
}

public class IfStatement : Statement
{
    public Expression Condition { get; }
    public Statement ThenBranch { get; }
    public Statement? ElseBranch { get; }
    public IfStatement(Expression condition, Statement thenBranch, Statement? elseBranch, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Condition = condition; ThenBranch = thenBranch; ElseBranch = elseBranch; }
}

public class WhileStatement : Statement
{
    public Expression Condition { get; }
    public Statement Body { get; }
    public WhileStatement(Expression condition, Statement body, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Condition = condition; Body = body; }
}

/// <summary>
/// For loop: for i in 1..10 { ... }
/// </summary>
public class ForStatement : Statement
{
    public string VariableName { get; }
    public Expression Iterable { get; }
    public Statement Body { get; }
    public ForStatement(string variableName, Expression iterable, Statement body, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { VariableName = variableName; Iterable = iterable; Body = body; }
}

public class FunctionDeclaration : Statement
{
    public string Name { get; }
    public List<string> Parameters { get; }
    public BlockStatement Body { get; }
    public FunctionDeclaration(string name, List<string> parameters, BlockStatement body, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Name = name; Parameters = parameters; Body = body; }
}

public class ReturnStatement : Statement
{
    public Expression? Value { get; }
    public ReturnStatement(Expression? value, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Value = value; }
}

public class PrintStatement : Statement
{
    public Expression Expression { get; }
    public PrintStatement(Expression expression, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Expression = expression; }
}

/// <summary>
/// Break statement — exits the innermost loop.
/// </summary>
public class BreakStatement : Statement
{
    public BreakStatement(int line, int column, string sourceFile) : base(line, column, sourceFile) { }
}

/// <summary>
/// Continue statement — skips to the next iteration of the innermost loop.
/// </summary>
public class ContinueStatement : Statement
{
    public ContinueStatement(int line, int column, string sourceFile) : base(line, column, sourceFile) { }
}
