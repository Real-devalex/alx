using ALX.Compiler.AST;
using ALX.Compiler.Diagnostics;
using ALX.Compiler.Lexer;

namespace ALX.Compiler.Parser;

public class Parser
{
    private readonly List<Token> _tokens;
    private readonly string _sourceFile;
    private readonly DiagnosticBag _diagnostics;
    private int _current;

    public Parser(List<Token> tokens, string sourceFile = "", DiagnosticBag? diagnostics = null)
    {
        _tokens = tokens;
        _sourceFile = sourceFile;
        _diagnostics = diagnostics ?? new DiagnosticBag();
    }

    public ProgramNode Parse()
    {
        var statements = new List<Statement>();

        while (!IsAtEnd())
        {
            var statement = ParseStatement();
            if (statement != null)
            {
                statements.Add(statement);
            }
        }

        return new ProgramNode(statements, 1, 1, _sourceFile);
    }

    private Statement? ParseStatement()
    {
        SkipNewlines();
        if (IsAtEnd()) return null;

        var token = Peek();

        switch (token.Type)
        {
            case TokenType.Function:
                return ParseFunctionDeclaration();
            case TokenType.If:
                return ParseIfStatement();
            case TokenType.While:
                return ParseWhileStatement();
            case TokenType.For:
                return ParseForStatement();
            case TokenType.Return:
                return ParseReturnStatement();
            case TokenType.Break:
                return ParseBreakStatement();
            case TokenType.Continue:
                return ParseContinueStatement();
            case TokenType.LeftBrace:
                return ParseBlock();
            case TokenType.Const:
                return ParseVariableDeclaration(isConstant: true);
            case TokenType.Print:
                return ParsePrintStatement();
            case TokenType.Identifier:
                return ParseIdentifierStatement();
            default:
                return ParseExpressionStatement();
        }
    }

    // ===== NEW STATEMENTS (0.2.0) =====

    private Statement ParseForStatement()
    {
        var keyword = Advance(); // consume 'for'
        var varName = Expect(TokenType.Identifier, "loop variable name");
        Expect(TokenType.In, "'in'");
        var iterable = ParseRangeExpression();
        var body = ParseBlock();

        return new ForStatement(
            varName.Lexeme,
            iterable,
            body,
            keyword.Line,
            keyword.Column,
            _sourceFile
        );
    }

    private Statement ParseBreakStatement()
    {
        var keyword = Advance(); // consume 'break'
        Match(TokenType.Semicolon);
        Match(TokenType.Newline);
        return new BreakStatement(keyword.Line, keyword.Column, _sourceFile);
    }

    private Statement ParseContinueStatement()
    {
        var keyword = Advance(); // consume 'continue'
        Match(TokenType.Semicolon);
        Match(TokenType.Newline);
        return new ContinueStatement(keyword.Line, keyword.Column, _sourceFile);
    }

    // ===== EXISTING STATEMENTS =====

    private Statement ParseFunctionDeclaration()
    {
        var keyword = Advance();
        var name = Expect(TokenType.Identifier, "function name");
        Expect(TokenType.LeftParen, "'('");

        var parameters = new List<string>();
        if (Peek().Type != TokenType.RightParen)
        {
            do
            {
                var param = Expect(TokenType.Identifier, "parameter name");
                parameters.Add(param.Lexeme);
            } while (Match(TokenType.Comma));
        }

        Expect(TokenType.RightParen, "')'");
        var body = ParseBlock();

        return new FunctionDeclaration(name.Lexeme, parameters, body, keyword.Line, keyword.Column, _sourceFile);
    }

    private Statement ParseIfStatement()
    {
        var keyword = Advance();
        var condition = ParseExpression();
        var thenBranch = ParseBlock();

        Statement? elseBranch = null;
        if (Match(TokenType.Else))
        {
            if (Peek().Type == TokenType.If)
                elseBranch = ParseIfStatement();
            else
                elseBranch = ParseBlock();
        }

        return new IfStatement(condition, thenBranch, elseBranch, keyword.Line, keyword.Column, _sourceFile);
    }

