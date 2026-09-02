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
        RegisterBuiltins();
    }

    private void RegisterBuiltins()
    {
        // input([prompt]) — read user input from stdin
        _rootEnvironment.Define("input", new InputFunction());
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
            case ClassDeclaration classDecl:
                ExecuteClassDeclaration(classDecl);
                break;
            case ImportStatement importDecl:
                ExecuteImport(importDecl);
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

    private void ExecuteClassDeclaration(ClassDeclaration classDecl)
    {
        // Resolve superclass if present
        AlxClassValue? superclass = null;
        if (classDecl.SuperclassName != null)
        {
            var superVal = _environment.Get(classDecl.SuperclassName);
            if (superVal is not AlxClassValue sc)
            {
                _diagnostics.ReportTypeMismatch(classDecl.SourceFile, classDecl.Line, classDecl.Column,
                    "class", superVal?.TypeName ?? "undefined");
                return;
            }
            superclass = sc;
        }

        // Collect methods
        var methods = new Dictionary<string, AlxValue>();
        foreach (var method in classDecl.Methods)
        {
            var func = new AlxFunction(method, _environment);
            methods[method.Name] = func;
        }

        // Create class value
        var classValue = new AlxClassValue(classDecl.Name, superclass, methods, classDecl.Constructor != null ? new AlxFunction(classDecl.Constructor, _environment) : null);
        _environment.Define(classDecl.Name, classValue);
    }

    private void ExecuteImport(ImportStatement importDecl)
    {
        // Look for the file: module_name.alx in the same directory or a modules/ directory
        string sourceDir = Path.GetDirectoryName(importDecl.SourceFile) ?? ".";
        string[] searchPaths = {
            Path.Combine(sourceDir, importDecl.ModuleName + ".alx"),
            Path.Combine(sourceDir, "modules", importDecl.ModuleName + ".alx"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modules", importDecl.ModuleName + ".alx"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ALX", "modules", importDecl.ModuleName + ".alx"),
        };

        string? filePath = null;
        foreach (var path in searchPaths)
        {
            if (File.Exists(path)) { filePath = path; break; }
        }

        if (filePath == null)
        {
            _diagnostics.ReportTypeMismatch(importDecl.SourceFile, importDecl.Line, importDecl.Column,
                $"module file '{importDecl.ModuleName}.alx'", "file not found");
            return;
        }

        // Parse and execute the module
        string source = File.ReadAllText(filePath);
        string moduleName = importDecl.Alias ?? importDecl.ModuleName;

        var moduleDiag = new DiagnosticBag();
        var moduleLexer = new ALX.Compiler.Lexer.Lexer(source, Path.GetFileName(filePath), moduleDiag);
        var moduleTokens = moduleLexer.Tokenize();
        var moduleParser = new ALX.Compiler.Parser.Parser(moduleTokens, Path.GetFileName(filePath), moduleDiag);
        var moduleAst = moduleParser.Parse();

        if (moduleDiag.HasErrors)
        {
            foreach (var d in moduleDiag.Diagnostics)
                _diagnostics.Add(d);
            return;
        }

        // Create a new environment for the module, then capture its exports
        var moduleEnv = new AlxEnvironment(_rootEnvironment);
        var previous = _environment;
        _environment = moduleEnv;

        try
        {
            foreach (var stmt in moduleAst.Statements)
                ExecuteStatement(stmt);
        }
        finally
        {
            _environment = previous;
        }

        // Create a map with all module-level variables
        var moduleMap = new Dictionary<string, AlxValue>();
        var current = moduleEnv;
        while (current != null)
        {
            foreach (var kvp in current.GetAll())
                moduleMap[kvp.Key] = kvp.Value;
            current = current.Parent;
        }

        _environment.Define(moduleName, new MapValue(moduleMap));
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
            LambdaExpression lambdaExpr => EvaluateLambda(lambdaExpr),
            RangeExpression rangeExpr => EvaluateRange(rangeExpr),
            InterpolatedStringExpression interpExpr => EvaluateInterpolatedString(interpExpr),
            ArrayExpression arrayExpr => EvaluateArray(arrayExpr),
            MapExpression mapExpr => EvaluateMap(mapExpr),
            IndexExpression indexExpr => EvaluateIndex(indexExpr),
            MemberExpression memberExpr => EvaluateMember(memberExpr),
            MemberAssignmentExpression memberAssign => EvaluateMemberAssignment(memberAssign),
            IndexAssignmentExpression indexAssign => EvaluateIndexAssignment(indexAssign),
            NewExpression newExpr => EvaluateNew(newExpr),
            ThisExpression => EvaluateThis(),
            SuperExpression superExpr => EvaluateSuper(superExpr),
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

    private AlxValue EvaluateLambda(LambdaExpression expr)
    {
        // Create an anonymous function that captures the current environment
        var anonName = $"<lambda@{expr.Line}:{expr.Column}>";
        var decl = new FunctionDeclaration(anonName, expr.Parameters, expr.Body, expr.Line, expr.Column, expr.SourceFile);
        return new AlxFunction(decl, _environment);
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

        if (func is AlxBoundMethod boundMethod)
        {
            var method = boundMethod.Method;
            if (expr.Arguments.Count != method.Declaration.Parameters.Count)
            {
                _diagnostics.ReportWrongArgumentCount(
                    expr.SourceFile, expr.Line, expr.Column,
                    method.Declaration.Name, method.Declaration.Parameters.Count, expr.Arguments.Count
                );
                return NullValue.Instance;
            }

            var funcEnv = new AlxEnvironment(method.Closure);
            funcEnv.Define("this", boundMethod.Instance);
            for (int i = 0; i < method.Declaration.Parameters.Count; i++)
                funcEnv.Define(method.Declaration.Parameters[i], Evaluate(expr.Arguments[i]));

            try
            {
                ExecuteBlock(method.Declaration.Body, funcEnv);
                return NullValue.Instance;
            }
            catch (ReturnException ret)
            {
                return ret.Value;
            }
        }

        if (func is AlxBuiltinFunction builtin)
        {
            var args = expr.Arguments.Select(Evaluate).ToList();
            return builtin.Invoke(args, expr.SourceFile, expr.Line, expr.Column);
        }

        if (func is LambdaBuiltinFunction lambdaBuiltin)
        {
            var args = expr.Arguments.Select(Evaluate).ToList();
            return lambdaBuiltin.Invoke(args);
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
                // Lambda implicit return: evaluate last expression in lambda context
                // Lambda implicit return: evaluate last expression in lambda context
                if (userFunc.Declaration.Name.StartsWith("<lambda"))
                {
                    var lastStmt = userFunc.Declaration.Body.Statements.LastOrDefault();
                    if (lastStmt is ExpressionStatement exprStmt)
                    {
                        var previous = _environment;
                        _environment = funcEnv;
                        try
                        {
                            for (int i = 0; i < userFunc.Declaration.Body.Statements.Count - 1; i++)
                                ExecuteStatement(userFunc.Declaration.Body.Statements[i]);
                            return Evaluate(exprStmt.Expression);
                        }
                        finally
                        {
                            _environment = previous;
                        }
                    }
                }

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

    // ===== 0.4.0: ARRAYS & MAPS =====

    private AlxValue EvaluateArray(ArrayExpression expr)
    {
        var elements = expr.Elements.Select(Evaluate).ToList();
        return new ArrayValue(elements);
    }

    private AlxValue EvaluateMap(MapExpression expr)
    {
        var entries = new Dictionary<string, AlxValue>();
        foreach (var (keyExpr, valueExpr) in expr.Entries)
        {
            var key = Evaluate(keyExpr);
            if (key is not StringValue keyStr)
                throw CreateTypeError(expr, "string key", key.TypeName);
            entries[keyStr.Value] = Evaluate(valueExpr);
        }
        return new MapValue(entries);
    }

    private AlxValue EvaluateIndex(IndexExpression expr)
    {
        var obj = Evaluate(expr.Object);
        var index = Evaluate(expr.Index);

        if (obj is ArrayValue arr)
        {
            if (index is IntegerValue intIdx)
            {
                if (intIdx.Value < 0 || intIdx.Value >= arr.Elements.Count)
                {
                    _diagnostics.ReportTypeMismatch(expr.SourceFile, expr.Line, expr.Column,
                        $"valid index (0..{arr.Elements.Count - 1})", intIdx.Value.ToString());
                    return NullValue.Instance;
                }
                return arr.Elements[(int)intIdx.Value];
            }
            throw CreateTypeError(expr, "integer index", index.TypeName);
        }

        if (obj is MapValue map)
        {
            if (index is StringValue keyStr)
            {
                if (map.Entries.TryGetValue(keyStr.Value, out var val))
                    return val;
                return NullValue.Instance;
            }
            throw CreateTypeError(expr, "string key", index.TypeName);
        }

        throw CreateTypeError(expr, "array or map", obj.TypeName);
    }

    private AlxValue EvaluateMember(MemberExpression expr)
    {
        var obj = Evaluate(expr.Object);

        // Handle instance property and method access
        if (obj is AlxInstanceValue instance)
        {
            // Check instance properties first
            if (instance.Properties.TryGetValue(expr.MemberName, out var propVal))
                return propVal;

            // Then check class methods
            if (instance.Class.Methods.TryGetValue(expr.MemberName, out var method) && method is AlxFunction func)
                return new AlxBoundMethod(func, instance);

            // Check superclass methods
            var super = instance.Class.Superclass;
            while (super != null)
            {
                if (super.Methods.TryGetValue(expr.MemberName, out var superMethod) && superMethod is AlxFunction superFunc)
                    return new AlxBoundMethod(superFunc, instance);
                super = super.Superclass;
            }

            _diagnostics.ReportTypeMismatch(expr.SourceFile, expr.Line, expr.Column,
                $"property or method '{expr.MemberName}'", $"not found on {instance.Class.Name}");
            return NullValue.Instance;
        }

        // Handle class static access (e.g., ClassName.someMethod)
        if (obj is AlxClassValue classVal)
        {
            if (classVal.Methods.TryGetValue(expr.MemberName, out var classMethod))
                return classMethod;
        }

        // Built-in methods for arrays
        if (obj is ArrayValue arr)
        {
            return expr.MemberName switch
            {
                "length" => new IntegerValue(arr.Elements.Count),
                "push" => CreateBuiltinMethod("push", args =>
                {
                    foreach (var arg in args) arr.Elements.Add(arg);
                    return new IntegerValue(arr.Elements.Count);
                }),
                "pop" => CreateBuiltinMethod("pop", args =>
                {
                    if (arr.Elements.Count == 0) return NullValue.Instance;
                    var last = arr.Elements[arr.Elements.Count - 1];
                    arr.Elements.RemoveAt(arr.Elements.Count - 1);
                    return last;
                }),
                "first" => arr.Elements.Count > 0 ? arr.Elements[0] : NullValue.Instance,
                "last" => arr.Elements.Count > 0 ? arr.Elements[arr.Elements.Count - 1] : NullValue.Instance,
                "contains" => CreateBuiltinMethod("contains", args =>
                {
                    if (args.Count < 1) return BooleanValue.False;
                    var search = args[0];
                    for (int i = 0; i < arr.Elements.Count; i++)
                        if (ValuesEqual(arr.Elements[i], search)) return BooleanValue.True;
                    return BooleanValue.False;
                }),
                "indexOf" => CreateBuiltinMethod("indexOf", args =>
                {
                    if (args.Count < 1) return new IntegerValue(-1);
                    var search = args[0];
                    for (int i = 0; i < arr.Elements.Count; i++)
                        if (ValuesEqual(arr.Elements[i], search)) return new IntegerValue(i);
                    return new IntegerValue(-1);
                }),
                "join" => CreateBuiltinMethod("join", args =>
                {
                    var sep = args.Count > 0 ? FormatValue(args[0]) : ", ";
                    return new StringValue(string.Join(sep, arr.Elements.Select(FormatValue)));
                }),
                "reverse" => CreateBuiltinMethod("reverse", args =>
                {
                    arr.Elements.Reverse();
                    return obj;
                }),
                _ => throw CreateTypeError(expr, "array method (length, push, pop, first, last, contains, indexOf, join, reverse)", expr.MemberName)
            };
        }

        // Built-in methods for maps
        if (obj is MapValue map)
        {
            // First check if it's a direct property access
            if (map.Entries.TryGetValue(expr.MemberName, out var propVal))
                return propVal;

            // Then check built-in methods
            return expr.MemberName switch
            {
                "length" => new IntegerValue(map.Entries.Count),
                "keys" => CreateBuiltinMethod("keys", args =>
                {
                    return new ArrayValue(map.Entries.Keys.Select(k => (AlxValue)new StringValue(k)).ToList());
                }),
                "values" => CreateBuiltinMethod("values", args =>
                {
                    return new ArrayValue(map.Entries.Values.ToList());
                }),
                "containsKey" => CreateBuiltinMethod("containsKey", args =>
                {
                    if (args.Count < 1) return BooleanValue.False;
                    if (args[0] is StringValue keyStr)
                        return BooleanValue.FromBool(map.Entries.ContainsKey(keyStr.Value));
                    return BooleanValue.False;
                }),
                "get" => CreateBuiltinMethod("get", args =>
                {
                    if (args.Count < 2 || args[0] is not StringValue keyStr) return NullValue.Instance;
                    if (map.Entries.TryGetValue(keyStr.Value, out var val)) return val;
                    if (args.Count > 1) return args[1]; // default value
                    return NullValue.Instance;
                }),
                _ => throw CreateTypeError(expr, "map property or method", expr.MemberName)
            };
        }

        // String length
        if (obj is StringValue str)
        {
            if (expr.MemberName == "length")
                return new IntegerValue(str.Value.Length);
        }

        throw CreateTypeError(expr, "array, map, or string", obj.TypeName);
    }

    private AlxValue CreateBuiltinMethod(string name, Func<List<AlxValue>, AlxValue> impl)
    {
        return new LambdaBuiltinFunction(name, impl);
    }

    private AlxValue EvaluateAssignment(AssignmentExpression expr)
    {
        var value = Evaluate(expr.Value);
        if (!_environment.Set(expr.Name, value))
            _rootEnvironment.Define(expr.Name, value);
        return value;
    }

    // ===== 0.5.0: CLASSES & OBJECTS =====

    private AlxValue EvaluateNew(NewExpression expr)
    {
        var classVal = _environment.Get(expr.ClassName);
        if (classVal is not AlxClassValue classValue)
        {
            _diagnostics.ReportTypeMismatch(expr.SourceFile, expr.Line, expr.Column, "class", classVal?.TypeName ?? "undefined");
            return NullValue.Instance;
        }

        var instance = new AlxInstanceValue(classValue);

        // Call constructor if present
        if (classValue.Constructor != null)
        {
            var constructor = classValue.Constructor;
            if (expr.Arguments.Count != constructor.Declaration.Parameters.Count)
            {
                _diagnostics.ReportWrongArgumentCount(expr.SourceFile, expr.Line, expr.Column,
                    "constructor", constructor.Declaration.Parameters.Count, expr.Arguments.Count);
                return NullValue.Instance;
            }

            var funcEnv = new AlxEnvironment(classValue.Constructor.Closure);
            funcEnv.Define("this", instance);
            for (int i = 0; i < constructor.Declaration.Parameters.Count; i++)
                funcEnv.Define(constructor.Declaration.Parameters[i], Evaluate(expr.Arguments[i]));

            try
            {
                ExecuteBlock(constructor.Declaration.Body, funcEnv);
            }
            catch (ReturnException) { /* constructor returns are ignored */ }
        }
        else if (expr.Arguments.Count > 0)
        {
            _diagnostics.ReportWrongArgumentCount(expr.SourceFile, expr.Line, expr.Column,
                expr.ClassName, 0, expr.Arguments.Count);
        }

        return instance;
    }

    private AlxValue EvaluateThis()
    {
        var value = _environment.Get("this");
        if (value == null)
        {
            _diagnostics.ReportTypeMismatch("", 0, 0, "class instance", "nothing (this used outside a method)");
            return NullValue.Instance;
        }
        return value;
    }

    private AlxValue EvaluateSuper(SuperExpression expr)
    {
        var thisVal = _environment.Get("this");
        if (thisVal is not AlxInstanceValue instance)
        {
            _diagnostics.ReportTypeMismatch(expr.SourceFile, expr.Line, expr.Column, "class instance", thisVal?.TypeName ?? "undefined");
            return NullValue.Instance;
        }

        var superclass = instance.Class.Superclass;
        if (superclass == null)
        {
            _diagnostics.ReportTypeMismatch(expr.SourceFile, expr.Line, expr.Column, "class with superclass", "no superclass");
            return NullValue.Instance;
        }

        // super(args) — call parent constructor
        if (expr.MethodName == "constructor")
        {
            if (superclass.Constructor != null)
            {
                var constructor = superclass.Constructor;
                if (expr.Arguments.Count != constructor.Declaration.Parameters.Count)
                {
                    _diagnostics.ReportWrongArgumentCount(expr.SourceFile, expr.Line, expr.Column,
                        "super constructor", constructor.Declaration.Parameters.Count, expr.Arguments.Count);
                    return NullValue.Instance;
                }

                var funcEnv = new AlxEnvironment(constructor.Closure);
                funcEnv.Define("this", instance);
                for (int i = 0; i < constructor.Declaration.Parameters.Count; i++)
                    funcEnv.Define(constructor.Declaration.Parameters[i], Evaluate(expr.Arguments[i]));

                try
                {
                    ExecuteBlock(constructor.Declaration.Body, funcEnv);
                }
                catch (ReturnException) { }

                return NullValue.Instance;
            }
            return NullValue.Instance;
        }

        // super.method(args) — call parent method
        if (superclass.Methods.TryGetValue(expr.MethodName, out var method) && method is AlxFunction func)
        {
            return new AlxBoundMethod(func, instance);
        }

        _diagnostics.ReportTypeMismatch(expr.SourceFile, expr.Line, expr.Column,
            $"method '{expr.MethodName}'", "not found in superclass");
        return NullValue.Instance;
    }

    private AlxValue EvaluateMemberAssignment(MemberAssignmentExpression expr)
    {
        var obj = Evaluate(expr.Object);
        var value = Evaluate(expr.Value);

        if (obj is AlxInstanceValue instance)
        {
            instance.Properties[expr.MemberName] = value;
            return value;
        }

        if (obj is MapValue map)
        {
            map.Entries[expr.MemberName] = value;
            return value;
        }

        if (obj is ArrayValue arr)
        {
            // Allow setting length to shrink the array
            if (expr.MemberName == "length" && value is IntegerValue newLen)
            {
                while (arr.Elements.Count > newLen.Value)
                    arr.Elements.RemoveAt(arr.Elements.Count - 1);
                return value;
            }
        }

        throw CreateTypeError(expr, "map or array", obj.TypeName);
    }

    private AlxValue EvaluateIndexAssignment(IndexAssignmentExpression expr)
    {
        var obj = Evaluate(expr.Object);
        var index = Evaluate(expr.Index);
        var value = Evaluate(expr.Value);

        if (obj is ArrayValue arr)
        {
            if (index is IntegerValue intIdx)
            {
                if (intIdx.Value < 0 || intIdx.Value >= arr.Elements.Count)
                {
                    _diagnostics.ReportTypeMismatch(expr.SourceFile, expr.Line, expr.Column,
                        $"valid index (0..{arr.Elements.Count - 1})", intIdx.Value.ToString());
                    return NullValue.Instance;
                }
                arr.Elements[(int)intIdx.Value] = value;
                return value;
            }
            throw CreateTypeError(expr, "integer index", index.TypeName);
        }

        if (obj is MapValue map)
        {
            if (index is StringValue keyStr)
            {
                map.Entries[keyStr.Value] = value;
                return value;
            }
            throw CreateTypeError(expr, "string key", index.TypeName);
        }

        throw CreateTypeError(expr, "array or map", obj.TypeName);
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

/// <summary>
/// A builtin function implemented as a lambda/delegate (used for array/map methods).
/// </summary>
public class LambdaBuiltinFunction : FunctionValue
{
    private readonly Func<List<AlxValue>, AlxValue> _impl;
    public LambdaBuiltinFunction(string name, Func<List<AlxValue>, AlxValue> impl) : base(name)
    {
        _impl = impl;
    }
    public AlxValue Invoke(List<AlxValue> args) => _impl(args);
}

/// <summary>
/// Built-in input() function — reads user input from stdin.
/// Usage: input() or input("Enter name: ")
/// </summary>
public class InputFunction : AlxBuiltinFunction
{
    public InputFunction() : base("input") { }

    public override AlxValue Invoke(List<AlxValue> args, string sourceFile, int line, int column)
    {
        // Print prompt if provided
        if (args.Count > 0 && args[0] is StringValue prompt)
        {
            Console.Write(prompt.Value);
        }

        // Read from stdin
        string? input = Console.ReadLine();
        return new StringValue(input ?? "");
    }
}
