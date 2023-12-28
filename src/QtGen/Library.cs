using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using QtGen.Passes;

namespace QtGen;

public class Library(string QtPath, string OutPath) : ILibrary
{
    public void Setup(Driver driver)
    {
        driver.Options.GenerateDefaultValuesForArguments = true;
        driver.Options.GenerateDeprecatedDeclarations = false;
        driver.Options.CommentKind = CommentKind.BCPLSlash;
        driver.Options.GeneratorKind = GeneratorKind.CSharp;
        driver.Options.MarshalCharAsManagedChar = false;
        driver.Options.MarshalConstCharArrayAsString = false;
        driver.Options.OutputDir = OutPath;

        driver.ParserOptions.AddDefines("QT_NO_OPENGL");

#if DEBUG
        driver.Options.GenerateDebugOutput = true;
#endif

        driver.ParserOptions.EnableRTTI = true;
        driver.ParserOptions.SetupMSVC(VisualStudioVersion.VS2022);
        driver.ParserOptions.Setup(TargetPlatform.Windows);

        driver.ParserOptions.SkipFunctionBodies = true;
        driver.ParserOptions.SkipPrivateDeclarations = true;

        AddModule(driver, "QtCore", "Qt5Core", "Qt");
    }

    void AddModule(Driver driver, string moduleName, string libName, string namespaceName)
    {
        var module = driver.Options.AddModule(moduleName);
        module.OutputNamespace = namespaceName;
        module.LibraryDirs.Add(Path.Combine(QtPath, "lib"));
        module.IncludeDirs.Add(Path.Combine(QtPath, "include"));
        module.IncludeDirs.Add(Path.Combine(QtPath, "include", moduleName));

        if (moduleName == "QtCore")
        {
            module.Headers.Add("qobject.h");
            module.Headers.Add("qcoreapplication.h");
            //module.Headers.Add(moduleName);
        }

        module.Libraries.Add(libName);
    }

    public void SetupPasses(Driver driver)
    {
        driver.Context.TranslationUnitPasses.AddPass(new UseWhitelistPass());
    }

    public void Preprocess(Driver driver, ASTContext ctx) { }

    public void Postprocess(Driver driver, ASTContext ctx) { }
}
