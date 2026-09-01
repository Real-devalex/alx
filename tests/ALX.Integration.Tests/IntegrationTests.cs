using ALX.Compiler.Diagnostics;
using ALX.Compiler.Lexer;
using ALX.Compiler.Parser;
using ALX.Runtime;
using Xunit;

namespace ALX.Integration.Tests;

public class IntegrationTests
{
    // ===== BASIC FEATURES (0.1.0) =====

    [Fact] public void HelloWorld() => Assert.Equal("Hello, ALEXION!", Run("print(\"Hello, ALEXION!\")"));

    [Fact] public void PrintInteger() => Assert.Equal("42", Run("print(42)"));

    [Fact] public void PrintFloat() => Assert.Equal("3.14", Run("print(3.14)"));

    [Fact] public void PrintBoolean() => Assert.Equal("true", Run("print(true)"));

    [Fact] public void PrintNull() => Assert.Equal("null", Run("print(null)"));

    [Fact]
    public void VariableAssignment()
    {
        var output = Run("name = \"ALEXION STUDIOS\"\nversion = 1\nprint(name)\nprint(version)");
        Assert.Equal("ALEXION STUDIOS\n1", output);
    }

    [Fact] public void Arithmetic() => Assert.Equal("30", Run("a = 10\nb = 20\nprint(a + b)"));

    [Fact] public void OperatorPrecedence() => Assert.Equal("20", Run("print(10 + 5 * 2)"));

    [Fact] public void VariableReassignment()
    {
        var output = Run("health = 100\ndamage = 25\nhealth = health - damage\nprint(health)");
        Assert.Equal("75", output);
    }

    [Fact]
    public void StringConcatenation()
    {
        var output = Run("greeting = \"Hello\" + \" \" + \"World\"\nprint(greeting)");
        Assert.Equal("Hello World", output);
    }

    [Fact]
    public void IfCondition()
    {
        var output = Run("health = 100\nif health > 50 {\n  print(\"Healthy\")\n} else {\n  print(\"Weak\")\n}");
        Assert.Equal("Healthy", output);
    }

    [Fact]
    public void IfElseIf()
    {
        var source = @"score = 85
if score >= 90 {
  print(""A"")
} else if score >= 80 {
  print(""B"")
} else {
  print(""C"")
}";
        Assert.Equal("B", Run(source));
    }

    [Fact]
    public void WhileLoop() => Assert.Equal("0", Run("x = 5\nwhile x > 0 {\n  x = x - 1\n}\nprint(x)"));

