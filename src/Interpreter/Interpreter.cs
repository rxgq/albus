using System.Security.Cryptography;
using albus.src.Parser;

namespace albus.src.Interpreter;

public sealed class Interpreter(List<Expression> expressions) {
    private readonly List<Expression> Expressions = expressions;
    private int CurrentLine = 1;

    public Result<ValueType> InterpretProgram() {
        var result = Result<ValueType>.Ok(new IntegerType(0));

        foreach (var expr in Expressions) {
            CurrentLine = expr.Line;

            result = EvaluateExpression(expr);
            if (!result.IsSuccess) return result;
        }

        return result;
    }

    private Result<ValueType> EvaluateExpression(Expression expr) {
        return expr switch {
            _ when expr is BinaryExpression binExpr  => EvaluateBinaryExpression(binExpr),
            _ when expr is LiteralExpression litExpr => EvaluatePrimaryExpression(litExpr),
            _ => TypeError("unknown expression"),
        };
    }

    private Result<ValueType> EvaluateBinaryExpression(BinaryExpression expr) {
        var leftResult = EvaluateExpression(expr.Left);
        var rightResult = EvaluateExpression(expr.Right);

        if (!leftResult.IsSuccess || !rightResult.IsSuccess) {
            return TypeError("error evaluating expression");
        }

        return EvaluateNumericExpression(leftResult.Value!, expr.Op, rightResult.Value!);
    }

    private Result<ValueType> EvaluateNumericExpression(ValueType left, Token op, ValueType right) {
        double leftValue;
        double rightValue;

        if (left is FloatType leftFloat) {
            leftValue = leftFloat.Value;
        } else if (left is IntegerType leftInt) {
            leftValue = leftInt.Value;
        } else {
            return TypeError("expected numeric type");
        }

        if (right is FloatType rightFloat) {
            rightValue = rightFloat.Value;
        } else if (right is IntegerType rightInt) {
            rightValue = rightInt.Value;
        } else {
            return Result<ValueType>.Err();
        }

        double result = op.Type switch {
            TokenType.Plus   => leftValue + rightValue,
            TokenType.Minus  => leftValue - rightValue,
            TokenType.Star   => leftValue * rightValue,
            TokenType.Slash  => leftValue / rightValue,
            TokenType.Modulo => leftValue % rightValue,
            _                => throw new InvalidOperationException("invalid binary operator")
        };

        if (left is FloatType || right is FloatType) {
            return Result<ValueType>.Ok(new FloatType(result));
        }

        return Result<ValueType>.Ok(new IntegerType((int)result));
    }
    
    private Result<ValueType> EvaluatePrimaryExpression(LiteralExpression expr) {
        return expr switch {
            _ when expr.Literal is int val    => Result<ValueType>.Ok(new IntegerType(val)),
            _ when expr.Literal is double val => Result<ValueType>.Ok(new FloatType(val)),
            _ when expr.Literal is string val => Result<ValueType>.Ok(new StringType(val)),
            _ => TypeError("Unknown primary expression")
        };
    }

    private Result<ValueType> TypeError(string message) {
        Console.Write($"TypeError: {message} on line {CurrentLine}");
        return Result<ValueType>.Err();
    }
}