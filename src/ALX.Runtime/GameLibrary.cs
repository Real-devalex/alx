using ALX.Compiler.Diagnostics;
using ALX.Runtime.Values;

namespace ALX.Runtime;

/// <summary>
/// ALX 0.7.0, the game runtime foundation: math, random numbers, time, and a small
/// vector and entity library written in ALX itself.
/// </summary>
public static class GameLibrary
{
    private static readonly Random Rng = new();

    /// <summary>Registers the math, random and time functions in the given environment.</summary>
    public static void Register(AlxEnvironment env)
    {
        env.Define("PI", new FloatValue(Math.PI), true);
        env.Define("TAU", new FloatValue(Math.PI * 2), true);

        // ----- math -----
        Fn(env, "sqrt", 1, a => new FloatValue(Math.Sqrt(Num("sqrt", a[0]))));
        Fn(env, "pow", 2, a => new FloatValue(Math.Pow(Num("pow", a[0]), Num("pow", a[1]))));
        Fn(env, "sin", 1, a => new FloatValue(Math.Sin(Num("sin", a[0]))));
        Fn(env, "cos", 1, a => new FloatValue(Math.Cos(Num("cos", a[0]))));
        Fn(env, "tan", 1, a => new FloatValue(Math.Tan(Num("tan", a[0]))));
        Fn(env, "atan2", 2, a => new FloatValue(Math.Atan2(Num("atan2", a[0]), Num("atan2", a[1]))));
        Fn(env, "toRadians", 1, a => new FloatValue(Num("toRadians", a[0]) * Math.PI / 180.0));
        Fn(env, "toDegrees", 1, a => new FloatValue(Num("toDegrees", a[0]) * 180.0 / Math.PI));
        Fn(env, "floor", 1, a => new IntegerValue((long)Math.Floor(Num("floor", a[0]))));
        Fn(env, "ceil", 1, a => new IntegerValue((long)Math.Ceiling(Num("ceil", a[0]))));
        Fn(env, "round", 1, a => new IntegerValue((long)Math.Round(Num("round", a[0]), MidpointRounding.AwayFromZero)));

        Fn(env, "abs", 1, a => a[0] is IntegerValue i ? new IntegerValue(Math.Abs(i.Value)) : new FloatValue(Math.Abs(Num("abs", a[0]))));
        Fn(env, "sign", 1, a => new IntegerValue(Math.Sign(Num("sign", a[0]))));
        Fn(env, "min", 2, a => Pick(a[0], a[1], Math.Min(Num("min", a[0]), Num("min", a[1]))));
        Fn(env, "max", 2, a => Pick(a[0], a[1], Math.Max(Num("max", a[0]), Num("max", a[1]))));
        Fn(env, "clamp", 3, a =>
        {
            double v = Num("clamp", a[0]), lo = Num("clamp", a[1]), hi = Num("clamp", a[2]);
            double r = Math.Min(Math.Max(v, lo), hi);
            return a[0] is IntegerValue && a[1] is IntegerValue && a[2] is IntegerValue ? new IntegerValue((long)r) : new FloatValue(r);
        });
        Fn(env, "lerp", 3, a =>
        {
            double from = Num("lerp", a[0]), to = Num("lerp", a[1]), t = Num("lerp", a[2]);
            return new FloatValue(from + (to - from) * t);
        });

        // ----- random -----
        Fn(env, "random", 0, a => new FloatValue(Rng.NextDouble()));
        Fn(env, "randomInt", 2, a =>
        {
            long lo = (long)Num("randomInt", a[0]), hi = (long)Num("randomInt", a[1]);
            if (hi < lo) (lo, hi) = (hi, lo);
            return new IntegerValue(lo + (long)Math.Floor(Rng.NextDouble() * (hi - lo + 1)));
        });

        // ----- time and type -----
        Fn(env, "time", 0, a => new FloatValue(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0));
        Fn(env, "sleep", 1, a =>
        {
            int ms = (int)Math.Max(0, Math.Min(Num("sleep", a[0]), 10000));
            Thread.Sleep(ms);
            return NullValue.Instance;
        });
        Fn(env, "type", 1, a => new StringValue(a[0].TypeName));
    }

    private static void Fn(AlxEnvironment env, string name, int arity, Func<List<AlxValue>, AlxValue> body)
    {
        env.Define(name, new LambdaBuiltinFunction(name, args =>
        {
            if (args.Count != arity)
                throw new InvalidOperationException($"{name}() takes {arity} argument(s) but got {args.Count}");
            return body(args);
        }));
    }

    private static double Num(string fn, AlxValue v)
    {
        if (v is IntegerValue i) return i.Value;
        if (v is FloatValue f) return f.Value;
        throw new InvalidOperationException($"{fn}() needs a number but got a {v.TypeName}");
    }

    /// <summary>Keeps integers as integers when both inputs were integers.</summary>
    private static AlxValue Pick(AlxValue x, AlxValue y, double result)
        => x is IntegerValue && y is IntegerValue ? new IntegerValue((long)result) : new FloatValue(result);

    /// <summary>The Vec2 and Entity classes, written in ALX and loaded before every program.</summary>
    public const string Prelude = """
class Vec2 {
    function constructor(x, y) {
        this.x = x
        this.y = y
    }
    function add(o) { return new Vec2(this.x + o.x, this.y + o.y) }
    function sub(o) { return new Vec2(this.x - o.x, this.y - o.y) }
    function scale(k) { return new Vec2(this.x * k, this.y * k) }
    function dot(o) { return this.x * o.x + this.y * o.y }
    function length() { return sqrt(this.x * this.x + this.y * this.y) }
    function distanceTo(o) { return this.sub(o).length() }
    function normalized() {
        len = this.length()
        if len == 0 { return new Vec2(0, 0) }
        return new Vec2(this.x / len, this.y / len)
    }
    function lerp(o, t) { return new Vec2(this.x + (o.x - this.x) * t, this.y + (o.y - this.y) * t) }
    function toString() { return "({this.x}, {this.y})" }
}

class Entity {
    function constructor(name, x, y) {
        this.name = name
        this.position = new Vec2(x, y)
        this.velocity = new Vec2(0, 0)
        this.alive = true
    }
    function update(dt) { this.position = this.position.add(this.velocity.scale(dt)) }
    function moveTo(x, y) { this.position = new Vec2(x, y) }
    function distanceTo(other) { return this.position.distanceTo(other.position) }
}
""";

    /// <summary>Loads the ALX-written part of the library. Needs the parser, so it lives here and is called by the interpreter.</summary>
    public static ALX.Compiler.AST.ProgramNode? ParsePrelude()
    {
        var diagnostics = new DiagnosticBag();
        var tokens = new ALX.Compiler.Lexer.Lexer(Prelude, "prelude.alx", diagnostics).Tokenize();
        var program = new ALX.Compiler.Parser.Parser(tokens, "prelude.alx", diagnostics).Parse();
        return diagnostics.HasErrors ? null : program;
    }
}