    private Statement ParseWhileStatement()
    {
        var keyword = Advance();
        var condition = ParseExpression();
        var body = ParseBlock();
        return new WhileStatement(condition, body, keyword.Line, keyword.Column, _sourceFile);
    }

    private Statement ParseReturnStatement()
    {
        var keyword = Advance();
        Expression? value = null;
        if (Peek().Type != TokenType.Newline && Peek().Type != TokenType.RightBrace && !IsAtEnd())
            value = ParseExpression();
        return new ReturnStatement(value, keyword.Line, keyword.Column, _sourceFile);
    }

    private BlockStatement ParseBlock()
    {
        SkipNewlines();
        var openBrace = Expect(TokenType.LeftBrace, "'{'");
        var statements = new List<Statement>();

        SkipNewlines();
        while (!IsAtEnd() && Peek().Type != TokenType.RightBrace)
        {
            var statement = ParseStatement();
            if (statement != null) statements.Add(statement);
            SkipNewlines();
        }

        Expect(TokenType.RightBrace, "'}'");
        return new BlockStatement(statements, openBrace.Line, openBrace.Column, _sourceFile);
    }

    private Statement? ParseIdentifierStatement()
    {
        return ParseExpressionStatement();
    }

    private Statement ParseVariableDeclaration(bool isConstant)
    {
        Token keyword;
        Token name;

        if (isConstant)
        {
            keyword = Advance();
            name = Expect(TokenType.Identifier, "variable name");
        }
        else
        {
            name = Advance();
            keyword = name;
        }

        Expect(TokenType.Assign, "'='");
        var initializer = ParseExpression();
        return new VariableDeclaration(name.Lexeme, initializer, isConstant, keyword.Line, keyword.Column, _sourceFile);
    }

    private Statement ParsePrintStatement()
    {
        var keyword = Advance();
        Expect(TokenType.LeftParen, "'('");
        var expression = ParseExpression();
        Expect(TokenType.RightParen, "')'");
        Match(TokenType.Semicolon);
        Match(TokenType.Newline);
        return new PrintStatement(expression, keyword.Line, keyword.Column, _sourceFile);
    }

    private Statement ParseExpressionStatement()
    {
        var expression = ParseExpression();
        Match(TokenType.Semicolon);
        Match(TokenType.Newline);
        return new ExpressionStatement(expression, expression.Line, expression.Column, _sourceFile);
    }

    // ===== EXPRESSIONS =====

    public Expression ParseExpression()
    {
        return ParseAssignment();
    }

    private Expression ParseAssignment()
    {
        var expr = ParseOr();

        if (Match(TokenType.Assign))
        {
            if (expr is IdentifierExpression identifier)
            {
                var value = ParseAssignment();
                return new AssignmentExpression(identifier.Name, value, identifier.Line, identifier.Column, _sourceFile);
            }

            _diagnostics.ReportUnexpectedToken(
                _sourceFile, expr.Line, expr.Column, "identifier",
                expr switch { IntegerExpression => "integer", FloatExpression => "float", StringExpression => "string", BooleanExpression => "boolean", _ => "expression" }
            );
        }

        return expr;
    }

    private Expression ParseOr()
    {
        var left = ParseAnd();
        while (Match(TokenType.Or))
        {
            var right = ParseAnd();
            left = new BinaryExpression(left, TokenType.Or, right, left.Line, left.Column, _sourceFile);
        }
        return left;
    }

    private Expression ParseAnd()
    {
        var left = ParseEquality();
        while (Match(TokenType.And))
        {
            var right = ParseEquality();
            left = new BinaryExpression(left, TokenType.And, right, left.Line, left.Column, _sourceFile);
        }
        return left;
    }

    private Expression ParseEquality()
    {
        var left = ParseComparison();
        while (Peek().Type is TokenType.Equal or TokenType.NotEqual)
        {
            var op = Advance();
            var right = ParseComparison();
            left = new BinaryExpression(left, op.Type, right, left.Line, left.Column, _sourceFile);
        }
        return left;
    }

