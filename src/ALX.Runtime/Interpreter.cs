using ALX.Compiler.AST;
using ALX.Compiler.Diagnostics;
using ALX.Compiler.Lexer;
using ALX.Runtime.Values;

namespace ALX.Runtime;

public class Interpreter
{
    private AlxEnvironment _environment;
    private readonly AlxEnvironment _rootEnvironment;
    private readonly DiagnosticBag _diagnostics;
    private readonly Action<string> _output;

    public Interpreter(DiagnosticBag? diagnostics = null, Action<string>? output = null)
    {
        _diagnostics = diagnostics ?? new DiagnosticBag();
        _environment = new AlxEnvironment();
        _rootEnvironment = _environment;
        _output = output ?? Console.WriteLine;
    }

    public void Execute(ProgramNode program)
    {
        foreach (var statement in program.Statements)
            ExecuteStatement(statement);
    }

    private void ExecuteStatement(Statement statement)
    {
        switch (statement)
        {
            case ExpressionStatement expr:
                Evaluate(expr.Expression);
                break;
            case VariableDeclaration varDecl:
                ExecuteVariableDeclaration(varDecl);
                break;
            case BlockStatement block:
                ExecuteBlock(block, new AlxEnvironment(_environment));
                break;
            case IfStatement ifStmt:
                ExecuteIfStatement(ifStmt);
                break;
            case WhileStatement whileStmt:
                ExecuteWhileStatement(whileStmt);
                break;
            case ForStatement forStmt:
                ExecuteForStatement(forStmt);
                break;
            case FunctionDeclaration funcDecl:
                ExecuteFunctionDeclaration(funcDecl);
                break;
            case ReturnStatement returnStmt:
                ExecuteReturnStatement(returnStmt);
                break;
            case PrintStatement printStmt:
                ExecutePrintStatement(printStmt);
                break;
            case BreakStatement:
                throw new BreakException();
            case ContinueStatement:
                throw new ContinueException();
        }
    }

    private void ExecuteVariableDeclaration(VariableDeclaration varDecl)
    {
        var value = Evaluate(varDecl.Initializer);
        _environment.Define(varDecl.Name, value, varDecl.IsConstant);
    }

    private void ExecuteBlock(BlockStatement block, AlxEnvironment blockEnv)
    {
        var previous = _environment;
        _environment = blockEnv;
        try
        {
            foreach (var statement in block.Statements)
                ExecuteStatement(statement);
        }
        finally
        {
            _environment = previous;
        }
    }

    private void ExecuteIfStatement(IfStatement ifStmt)
    {
        var condition = Evaluate(ifStmt.Condition);
        if (condition.IsTruthy())
            ExecuteStatement(ifStmt.ThenBranch);
        else if (ifStmt.ElseBranch != null)
            ExecuteStatement(ifStmt.ElseBranch);
    }

    private void ExecuteWhileStatement(WhileStatement whileStmt)
    {
        while (Evaluate(whileStmt.Condition).IsTruthy())
        {
            try
            {
                ExecuteStatement(whileStmt.Body);
            }
            catch (BreakException) { break; }
            catch (ContinueException) { /* skip to next iteration */ }
        }
    }

    /// <summary>
    /// Execute a for loop: for i in start..end { body }
    /// </summary>
    private void ExecuteForStatement(ForStatement forStmt)
    {
        var iterable = Evaluate(forStmt.Iterable);

        if (iterable is RangeValue range)
        {
            var loopEnv = new AlxEnvironment(_environment);
            var previous = _environment;
            _environment = loopEnv;

            try
            {
                for (long i = range.Start; i < range.End; i++)
                {
                    // Define or update the loop variable
                    if (!_environment.Set(forStmt.VariableName, new IntegerValue(i)))
                    {
                        _environment.Define(forStmt.VariableName, new IntegerValue(i));
                    }

                    try
                    {
                        ExecuteStatement(forStmt.Body);
                    }
                    catch (BreakException)
                    {
                        break;
                    }
                    catch (ContinueException)
                    {
                        // Skip to next iteration
                    }
                }
            }
            finally
            {
                _environment = previous;
            }
        }
    }

