namespace ALX.Compiler.Lexer;

public enum TokenType
{
    Error, Eof,
    Integer, Float, String, InterpolatedString, Boolean, Null,
    Identifier, Function, Afun, If, Else, While, For, In, Return, True, False, NullLiteral, Const, And, Or, Not,    Print,
    Break,
    Continue,
    Lambda,
    Class, This, New, Extends, Super,
    Import, From, As,
    Plus, Minus, Star, Slash, Percent, Assign, Equal, NotEqual, Less, Greater, LessEqual, GreaterEqual, DotDot,
    LeftParen, RightParen, LeftBrace, RightBrace, LeftBracket, RightBracket, Comma, Dot, Colon, Semicolon,
    Newline, Comment,
}