    private Expression ParseComparison()
    {
        var left = ParseRange();
        while (Peek().Type is TokenType.Greater or TokenType.GreaterEqual or TokenType.Less or TokenType.LessEqual)
        {
            var op = Advance();
            var right = ParseRange();
            left = new BinaryExpression(left, op.Type, right, left.Line, left.Column, _sourceFile);
        }
        return left;
    }

    /// <summary>
    /// Parse range expressions: start..end (used in for loops and standalone)
    /// </summary>
    private Expression ParseRange()
    {
        var start = ParseAddition();
        if (Match(TokenType.DotDot))
        {
            var end = ParseAddition();
            return new RangeExpression(start, end, start.Line, start.Column, _sourceFile);
        }
        return start;
    }

    private Expression ParseAddition()
    {
        var left = ParseMultiplication();
        while (Peek().Type is TokenType.Plus or TokenType.Minus)
        {
            var op = Advance();
            var right = ParseMultiplication();
            left = new BinaryExpression(left, op.Type, right, left.Line, left.Column, _sourceFile);
        }
        return left;
    }

    private Expression ParseMultiplication()
    {
        var left = ParseUnary();
        while (Peek().Type is TokenType.Star or TokenType.Slash or TokenType.Percent)
        {
            var op = Advance();
            var right = ParseUnary();
            left = new BinaryExpression(left, op.Type, right, left.Line, left.Column, _sourceFile);
        }
        return left;
    }

    private Expression ParseUnary()
    {
        if (Peek().Type is TokenType.Minus or TokenType.Not)
        {
            var op = Advance();
            var operand = ParseUnary();
            return new UnaryExpression(op.Type, operand, op.Line, op.Column, _sourceFile);
        }
        return ParseCall();
    }

    private Expression ParseCall()
    {
        var expr = ParsePrimary();

        while (Match(TokenType.LeftParen))
        {
            var arguments = new List<Expression>();
            if (Peek().Type != TokenType.RightParen)
            {
                do { arguments.Add(ParseExpression()); } while (Match(TokenType.Comma));
            }
            Expect(TokenType.RightParen, "')'");
            expr = new CallExpression(expr, arguments, expr.Line, expr.Column, _sourceFile);
        }

        return expr;
    }

    private Expression ParsePrimary()
    {
        SkipNewlines();
        var token = Peek();

        switch (token.Type)
        {
            case TokenType.Integer:
                Advance();
                return new IntegerExpression((long)token.Literal!, token.Line, token.Column, _sourceFile);

            case TokenType.Float:
                Advance();
                return new FloatExpression((double)token.Literal!, token.Line, token.Column, _sourceFile);

            case TokenType.String:
                Advance();
                return new StringExpression((string)token.Literal!, token.Line, token.Column, _sourceFile);

            case TokenType.InterpolatedString:
                return ParseInterpolatedString();

            case TokenType.True:
                Advance();
                return new BooleanExpression(true, token.Line, token.Column, _sourceFile);

            case TokenType.False:
                Advance();
                return new BooleanExpression(false, token.Line, token.Column, _sourceFile);

            case TokenType.NullLiteral:
                Advance();
                return new NullExpression(token.Line, token.Column, _sourceFile);

            case TokenType.Identifier:
                Advance();
                return new IdentifierExpression(token.Lexeme, token.Line, token.Column, _sourceFile);

            case TokenType.Lambda:
                return ParseLambdaExpression();

            case TokenType.LeftParen:
                Advance();
                var expr = ParseExpression();
                Expect(TokenType.RightParen, "')'");
                return expr;

            default:
                _diagnostics.ReportUnexpectedToken(_sourceFile, token.Line, token.Column, "expression", token.Lexeme);
                Advance();
                return new NullExpression(token.Line, token.Column, _sourceFile);
        }
    }

