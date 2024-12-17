using albus.src.Parser;

namespace albus;

internal class Program 
{
    static void Main() 
    {
        var source = File.ReadAllText("example/source.txt");

        var lexer = new Lexer(source);
        var result = lexer.Tokenize();

        if (result.Error is not null) {
            return;
        }

    }
}