    private void ExecuteFunctionDeclaration(FunctionDeclaration funcDecl)
    {
        var func = new AlxFunction(funcDecl, _environment);
        _environment.Define(funcDecl.Name, func);
    }

    private void ExecuteReturnStatement(ReturnStatement returnStmt)
    {
        AlxValue? value = null;
        if (returnStmt.Value != null)
            value = Evaluate(returnStmt.Value);
        throw new ReturnException(value ?? NullValue.Instance);
    }

    private void ExecutePrintStatement(PrintStatement printStmt)
    {
        var value = Evaluate(printStmt.Expression);
        _output(FormatValue(value));
    }

    // ===== EXPRESSION EVALUATION =====

    public AlxValue Evaluate(Expression expression)
    {
        return expression switch
        {
            IntegerExpression intExpr => new IntegerValue(intExpr.Value),
            FloatExpression floatExpr => new FloatValue(floatExpr.Value),
            StringExpression strExpr => new StringValue(strExpr.Value),
            BooleanExpression boolExpr => BooleanValue.FromBool(boolExpr.Value),
            NullExpression => NullValue.Instance,
            IdentifierExpression identExpr => EvaluateIdentifier(identExpr),
            BinaryExpression binaryExpr => EvaluateBinary(binaryExpr),
            UnaryExpression unaryExpr => EvaluateUnary(unaryExpr),
            CallExpression callExpr => EvaluateCall(callExpr),
            AssignmentExpression assignExpr => EvaluateAssignment(assignExpr),
            RangeExpression rangeExpr => EvaluateRange(rangeExpr),
            InterpolatedStringExpression interpExpr => EvaluateInterpolatedString(interpExpr),
            _ => throw new InvalidOperationException($"Unknown expression type: {expression.GetType().Name}")
        };
    }

    private AlxValue EvaluateIdentifier(IdentifierExpression expr)
    {
        var value = _environment.Get(expr.Name);
        if (value == null)
        {
            _diagnostics.ReportUndefinedVariable(expr.SourceFile, expr.Line, expr.Column, expr.Name);
            return NullValue.Instance;
        }
        return value;
    }

    private AlxValue EvaluateRange(RangeExpression expr)
    {
        var startVal = Evaluate(expr.Start);
        var endVal = Evaluate(expr.End);

        if (startVal is IntegerValue startInt && endVal is IntegerValue endInt)
            return new RangeValue(startInt.Value, endInt.Value);

        throw CreateTypeError(expr, "integer range", $"{startVal.TypeName}..{endVal.TypeName}");
    }

