namespace albus.src.Interpreter;

public abstract class ValueType {

}


public sealed class IntegerType(int value) : ValueType {
    public readonly int Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }
}

public sealed class FloatType(double value) : ValueType {
    public readonly double Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }
}

public sealed class StringType(string value) : ValueType {
    public readonly string Value = value;

    public override string ToString()
    {
        return Value;
    }
}

public sealed class BoolType(bool value) : ValueType {
    public readonly bool Value = value;

    public override string ToString()
    {
        return Value.ToString();
    }
}