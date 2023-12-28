using System.Diagnostics;
using System.Text;
using CppSharp;
using Spectre.Console;

namespace QtGen;

static class Program
{
    static void Main()
    {
        if (!File.Exists("QtGen.sln"))
        {
            throw new Exception("Generator must be run from solution directory");
        }

        // Print title figlet.
        AnsiConsole.Write(new FigletText("QtGen").Color(Color.Red));

        // Clear existing files from build folder
        foreach (var file in Directory.GetFiles("build", "", SearchOption.AllDirectories))
        {
            File.Delete(file);
        }

        // Run generator driver
        PrintLabel("Generate");
        ConsoleDriver.Run(
            new Library(
                @"F:\Qt\5.15.2\msvc2019_64",
                Path.Combine(Directory.GetCurrentDirectory(), "build")
            )
        );

        // Copy project files to build folder
        File.Copy("src/QtGen/Build/Qt.csproj", "build/Qt.csproj", true);

        // Build C# project
        PrintLabel("Compile (C#)");
        RunProcess("dotnet", "build", Path.Combine(Directory.GetCurrentDirectory(), "build"));
    }

    static void PrintLabel(string text)
    {
        AnsiConsole.MarkupLine($"\n── [green]{text}[/] ──".PadRight(128, '─'));
    }

    static void RunProcess(string name, string args, string? workingDirectory = null)
    {
        var process = new Process()
        {
            StartInfo = new()
            {
                FileName = name,
                WorkingDirectory = workingDirectory,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            },
        };

        process.Start();
        AnsiConsole.WriteLine(process.StandardOutput.ReadToEnd());
        process.WaitForExit();
    }
}
