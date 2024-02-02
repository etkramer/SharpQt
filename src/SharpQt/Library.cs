using System.Reflection;
using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Passes;
using CppSharp.Parser;
using HarmonyLib;
using SharpQt.Passes;

namespace SharpQt;

public class Library(string QtPath, string OutPath, IEnumerable<string> ModuleNames) : ILibrary
{
    // Note: if a whitelisted type inherits from another type, its parent also needs to be whitelisted. Could probably do this automatically in the future.
    static readonly HashSet<string> whitelist = ["QObject", "QCoreApplication", "QGuiApplication", "QWindow", "QPoint", "QApplication", "QWidget", "WindowType"];

    public void Setup(Driver driver)
    {
        // Apply patches for CppSharp.
        var harmony = new Harmony("com.sharpqt.patch");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        driver.Options.GenerateDefaultValuesForArguments = true;
        driver.Options.GenerateDeprecatedDeclarations = false;
        driver.Options.GenerateSequentialLayout = false;
        driver.Options.GenerateFinalizers = false;
        driver.Options.GeneratorKind = GeneratorKind.CSharp;
        driver.Options.MarshalCharAsManagedChar = true;
        driver.Options.MarshalConstCharArrayAsString = false;
        driver.Options.OutputDir = OutPath;

#if DEBUG
        driver.Options.GenerateDebugOutput = true;
#endif

        driver.ParserOptions.EnableRTTI = true;
        driver.ParserOptions.LanguageVersion = LanguageVersion.CPP14_GNU;
        driver.ParserOptions.SetupMSVC(VisualStudioVersion.VS2022);
        driver.ParserOptions.Setup(TargetPlatform.Windows);

        driver.ParserOptions.SkipFunctionBodies = true;
        driver.ParserOptions.SkipPrivateDeclarations = true;

        foreach (var moduleName in ModuleNames)
        {
            var shortName = moduleName.Split("Qt")[1];
            var libName = $"Qt5{shortName}";
            var namespaceName = shortName == "Core" ? "Qt" : $"Qt.{shortName}";

            AddModule(driver, moduleName, libName, namespaceName);
        }
    }

    void AddModule(Driver driver, string moduleName, string libName, string namespaceName)
    {
        var module = driver.Options.AddModule(moduleName);
        module.OutputNamespace = namespaceName;
        module.LibraryDirs.Add(Path.Combine(QtPath, "lib"));
        module.IncludeDirs.Add(Path.Combine(QtPath, "include"));
        module.IncludeDirs.Add(Path.Combine(QtPath, "include", moduleName));

        module.Headers.Add(moduleName);
        module.Libraries.Add(libName);
    }

    public void SetupPasses(Driver driver)
    {
        var passes = driver.Context.TranslationUnitPasses;

        passes.AddPass(new UseWhitelistPass1());
        passes.AddPass(new UseWhitelistPass2());
        passes.AddPass(new RemapQStringMethodsPass());
        passes.AddPass(new RemoveCharPass());
        passes.AddPass(new RemoveQObjectMembersPass());
        passes.AddPass(new MarkTypedefsInternalPass());

        passes.AddPass(new CheckIgnoredDeclsPass());
    }

    public void Preprocess(Driver driver, ASTContext ctx)
    {
        //ctx.SetClassAsValueType("QPoint");
        //ctx.SetClassAsValueType("QSize");
        //ctx.SetClassAsValueType("QRect");

        foreach (var unit in ctx.TranslationUnits.Where(u => u.IsValid))
        {
            IgnorePrivateDecls(unit);
        }
    }

    public void Postprocess(Driver driver, ASTContext ctx) { }

    static void IgnorePrivateDecls(DeclarationContext unit)
    {
        foreach (var decl in unit.Declarations)
        {
            IgnorePrivateDecls(decl);
        }
    }

    static void IgnorePrivateDecls(Declaration decl)
    {
        if (
            decl.Name is not null
            && (
                decl.Name.StartsWith("Private", StringComparison.Ordinal)
                || decl.Name.EndsWith("Private", StringComparison.Ordinal)
            )
        )
        {
            decl.ExplicitlyIgnore();
        }
        else
        {
            if (decl is DeclarationContext declContext)
            {
                IgnorePrivateDecls(declContext);
            }
        }
    }

    public static bool IsDeclWhitelisted(Declaration decl)
    {
        // Check if decl is whitelisted
        if (whitelist.Contains(decl.OriginalName))
        {
            return true;
        }

        // Recursively check if parent class (or namespace) is whitelisted
        if (decl.OriginalNamespace != null && IsDeclWhitelisted(decl.OriginalNamespace))
        {
            return true;
        }

        return false;
    }
}
