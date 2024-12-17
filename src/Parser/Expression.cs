namespace albus.src.Parser;

public abstract class Expression {}

public sealed class LiteralExpression(dynamic literal) : Expression {
    public readonly dynamic Literal = literal;

    public override string ToString() {
        return $"[{Literal}]";
    }
}

public sealed class VariableDeclaration(string identifier, bool isMutable, Expression value) : Expression {
    public readonly string Identifier = identifier;
    public readonly bool IsMutable = isMutable;
    public readonly Expression Value = value;

    public override string ToString() {
        return $"{(IsMutable ? "mut" : "")}[{Identifier}] = {Value}";
    }
}

public sealed class BinaryExpression(Expression left, Token op, Expression right) : Expression {
    public readonly Expression Left = left;
    public readonly Token Op = op;
    public readonly Expression Right = right;

    public override string ToString() {
        return $"({Left} {Op.Lexeme} {Right})";
    }
}

public sealed class UnaryExpression(Token op, Expression right) : Expression {
    public readonly Token Op = op;
    public readonly Expression Right = right;

    public override string ToString() {
        return $"({Op.Lexeme} {Right})";
    }
}