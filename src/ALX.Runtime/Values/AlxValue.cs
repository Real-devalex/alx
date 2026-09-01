namespace ALX.Runtime.Values;

public abstract class AlxValue
{
    public abstract string TypeName { get; }
    public virtual bool IsTruthy() => true;
    public override string ToString() => $"[{TypeName}]";
}

public class IntegerValue : AlxValue
{
    public override string TypeName => "integer";
    public long Value { get; }
    public IntegerValue(long value) { Value = value; }
    public override bool IsTruthy() => Value != 0;
    public override string ToString() => Value.ToString();
}

public class FloatValue : AlxValue
{
    public override string TypeName => "float";
    public double Value { get; }
    public FloatValue(double value) { Value = value; }
    public override bool IsTruthy() => Value != 0.0;
    public override string ToString() => Value.ToString("G");
}

public class StringValue : AlxValue
{
    public override string TypeName => "string";
    public string Value { get; }
    public StringValue(string value) { Value = value; }
    public override bool IsTruthy() => Value.Length > 0;
    public override string ToString() => Value;
}

public class BooleanValue : AlxValue
{
    public override string TypeName => "boolean";
    public bool Value { get; }
    public static readonly BooleanValue True = new(true);
    public static readonly BooleanValue False = new(false);
    public BooleanValue(bool value) { Value = value; }
    public override bool IsTruthy() => Value;
    public override string ToString() => Value ? "true" : "false";
    public static BooleanValue FromBool(bool value) => value ? True : False;
}

public class NullValue : AlxValue
{
    public override string TypeName => "null";
    public static readonly NullValue Instance = new();
    private NullValue() { }
    public override bool IsTruthy() => false;
    public override string ToString() => "null";
}

public class FunctionValue : AlxValue
{
    public override string TypeName => "function";
    public string Name { get; }
    public FunctionValue(string name) { Name = name; }
    public override string ToString() => $"<function {Name}>";
}

/// <summary>
/// ALX range value: represents start..end
/// </summary>
public class RangeValue : AlxValue
{
    public override string TypeName => "range";
    public long Start { get; }
    public long End { get; }
    public RangeValue(long start, long end) { Start = start; End = end; }
    public override string ToString() => $"{Start}..{End}";
}

public class ReturnWrapper : AlxValue
{
    public AlxValue Value { get; }
    public ReturnWrapper(AlxValue value) { Value = value; }
    public override string TypeName => "return";
    public override string ToString() => Value.ToString();
}

// ===== CONTROL FLOW EXCEPTIONS =====

/// <summary>
/// Thrown by break statement to exit a loop.
/// </summary>
public class BreakException : Exception
{
    public BreakException() : base("break") { }
}

/// <summary>
/// Thrown by continue statement to skip to next loop iteration.
/// </summary>
public class ContinueException : Exception
{
    public ContinueException() : base("continue") { }
}
