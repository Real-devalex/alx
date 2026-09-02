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
/// Assignment to a member access: obj.member = value
/// </summary>
public class MemberAssignmentExpression : Expression
{
    public Expression Object { get; }
    public string MemberName { get; }
    public Expression Value { get; }
    public MemberAssignmentExpression(Expression obj, string memberName, Expression value, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Object = obj; MemberName = memberName; Value = value; }
}

/// <summary>
/// New expression: ClassName(args)
/// </summary>
public class NewExpression : Expression
{
    public string ClassName { get; }
    public List<Expression> Arguments { get; }
    public NewExpression(string className, List<Expression> arguments, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { ClassName = className; Arguments = arguments; }
}

/// <summary>
/// This expression: this
/// </summary>
public class ThisExpression : Expression
{
    public ThisExpression(int line, int column, string sourceFile) : base(line, column, sourceFile) { }
}

/// <summary>
/// Super expression: super.method(args) or super(args)
/// </summary>
public class SuperExpression : Expression
{
    public string MethodName { get; }
    public List<Expression> Arguments { get; }
    public SuperExpression(string methodName, List<Expression> arguments, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { MethodName = methodName; Arguments = arguments; }
}

/// <summary>
/// Assignment to an index: arr[index] = value
/// </summary>
public class IndexAssignmentExpression : Expression
{
    public Expression Object { get; }
    public Expression Index { get; }
    public Expression Value { get; }
    public IndexAssignmentExpression(Expression obj, Expression index, Expression value, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Object = obj; Index = index; Value = value; }
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

// ===== 0.4.0: ARRAYS & MAPS =====

/// <summary>
/// Array literal: [1, 2, 3]
/// </summary>
public class ArrayExpression : Expression
{
    public List<Expression> Elements { get; }
    public ArrayExpression(List<Expression> elements, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Elements = elements; }
}

/// <summary>
/// Map literal: { "name": "Hero", "health": 100 }
/// Keys are expressions (typically strings), values are expressions.
/// </summary>
public class MapExpression : Expression
{
    public List<(Expression Key, Expression Value)> Entries { get; }
    public MapExpression(List<(Expression Key, Expression Value)> entries, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Entries = entries; }
}

/// <summary>
/// Index expression: arr[index] or map[key]
/// </summary>
public class IndexExpression : Expression
{
    public Expression Object { get; }
    public Expression Index { get; }
    public IndexExpression(Expression obj, Expression index, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Object = obj; Index = index; }
}

/// <summary>
/// Member access expression: obj.member (dot notation for map properties)
/// </summary>
public class MemberExpression : Expression
{
    public Expression Object { get; }
    public string MemberName { get; }
    public MemberExpression(Expression obj, string memberName, int line, int column, string sourceFile)
        : base(line, column, sourceFile) { Object = obj; MemberName = memberName; }
}
