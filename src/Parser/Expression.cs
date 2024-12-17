namespace albus.src.Parser;

public abstract class Expression {}

public sealed class LiteralExpression(object literal) : Expression {
    public readonly object Literal = literal;

    public override string ToString() {
        return $"({Literal})";
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