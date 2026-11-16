using ALX.Compiler.Diagnostics;
using ALX.Runtime;
using Xunit;

namespace ALX.Integration.Tests;

/// <summary>ALX 0.7.0: the game runtime foundation (math, random, vectors, entities).</summary>
public class GameLibraryTests
{
    [Fact] public void Sqrt() => Assert.Equal("4", Run("print(sqrt(16))"));
    [Fact] public void Pow() => Assert.Equal("8", Run("print(pow(2, 3))"));
    [Fact] public void AbsInteger() => Assert.Equal("5", Run("print(abs(-5))"));
    [Fact] public void AbsFloat() => Assert.Equal("2.5", Run("print(abs(-2.5))"));
    [Fact] public void MinMaxKeepIntegers() => Assert.Equal("3\n7", Run("print(min(3, 7))\nprint(max(3, 7))"));
    [Fact] public void Floor() => Assert.Equal("3", Run("print(floor(3.7))"));
    [Fact] public void Ceil() => Assert.Equal("4", Run("print(ceil(3.2))"));
    [Fact] public void Round() => Assert.Equal("4", Run("print(round(3.5))"));
    [Fact] public void ClampHigh() => Assert.Equal("10", Run("print(clamp(15, 0, 10))"));
    [Fact] public void ClampLow() => Assert.Equal("0", Run("print(clamp(-4, 0, 10))"));
    [Fact] public void Lerp() => Assert.Equal("5", Run("print(lerp(0, 10, 0.5))"));
    [Fact] public void Sign() => Assert.Equal("-1", Run("print(sign(-9))"));
    [Fact] public void PiIsDefined() => Assert.StartsWith("3.14159", Run("print(PI)"));
    [Fact] public void RandomIsBetweenZeroAndOne() => Assert.Equal("true", Run("r = random()\nprint(r >= 0 and r < 1)"));
    [Fact] public void RandomIntStaysInRange() => Assert.Equal("true", Run("n = randomInt(3, 6)\nprint(n >= 3 and n <= 6)"));
    [Fact] public void RandomIntSingleValue() => Assert.Equal("1", Run("print(randomInt(1, 1))"));
    [Fact] public void TimeIsPositive() => Assert.Equal("true", Run("print(time() > 0)"));
    [Fact] public void TypeOfValues() => Assert.Equal("integer\nstring", Run("print(type(1))\nprint(type(\"a\"))"));

    [Fact]
    public void Vec2Length() => Assert.Equal("5", Run("v = new Vec2(3, 4)\nprint(v.length())"));

    [Fact]
    public void Vec2Add() => Assert.Equal("(4, 6)", Run("a = new Vec2(1, 2)\nb = new Vec2(3, 4)\nprint(a.add(b).toString())"));

    [Fact]
    public void Vec2Scale() => Assert.Equal("(6, 8)", Run("v = new Vec2(3, 4)\nprint(v.scale(2).toString())"));

    [Fact]
    public void Vec2Dot() => Assert.Equal("11", Run("a = new Vec2(1, 2)\nb = new Vec2(3, 4)\nprint(a.dot(b))"));

    [Fact]
    public void EntityMovesWithItsVelocity()
    {
        var output = Run("e = new Entity(\"Hero\", 0, 0)\ne.velocity = new Vec2(10, 5)\ne.update(2)\nprint(e.position.toString())");
        Assert.Equal("(20, 10)", output);
    }

    [Fact]
    public void WrongArgumentCountIsAnError()
    {
        Assert.ThrowsAny<Exception>(() => Run("print(sqrt(1, 2))"));
    }

    private static string Run(string source)
    {
        var diagnostics = new DiagnosticBag();
        var output = new List<string>();
        var tokens = new ALX.Compiler.Lexer.Lexer(source, "test.alx", diagnostics).Tokenize();
        var ast = new ALX.Compiler.Parser.Parser(tokens, "test.alx", diagnostics).Parse();
        var interpreter = new Interpreter(diagnostics, line => output.Add(line));
        interpreter.Execute(ast);
        if (diagnostics.HasErrors)
            throw new InvalidOperationException("Diagnostics reported:\n" + string.Join("\n", diagnostics.Diagnostics.Select(d => d.ToString())));
        return string.Join("\n", output);
    }
}
