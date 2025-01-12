using albus.src;

namespace albus;

internal class Program 
{
    static void Main(string[] args) 
    {
        if (args.Length < 1) {
            Console.WriteLine("expected source file path");
            return;
        }

        if (!File.Exists(args[0])) {
            Console.WriteLine("source file path does not exist");
            return;
        }

        bool isDebug = false;
        if (args.Length == 2) {
            if (args[1] == "-d") isDebug = true;
            else {
                Console.WriteLine($"invalid flag '{args[1]}'");
            }
        }

        var source = File.ReadAllText(args[0]);
        var lexer = new Lexer(source, isDebug);
        var tokens = lexer.Tokenize();
        
    }
}