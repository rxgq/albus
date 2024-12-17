namespace albus.src.Parser;

public abstract class Expression {}

public sealed class LiteralExpressioon(object literal) : Expression {
    public readonly object Literal = literal;
}

public sealed class VariableDeclaration(string identifier, Expression value) : Expression {
    public readonly string Identifier = identifier;
    public readonly Expression Value = value;
}