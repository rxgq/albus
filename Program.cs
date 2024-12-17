using System.Runtime.CompilerServices;
using albus.src.Parser;

namespace albus;

internal class Program 
{
    static void Main() 
    {
        var source = File.ReadAllText("example/source.txt");

        var lexer = new Lexer(source);
        var result = lexer.Tokenize();
        if (!result.IsSuccess) return;

        // add a dividebyzero error if x/0 is encountered while parsing
        var parser = new Parser(result.Value!);
        var astResult = parser.ParseAst();
        if (!astResult.IsSuccess) {
            Console.Write(astResult.Error);
            return;
        }

        foreach (var expr in astResult.Value ?? []) {
            Console.WriteLine(expr.ToString());
        }
    }
}