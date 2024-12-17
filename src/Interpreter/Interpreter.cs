using System.Security.Cryptography;
using albus.src.Parser;

namespace albus.src.Interpreter;

public sealed class Interpreter(List<Expression> expressions) {
    private readonly List<Expression> Expressions = expressions;

    public Result<ValueType> InterpretProgram() {
        var result = Result<ValueType>.Ok(new IntegerType(0));

        foreach (var expr in Expressions) {
            result = EvaluateExpression(expr);
            if (!result.IsSuccess) return result;
        }

        return result;
    }

    private Result<ValueType> EvaluateExpression(Expression expr) {
        return expr switch {
            _ when expr is BinaryExpression binExpr  => EvaluateBinaryExpression(binExpr),
            _ when expr is LiteralExpression litExpr => EvaluatePrimaryExpression(litExpr),
            _ => Result<ValueType>.Err(""),
        };
    }

    private Result<ValueType> EvaluateBinaryExpression(BinaryExpression expr) {
        var left = EvaluateExpression(expr.Left);
        var right = EvaluateExpression(expr.Right);

        var operationResult =  expr.Op.Type switch {
            TokenType.Plus => ((IntegerType)left.Value).Value + ((IntegerType)right.Value).Value,
        };

        return Result<ValueType>.Ok(new IntegerType(operationResult));
    }

    private Result<ValueType> EvaluatePrimaryExpression(LiteralExpression expr) {
        return expr switch {
            _ when expr.Literal is int val => Result<ValueType>.Ok(new IntegerType(val)),
        };
    }
}