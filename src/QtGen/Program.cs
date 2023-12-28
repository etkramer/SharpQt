using CppSharp;

namespace QtGen;

static class Program
{
    static void Main(string[] args)
    {
        var lib = new Library(@"F:\Qt\5.15.2\msvc2019_64");
        ConsoleDriver.Run(lib);
    }
}
