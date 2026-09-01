using ALX.Compiler.Lexer;

namespace ALX.Compiler.AST;

public abstract class Expression : AstNode
{
    protected Expression(int line, int column, string sourceFile) : base(line, column, sourceFile) { }
}

public class IntegerExpression : Expression
{
    public long Value { get; }
    public IntegerExpression(long value, int line, int column, string sourceFile) : base(line, column, sourceFile) { Value = value; }
}

public class FloatExpression : Expression
{
    public double Value { get; }
    public FloatExpression(double value, int line, int column, string sourceFile) : base(line, column, sourceFile) { Value = value; }
}

public class StringExpression : Expression
{
    public string Value { get; }
    public StringExpression(string value, int line, int column, string sourceFile) : base(line, column, sourceFile) { Value = value; }
}

public class BooleanExpression : Expression
{
    public bool Value { get; }
    public BooleanExpression(bool value, int line, int column, string sourceFile) : base(line, column, sourceFile) { Value = value; }
}

public class NullExpression : Expression
{
    public NullExpression(int line, int column, string sourceFile) : base(line, column, sourceFile) { }
}

public class IdentifierExpression : Expression
{
    public string Name { get; }
    public IdentifierExpression(string name, int line, int column, string sourceFile) : base(line, column, sourceFile) { Name = name; }
}

public class BinaryExpression : Expression
{
    public Expression Left { get; }
    public TokenType Operator { get; }
    public Expression Right { get; }
    public BinaryExpression(Expression left, TokenType op, Expression right, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Left = left; Operator = op; Right = right; }
}

public class UnaryExpression : Expression
{
    public TokenType Operator { get; }
    public Expression Operand { get; }
    public UnaryExpression(TokenType op, Expression operand, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Operator = op; Operand = operand; }
}

public class CallExpression : Expression
{
    public Expression Callee { get; }
    public List<Expression> Arguments { get; }
    public CallExpression(Expression callee, List<Expression> arguments, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Callee = callee; Arguments = arguments; }
}

public class AssignmentExpression : Expression
{
    public string Name { get; }
    public Expression Value { get; }
    public AssignmentExpression(string name, Expression value, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Name = name; Value = value; }
}

/// <summary>
/// Range expression: 1..10
/// </summary>
public class RangeExpression : Expression
{
    public Expression Start { get; }
    public Expression End { get; }
    public RangeExpression(Expression start, Expression end, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Start = start; End = end; }
}

/// <summary>
/// Interpolated string: "Hello, {name}"
/// The raw value contains the string with {expr} placeholders.
/// Parts are split into alternating string literals and expressions.
/// </summary>
public class InterpolatedStringExpression : Expression
{
    public string RawValue { get; }
    public List<Expression> Parts { get; }
    public InterpolatedStringExpression(string rawValue, List<Expression> parts, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { RawValue = rawValue; Parts = parts; }
}

/// <summary>
/// Lambda expression: lambda(x, y) { return x + y }
/// Creates an anonymous function value.
/// </summary>
public class LambdaExpression : Expression
{
    public List<string> Parameters { get; }
    public BlockStatement Body { get; }
    public LambdaExpression(List<string> parameters, BlockStatement body, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Parameters = parameters; Body = body; }
}
