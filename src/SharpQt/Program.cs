using System.Diagnostics;
using System.Reflection;
using CppSharp;
using Spectre.Console;

namespace SharpQt;

static class Program
{
    static void Main()
    {
        string qtDir = @"F:\Qt\5.15.2\msvc2019_64";
        string buildDir = Path.Combine(Directory.GetCurrentDirectory(), "build");
        string outDir = Path.Combine(Directory.GetCurrentDirectory(), "bin");

        var watch = Stopwatch.StartNew();

        try
        {
            // Print title figlet.
            AnsiConsole.Write(new FigletText("SharpQt").Color(Color.Red));

            PrintLabel("Configure");
            {
                qtDir = AnsiConsole.Ask("Path to Qt5:", qtDir);
                buildDir = AnsiConsole.Ask("Path for build files:", buildDir);
                outDir = AnsiConsole.Ask("Path for output files:", outDir);

                // No-op if dir already exists
                Directory.CreateDirectory(buildDir);
                Directory.CreateDirectory(outDir);

                // Clear existing files from build folder
                foreach (var file in Directory.GetFiles(buildDir, "", SearchOption.AllDirectories))
                {
                    File.Delete(file);
                }
            }

            // Run generator driver
            PrintLabel("Generate");
            {
                ConsoleDriver.Run(new Library(qtDir, buildDir));
            }

            // Build C# project
            PrintLabel("Compile (C#)");
            {
                using (var fileStream = File.Create(Path.Combine(buildDir, "Qt.csproj"), 0))
                {
                    using var resStream = Assembly
                        .GetExecutingAssembly()
                        .GetManifestResourceStream("SharpQt.Build.Qt.csproj");

                    // Write project files to build folder
                    resStream!.CopyTo(fileStream);
                }

                RunProcess("dotnet", $"build -o \"{outDir}\"", buildDir);
            }

            AnsiConsole.MarkupLine($"[green]Done in {watch.Elapsed.ToString(@"mm\:ss")}![/]");
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
        }
    }

    static void PrintLabel(string text)
    {
        AnsiConsole.MarkupLine($"\n── [green]{text}[/] ──".PadRight(128, '─'));
    }

    static void RunProcess(string name, string args, string? workingDirectory = null)
    {
        using var process = new Process()
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

        if (process.ExitCode != 0)
        {
            throw new Exception($"Process '{name} {args}' exited with code {process.ExitCode}.");
        }
    }
}