    [Fact]
    public void FunctionDefinitionAndCall()
    {
        var source = @"function greet(name) {
  print(""Hello "" + name)
}
greet(""World"")";
        Assert.Equal("Hello World", Run(source));
    }

    [Fact]
    public void FunctionReturnValue()
    {
        var source = @"function add(a, b) {
  return a + b
}
result = add(10, 20)
print(result)";
        Assert.Equal("30", Run(source));
    }

    [Fact]
    public void RecursiveFunction()
    {
        var source = @"function factorial(n) {
  if n <= 1 {
    return 1
  }
  return n * factorial(n - 1)
}
print(factorial(5))";
        Assert.Equal("120", Run(source));
    }

    [Fact] public void NestedExpressions() => Assert.Equal("30", Run("print((10 + 5) * 2)"));

    [Fact]
    public void BooleanLogic()
    {
        var output = Run("print(true and false)\nprint(true or false)\nprint(not false)");
        Assert.Equal("false\ntrue\ntrue", output);
    }

    [Fact]
    public void ComparisonOperators()
    {
        var output = Run("print(10 > 5)\nprint(10 < 5)\nprint(10 >= 10)\nprint(10 <= 5)\nprint(10 == 10)\nprint(10 != 5)");
        Assert.Equal("true\nfalse\ntrue\nfalse\ntrue\ntrue", output);
    }

    [Fact]
    public void UndefinedVariable_ReportsDiagnostic()
    {
        var diagnostics = new DiagnosticBag();
        RunWithDiagnostics("print(undefinedVar)", diagnostics);
        Assert.True(diagnostics.HasErrors);
        Assert.Contains(diagnostics.Diagnostics, d => d.Code == "ALX3001");
    }

    // ===== NEW FEATURES (0.2.0) =====

    [Fact]
    public void ForLoopBasic()
    {
        var source = @"for i in 1..5 {
  print(i)
}";
        var output = Run(source);
        Assert.Equal("1\n2\n3\n4", output);
    }

    [Fact]
    public void ForLoopBreak()
    {
        var source = @"for i in 1..10 {
  if i == 5 {
    break
  }
  print(i)
}";
        var output = Run(source);
        Assert.Equal("1\n2\n3\n4", output);
    }

    [Fact]
    public void ForLoopContinue()
    {
        var source = @"for i in 1..6 {
  if i % 2 == 0 {
    continue
  }
  print(i)
}";
        var output = Run(source);
        Assert.Equal("1\n3\n5", output);
    }

    [Fact]
    public void ForLoopSum()
    {
        var source = @"total = 0
for i in 1..10 {
  total = total + i
}
print(total)";
        var output = Run(source);
        Assert.Equal("45", output);
    }

    [Fact]
    public void ForLoopNested()
    {
        var source = @"result = 0
for i in 1..4 {
  for j in 1..4 {
    result = result + 1
  }
}
print(result)";
        var output = Run(source);
        Assert.Equal("9", output);
    }

    [Fact]
    public void ForLoopWithFunction()
    {
        var source = @"function square(n) {
  return n * n
}
for i in 1..5 {
  print(square(i))
}";
        var output = Run(source);
        Assert.Equal("1\n4\n9\n16", output);
    }

    [Fact]
    public void StringInterpolationBasic()
    {
        var source = @"name = ""Daniel""
print(""Hello, {name}!"")";
        var output = Run(source);
        Assert.Equal("Hello, Daniel!", output);
    }

    [Fact]
    public void StringInterpolationMath()
    {
        var source = @"x = 10
y = 5
print(""Sum: {x + y}"")";
        var output = Run(source);
        Assert.Equal("Sum: 15", output);
    }

    [Fact]
    public void StringInterpolationMultipleExpressions()
    {
        var source = @"name = ""ALX""
version = 2
print(""Welcome to {name} {version}!"")";
        var output = Run(source);
        Assert.Equal("Welcome to ALX 2!", output);
    }

    [Fact]
    public void StringInterpolationFunction()
    {
        var source = @"function double(n) {
  return n * 2
}
print(""Double of 7 is {double(7)}"")";
        var output = Run(source);
        Assert.Equal("Double of 7 is 14", output);
    }

    [Fact]
    public void StringInterpolationBoolean()
    {
        var source = @"alive = true
print(""Status: {alive}"")";
        var output = Run(source);
        Assert.Equal("Status: true", output);
    }

    [Fact]
    public void RangeInVariable()
    {
        var source = @"r = 1..5
print(r)";
        // RangeValue.ToString() returns "1..5"
        var output = Run(source);
        Assert.Equal("1..5", output);
    }

    [Fact]
    public void WhileLoopBreak()
    {
        var source = @"x = 0
while true {
  x = x + 1
  if x == 5 {
    break
  }
}
print(x)";
        var output = Run(source);
        Assert.Equal("5", output);
    }

    [Fact]
    public void WhileLoopContinue()
    {
        var source = @"x = 0
while x < 10 {
  x = x + 1
  if x % 2 == 0 {
    continue
  }
  print(x)
}";
        var output = Run(source);
        Assert.Equal("1\n3\n5\n7\n9", output);
    }

    // ===== NEW FEATURES (0.3.0) — Closures & Lambdas =====

    [Fact]
    public void LambdaBasic()
    {
        var source = @"square = lambda(x) { x * x }
print(square(7))";
        Assert.Equal("49", Run(source));
    }

    [Fact]
    public void LambdaTwoParams()
    {
        var source = @"add = lambda(a, b) { a + b }
print(add(10, 20))";
        Assert.Equal("30", Run(source));
    }

    [Fact]
    public void LambdaImplicitReturn()
    {
        var source = @"f = lambda(x) { x + 1 }
print(f(99))";
        Assert.Equal("100", Run(source));
    }

    [Fact]
    public void LambdaWithExplicitReturn()
    {
        var source = @"f = lambda(x) {
  if x > 10 {
    return 100
  }
  return x
}
print(f(5))
print(f(15))";
        Assert.Equal("5\n100", Run(source));
    }

    [Fact]
    public void ClosureCapturesOuterScope()
    {
        var source = @"function makeCounter() {
  count = 0
  function increment() {
    count = count + 1
    return count
  }
  return increment
}
counter = makeCounter()
print(counter())
print(counter())
print(counter())";
        Assert.Equal("1\n2\n3", Run(source));
    }

    [Fact]
    public void ClosureWithLambda()
    {
        var source = @"function makeAdder(n) {
  return lambda(x) { x + n }
}
addTen = makeAdder(10)
print(addTen(5))
print(addTen(20))";
        Assert.Equal("15\n30", Run(source));
    }

    [Fact]
    public void HigherOrderFunction()
    {
        var source = @"function applyTwice(func, value) {
  return func(func(value))
}
double = lambda(x) { x * 2 }
print(applyTwice(double, 5))";
        Assert.Equal("20", Run(source));
    }

    [Fact]
    public void FunctionComposition()
    {
        var source = @"function compose(f, g) {
  return lambda(x) { f(g(x)) }
}
doubleIt = lambda(x) { x * 2 }
addOne = lambda(x) { x + 1 }
doubleThenAddOne = compose(addOne, doubleIt)
print(doubleThenAddOne(5))";
        Assert.Equal("11", Run(source));
    }

    [Fact]
    public void LambdaAsFunctionArgument()
    {
        var source = @"function transform(value, func) {
  return func(value)
}
print(transform(5, lambda(x) { x * x }))
print(transform(10, lambda(x) { x + 100 }))";
        Assert.Equal("25\n110", Run(source));
    }

    [Fact]
    public void AccumulatorClosure()
    {
        var source = @"function makeAccumulator(start) {
  total = start
  return lambda(amount) {
    total = total + amount
    return total
  }
}
acc = makeAccumulator(0)
print(acc(10))
print(acc(20))
print(acc(5))";
        Assert.Equal("10\n30\n35", Run(source));
    }

    // ===== HELPERS =====

    private static string Run(string source)
    {
        var diagnostics = new DiagnosticBag();
        var output = new List<string>();

        var lexer = new ALX.Compiler.Lexer.Lexer(source, "test.alx", diagnostics);
        var tokens = lexer.Tokenize();
        var parser = new ALX.Compiler.Parser.Parser(tokens, "test.alx", diagnostics);
        var ast = parser.Parse();
        var interpreter = new Interpreter(diagnostics, line => output.Add(line));
        interpreter.Execute(ast);

        if (diagnostics.HasErrors)
        {
            var errors = string.Join("\n", diagnostics.Diagnostics.Select(d => d.ToString()));
            throw new InvalidOperationException($"Diagnostics reported:\n{errors}");
        }

        return string.Join("\n", output);
    }

    private static void RunWithDiagnostics(string source, DiagnosticBag diagnostics)
    {
        var lexer = new ALX.Compiler.Lexer.Lexer(source, "test.alx", diagnostics);
        var tokens = lexer.Tokenize();
        if (diagnostics.HasErrors) return;

        var parser = new ALX.Compiler.Parser.Parser(tokens, "test.alx", diagnostics);
        var ast = parser.Parse();
        if (diagnostics.HasErrors) return;

        var interpreter = new Interpreter(diagnostics, _ => { });
        try { interpreter.Execute(ast); }
        catch { /* Ignore runtime exceptions for diagnostic tests */ }
    }
}
