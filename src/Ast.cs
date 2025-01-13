namespace albus.src;

public sealed class Ast {
    public readonly List<Expression> Body = new();
}

public abstract record Expression { }

public sealed record VariableDeclaration(IdentifierExpr Identifier, Expression Expr, bool IsMutable) : Expression {
    public readonly IdentifierExpr Id = Identifier;
    public readonly Expression Expr = Expr;
    public readonly bool IsMutable = IsMutable;
}

public sealed record IdentifierExpr(string Name) : Expression {
    public readonly string Name = Name;
}

public sealed record LiteralExpr(object Value) : Expression {
    public readonly object Value = Value;
}