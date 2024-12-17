using albus.src.Parser;

namespace albus.src.Interpreter;

public sealed class Interpreter(List<Expression> expressions) {
    private readonly List<Expression> Expressions = expressions;

    public Result<bool> Interpret() {
        foreach (var expr in Expressions) {
            var result = ExecuteExpression(expr);
            if (!result.IsSuccess) return result;
        }

        return Result<bool>.Ok(true);
    }

    public Result<bool> ExecuteExpression(Expression expr) {
        return Result<bool>.Ok(true);
    }
}