using CppSharp;

namespace QtGen;

static class Program
{
    static void Main()
    {
        var lib = new Library(
            @"F:\Qt\5.15.2\msvc2019_64",
            Path.Combine(Directory.GetCurrentDirectory(), "build")
        );

        ConsoleDriver.Run(lib);
    }
}
