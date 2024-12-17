using System.Diagnostics;
using System.Runtime.CompilerServices;
using albus.src.Interpreter;
using albus.src.Parser;

namespace albus;

internal class Program 
{
    static void Main() 
    {
        bool isDebug = true;
        var source = File.ReadAllText("example/source.txt");

        var lexer = new Lexer(source, debug: isDebug);
        var result = lexer.Tokenize();
        if (!result.IsSuccess) return;

        var parser = new Parser(result.Value!, isDebug: isDebug);
        var astResult = parser.ParseAst();
        if (!astResult.IsSuccess) return;

        var interpreter = new Interpreter(astResult.Value!);
        var programResult = interpreter.InterpretProgram();

        if (programResult.IsSuccess) {
            Console.WriteLine(programResult.Value!.ToString());
        }

        
    }
}