    private AlxValue EvaluateInterpolatedString(InterpolatedStringExpression expr)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var part in expr.Parts)
        {
            var value = Evaluate(part);
            sb.Append(FormatValue(value));
        }
        return new StringValue(sb.ToString());
    }

    private AlxValue EvaluateBinary(BinaryExpression expr)
    {
        if (expr.Operator == TokenType.And)
        {
            var left = Evaluate(expr.Left);
            if (!left.IsTruthy()) return left;
            return Evaluate(expr.Right);
        }

        if (expr.Operator == TokenType.Or)
        {
            var left = Evaluate(expr.Left);
            if (left.IsTruthy()) return left;
            return Evaluate(expr.Right);
        }

        var leftVal = Evaluate(expr.Left);
        var rightVal = Evaluate(expr.Right);

        return expr.Operator switch
        {
            TokenType.Plus => Add(leftVal, rightVal, expr),
            TokenType.Minus => Subtract(leftVal, rightVal, expr),
            TokenType.Star => Multiply(leftVal, rightVal, expr),
            TokenType.Slash => Divide(leftVal, rightVal, expr),
            TokenType.Percent => Modulo(leftVal, rightVal, expr),
            TokenType.Equal => BooleanValue.FromBool(ValuesEqual(leftVal, rightVal)),
            TokenType.NotEqual => BooleanValue.FromBool(!ValuesEqual(leftVal, rightVal)),
            TokenType.Greater => GreaterThan(leftVal, rightVal, expr),
            TokenType.GreaterEqual => GreaterThanOrEqual(leftVal, rightVal, expr),
            TokenType.Less => LessThan(leftVal, rightVal, expr),
            TokenType.LessEqual => LessThanOrEqual(leftVal, rightVal, expr),
            _ => throw new InvalidOperationException($"Unknown binary operator: {expr.Operator}")
        };
    }

    private AlxValue EvaluateUnary(UnaryExpression expr)
    {
        var operand = Evaluate(expr.Operand);
        return expr.Operator switch
        {
            TokenType.Minus => operand switch
            {
                IntegerValue intVal => new IntegerValue(-intVal.Value),
                FloatValue floatVal => new FloatValue(-floatVal.Value),
                _ => throw CreateTypeError(expr, "number", operand.TypeName)
            },
            TokenType.Not => BooleanValue.FromBool(!operand.IsTruthy()),
            _ => throw new InvalidOperationException($"Unknown unary operator: {expr.Operator}")
        };
    }

    private AlxValue EvaluateCall(CallExpression expr)
    {
        var callee = Evaluate(expr.Callee);

        if (callee is not FunctionValue func)
        {
            _diagnostics.ReportCannotCallNonFunction(
                expr.SourceFile, expr.Line, expr.Column,
                expr.Callee is IdentifierExpression id ? id.Name : callee.TypeName
            );
            return NullValue.Instance;
        }

        if (func is AlxBuiltinFunction builtin)
        {
            var args = expr.Arguments.Select(Evaluate).ToList();
            return builtin.Invoke(args, expr.SourceFile, expr.Line, expr.Column);
        }

        if (func is AlxFunction userFunc)
        {
            if (expr.Arguments.Count != userFunc.Declaration.Parameters.Count)
            {
                _diagnostics.ReportWrongArgumentCount(
                    expr.SourceFile, expr.Line, expr.Column,
                    userFunc.Declaration.Name, userFunc.Declaration.Parameters.Count, expr.Arguments.Count
                );
                return NullValue.Instance;
            }

            var funcEnv = new AlxEnvironment(userFunc.Closure);
            for (int i = 0; i < userFunc.Declaration.Parameters.Count; i++)
                funcEnv.Define(userFunc.Declaration.Parameters[i], Evaluate(expr.Arguments[i]));

            try
            {
                ExecuteBlock(userFunc.Declaration.Body, funcEnv);
                return NullValue.Instance;
            }
            catch (ReturnException ret)
            {
                return ret.Value;
            }
        }

        _diagnostics.ReportCannotCallNonFunction(expr.SourceFile, expr.Line, expr.Column, func.Name);
        return NullValue.Instance;
    }

    private AlxValue EvaluateAssignment(AssignmentExpression expr)
    {
        var value = Evaluate(expr.Value);
        if (!_environment.Set(expr.Name, value))
            _rootEnvironment.Define(expr.Name, value);
        return value;
    }

    // ===== ARITHMETIC OPERATIONS =====

    private AlxValue Add(AlxValue left, AlxValue right, BinaryExpression expr)
    {
        return (left, right) switch
        {
            (IntegerValue l, IntegerValue r) => new IntegerValue(l.Value + r.Value),
            (FloatValue l, FloatValue r) => new FloatValue(l.Value + r.Value),
            (IntegerValue l, FloatValue r) => new FloatValue(l.Value + r.Value),
            (FloatValue l, IntegerValue r) => new FloatValue(l.Value + r.Value),
            (StringValue l, StringValue r) => new StringValue(l.Value + r.Value),
            (StringValue l, _) => new StringValue(l.Value + FormatValue(right)),
            (_, StringValue r) => new StringValue(FormatValue(left) + r.Value),
            _ => throw CreateTypeError(expr, "number or string", $"{left.TypeName}, {right.TypeName}")
        };
    }

    private AlxValue Subtract(AlxValue left, AlxValue right, BinaryExpression expr)
    {
        return (left, right) switch
        {
            (IntegerValue l, IntegerValue r) => new IntegerValue(l.Value - r.Value),
            (FloatValue l, FloatValue r) => new FloatValue(l.Value - r.Value),
            (IntegerValue l, FloatValue r) => new FloatValue(l.Value - r.Value),
            (FloatValue l, IntegerValue r) => new FloatValue(l.Value - r.Value),
            _ => throw CreateTypeError(expr, "number", $"{left.TypeName}, {right.TypeName}")
        };
    }

    private AlxValue Multiply(AlxValue left, AlxValue right, BinaryExpression expr)
    {
        return (left, right) switch
        {
            (IntegerValue l, IntegerValue r) => new IntegerValue(l.Value * r.Value),
            (FloatValue l, FloatValue r) => new FloatValue(l.Value * r.Value),
            (IntegerValue l, FloatValue r) => new FloatValue(l.Value * r.Value),
            (FloatValue l, IntegerValue r) => new FloatValue(l.Value * r.Value),
            _ => throw CreateTypeError(expr, "number", $"{left.TypeName}, {right.TypeName}")
        };
    }

    private AlxValue Divide(AlxValue left, AlxValue right, BinaryExpression expr)
    {
        return (left, right) switch
        {
            (IntegerValue l, IntegerValue r) when r.Value == 0 => throw CreateDivisionByZero(expr),
            (IntegerValue l, IntegerValue r) => new IntegerValue(l.Value / r.Value),
            (FloatValue l, FloatValue r) when r.Value == 0.0 => throw CreateDivisionByZero(expr),
            (FloatValue l, FloatValue r) => new FloatValue(l.Value / r.Value),
            (IntegerValue l, FloatValue r) => new FloatValue(l.Value / r.Value),
            (FloatValue l, IntegerValue r) => new FloatValue(l.Value / r.Value),
            _ => throw CreateTypeError(expr, "number", $"{left.TypeName}, {right.TypeName}")
        };
    }

    private AlxValue Modulo(AlxValue left, AlxValue right, BinaryExpression expr)
    {
        return (left, right) switch
        {
            (IntegerValue l, IntegerValue r) when r.Value == 0 => throw CreateDivisionByZero(expr),
            (IntegerValue l, IntegerValue r) => new IntegerValue(l.Value % r.Value),
            (FloatValue l, FloatValue r) => new FloatValue(l.Value % r.Value),
            _ => throw CreateTypeError(expr, "number", $"{left.TypeName}, {right.TypeName}")
        };
    }

    private AlxValue GreaterThan(AlxValue left, AlxValue right, BinaryExpression expr)
    {
        return (left, right) switch
        {
            (IntegerValue l, IntegerValue r) => BooleanValue.FromBool(l.Value > r.Value),
            (FloatValue l, FloatValue r) => BooleanValue.FromBool(l.Value > r.Value),
            (IntegerValue l, FloatValue r) => BooleanValue.FromBool(l.Value > r.Value),
            (FloatValue l, IntegerValue r) => BooleanValue.FromBool(l.Value > r.Value),
            (StringValue l, StringValue r) => BooleanValue.FromBool(string.Compare(l.Value, r.Value, StringComparison.Ordinal) > 0),
            _ => throw CreateTypeError(expr, "comparable types", $"{left.TypeName}, {right.TypeName}")
        };
    }

    private AlxValue GreaterThanOrEqual(AlxValue left, AlxValue right, BinaryExpression expr)
    {
        return (left, right) switch
        {
            (IntegerValue l, IntegerValue r) => BooleanValue.FromBool(l.Value >= r.Value),
            (FloatValue l, FloatValue r) => BooleanValue.FromBool(l.Value >= r.Value),
            (IntegerValue l, FloatValue r) => BooleanValue.FromBool(l.Value >= r.Value),
            (FloatValue l, IntegerValue r) => BooleanValue.FromBool(l.Value >= r.Value),
            (StringValue l, StringValue r) => BooleanValue.FromBool(string.Compare(l.Value, r.Value, StringComparison.Ordinal) >= 0),
            _ => throw CreateTypeError(expr, "comparable types", $"{left.TypeName}, {right.TypeName}")
        };
    }

    private AlxValue LessThan(AlxValue left, AlxValue right, BinaryExpression expr)
    {
        return (left, right) switch
        {
            (IntegerValue l, IntegerValue r) => BooleanValue.FromBool(l.Value < r.Value),
            (FloatValue l, FloatValue r) => BooleanValue.FromBool(l.Value < r.Value),
            (IntegerValue l, FloatValue r) => BooleanValue.FromBool(l.Value < r.Value),
            (FloatValue l, IntegerValue r) => BooleanValue.FromBool(l.Value < r.Value),
            (StringValue l, StringValue r) => BooleanValue.FromBool(string.Compare(l.Value, r.Value, StringComparison.Ordinal) < 0),
            _ => throw CreateTypeError(expr, "comparable types", $"{left.TypeName}, {right.TypeName}")
        };
    }

    private AlxValue LessThanOrEqual(AlxValue left, AlxValue right, BinaryExpression expr)
    {
        return (left, right) switch
        {
            (IntegerValue l, IntegerValue r) => BooleanValue.FromBool(l.Value <= r.Value),
            (FloatValue l, FloatValue r) => BooleanValue.FromBool(l.Value <= r.Value),
            (IntegerValue l, FloatValue r) => BooleanValue.FromBool(l.Value <= r.Value),
            (FloatValue l, IntegerValue r) => BooleanValue.FromBool(l.Value <= r.Value),
            (StringValue l, StringValue r) => BooleanValue.FromBool(string.Compare(l.Value, r.Value, StringComparison.Ordinal) <= 0),
            _ => throw CreateTypeError(expr, "comparable types", $"{left.TypeName}, {right.TypeName}")
        };
    }

    private static bool ValuesEqual(AlxValue left, AlxValue right)
    {
        return (left, right) switch
        {
            (NullValue, NullValue) => true,
            (IntegerValue l, IntegerValue r) => l.Value == r.Value,
            (FloatValue l, FloatValue r) => l.Value == r.Value,
            (IntegerValue l, FloatValue r) => l.Value == r.Value,
            (FloatValue l, IntegerValue r) => l.Value == r.Value,
            (BooleanValue l, BooleanValue r) => l.Value == r.Value,
            (StringValue l, StringValue r) => l.Value == r.Value,
            _ => false
        };
    }

    private string FormatValue(AlxValue value)
    {
        return value switch
        {
            BooleanValue b => b.Value ? "true" : "false",
            NullValue => "null",
            _ => value.ToString() ?? "null"
        };
    }

    // ===== HELPERS =====

    private InvalidOperationException CreateTypeError(AstNode node, string expected, string actual)
    {
        _diagnostics.ReportTypeMismatch(node.SourceFile, node.Line, node.Column, expected, actual);
        return new InvalidOperationException($"Type mismatch: expected {expected}, got {actual}");
    }

    private InvalidOperationException CreateDivisionByZero(BinaryExpression expr)
    {
        _diagnostics.ReportTypeMismatch(expr.SourceFile, expr.Line, expr.Column, "non-zero number", "0");
        return new InvalidOperationException("Division by zero");
    }
}

// ===== FUNCTION TYPES =====

public class AlxFunction : FunctionValue
{
    public FunctionDeclaration Declaration { get; }
    public AlxEnvironment Closure { get; }
    public AlxFunction(FunctionDeclaration declaration, AlxEnvironment closure) : base(declaration.Name)
    {
        Declaration = declaration;
        Closure = closure;
    }
}

public abstract class AlxBuiltinFunction : FunctionValue
{
    protected AlxBuiltinFunction(string name) : base(name) { }
    public abstract AlxValue Invoke(List<AlxValue> args, string sourceFile, int line, int column);
}

public class ReturnException : Exception
{
    public AlxValue Value { get; }
    public ReturnException(AlxValue value) { Value = value; }
}
