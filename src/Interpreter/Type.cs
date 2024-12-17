namespace albus.src.Interpreter;

public abstract class ValueType {}

public sealed class IntegerType(int value) : ValueType {
    public readonly int Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }
}