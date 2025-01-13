namespace albus.src;

public sealed class Ast {
    public readonly List<Expression> Body = [];
}

public abstract record Expression { }

public sealed record VariableDeclaration(IdentifierExpr Identifier, Expression Expr, string? Type, bool IsMutable) : Expression {
    public readonly IdentifierExpr Id = Identifier;
    public readonly Expression Expr = Expr;
    public readonly string? Type = Type;
    public readonly bool IsMutable = IsMutable;
}

public sealed record AssignmentStatement(IdentifierExpr Identifier, Expression Expr) : Expression {
    public readonly IdentifierExpr Id = Identifier;
    public readonly Expression Expr = Expr;
}

public sealed record IdentifierExpr(string Name) : Expression {
    public readonly string Name = Name;
}

public sealed record LiteralExpr(object Value) : Expression {
    public readonly object Value = Value;
}