    /// <summary>
    /// Parse an interpolated string: "Hello, {name}!"
    /// Split the raw value on '{' and '}' boundaries and parse expressions between them.
    /// </summary>
    private Expression ParseInterpolatedString()
    {
        var token = Advance();
        string rawValue = (string)token.Literal!;
        var parts = new List<Expression>();

        // Split the raw value into alternating string literal and expression parts
        // Raw value looks like: "Hello, {name}!" → segments: ["Hello, ", "name", "!"]
        int i = 0;
        while (i < rawValue.Length)
        {
            int braceStart = rawValue.IndexOf('{', i);
            if (braceStart == -1)
            {
                // Rest is a string literal
                string remaining = rawValue.Substring(i);
                if (remaining.Length > 0)
                    parts.Add(new StringExpression(remaining, token.Line, token.Column, _sourceFile));
                break;
            }

            // Text before the brace
            if (braceStart > i)
            {
                string text = rawValue.Substring(i, braceStart - i);
                parts.Add(new StringExpression(text, token.Line, token.Column, _sourceFile));
            }

            // Find the closing brace
            int braceEnd = rawValue.IndexOf('}', braceStart);
            if (braceEnd == -1)
            {
                // No closing brace — treat rest as string
                _diagnostics.ReportUnexpectedToken(_sourceFile, token.Line, token.Column, "'}'", "end of string");
                string remaining = rawValue.Substring(braceStart);
                parts.Add(new StringExpression(remaining, token.Line, token.Column, _sourceFile));
                break;
            }

            // Parse the expression between braces
            string expressionText = rawValue.Substring(braceStart + 1, braceEnd - braceStart - 1);
            if (expressionText.Length > 0)
            {
                // Tokenize and parse the expression
                var innerLexer = new ALX.Compiler.Lexer.Lexer(expressionText, _sourceFile);
                var innerTokens = innerLexer.Tokenize();
                var innerParser = new Parser(innerTokens, _sourceFile);
                var innerExpr = innerParser.ParseExpression();
                parts.Add(innerExpr);
            }
            else
            {
                parts.Add(new StringExpression("", token.Line, token.Column, _sourceFile));
            }

            i = braceEnd + 1;
        }

        return new InterpolatedStringExpression(rawValue, parts, token.Line, token.Column, _sourceFile);
    }

    // ===== LAMBDA =====

    /// <summary>
    /// Parse lambda expression: lambda(params) { body }
    /// </summary>
    private Expression ParseLambdaExpression()
    {
        var keyword = Advance(); // consume 'lambda'
        Expect(TokenType.LeftParen, "'('");

        var parameters = new List<string>();
        if (Peek().Type != TokenType.RightParen)
        {
            do
            {
                var param = Expect(TokenType.Identifier, "parameter name");
                parameters.Add(param.Lexeme);
            } while (Match(TokenType.Comma));
        }

        Expect(TokenType.RightParen, "')'");
        var body = ParseBlock();

        return new LambdaExpression(parameters, body, keyword.Line, keyword.Column, _sourceFile);
    }

    // ===== RANGE =====

    /// <summary>
    /// Parse range expressions: start..end
    /// Range has lower precedence than comparison but higher than assignment.
    /// This is used by for loops: for i in 1..10
    /// </summary>
    private Expression ParseRangeExpression()
    {
        var start = ParseExpression();

        if (Match(TokenType.DotDot))
        {
            var end = ParseExpression();
            return new RangeExpression(start, end, start.Line, start.Column, _sourceFile);
        }

        return start;
    }

    // ===== HELPERS =====

    private void SkipNewlines()
    {
        while (Peek().Type == TokenType.Newline)
            Advance();
    }

    private Token Peek() => _tokens[_current];

    private Token? PeekNext() => _current + 1 < _tokens.Count ? _tokens[_current + 1] : null;

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return _tokens[_current - 1];
    }

    private bool Match(TokenType type)
    {
        if (Peek().Type == type) { Advance(); return true; }
        return false;
    }

    private Token Expect(TokenType type, string description)
    {
        if (Peek().Type == type) return Advance();
        _diagnostics.ReportUnexpectedToken(_sourceFile, Peek().Line, Peek().Column, description, Peek().Lexeme);
        return new Token(type, "", null, Peek().Line, Peek().Column, _sourceFile);
    }

    private bool IsAtEnd() => Peek().Type == TokenType.Eof;
}
