namespace albus.src.Parser;

public abstract class Expression {
    public int Line;
}

public sealed class LiteralExpression : Expression {
    public readonly dynamic Literal;

    public LiteralExpression(dynamic literal, int line) {
        Literal = literal;
        Line = line;
    }

    public override string ToString() {
        return $"[{Literal}]";
    }
}

public sealed class VariableDeclaration : Expression {
    public readonly string Identifier;
    public readonly bool IsMutable;
    public readonly Expression Value;

    public VariableDeclaration(string identifier, bool isMutable, Expression value, int line) {
        Identifier = identifier;
        IsMutable = isMutable;
        Value = value;
        Line = line;
    }

    public override string ToString() {
        return $"{(IsMutable ? "mut" : "")}[{Identifier}] = {Value}";
    }
}

public sealed class BinaryExpression : Expression {
    public readonly Expression Left;
    public readonly Token Op;
    public readonly Expression Right;

    public BinaryExpression(Expression left, Token op, Expression right, int line) {
        Left = left;
        Op = op;
        Right = right;
        Line = line;
    }

    public override string ToString() {
        return $"({Left} {Op.Lexeme} {Right})";
    }
}

public sealed class UnaryExpression : Expression {
    public readonly Token Op;
    public readonly Expression Right;

    public UnaryExpression(Token op, Expression right, int line) {
        Op = op;
        Right = right;
        Line = line;
    }

    public override string ToString() {
        return $"({Op.Lexeme} {Right})";
    }
}