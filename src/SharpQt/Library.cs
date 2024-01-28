﻿using System.Reflection;
using CppSharp;
using CppSharp.AST;
using CppSharp.Generators;
using HarmonyLib;
using SharpQt.Passes;

namespace SharpQt;

public class Library(string QtPath, string OutPath, IEnumerable<string> ModuleNames) : ILibrary
{
    public void Setup(Driver driver)
    {
        // Apply patches for CppSharp.
        var harmony = new Harmony("com.sharpqt.patch");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        driver.Options.GenerateDefaultValuesForArguments = true;
        driver.Options.GenerateDeprecatedDeclarations = false;
        driver.Options.CommentKind = CommentKind.BCPLSlash;
        driver.Options.GeneratorKind = GeneratorKind.CSharp;
        driver.Options.MarshalCharAsManagedChar = false;
        driver.Options.MarshalConstCharArrayAsString = false;
        driver.Options.OutputDir = OutPath;

        // Use LayoutKind.Explicit to allow some members of __Internal structs to be skipped.
        driver.Options.GenerateSequentialLayout = false;

#if DEBUG
        driver.Options.GenerateDebugOutput = true;
#endif

        driver.ParserOptions.EnableRTTI = true;
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

        passes.AddPass(new UseWhitelistPass());
        passes.AddPass(new RemapQStringMethodsPass());
        passes.AddPass(new RemoveCharPass());
        passes.AddPass(new RemoveQObjectMembersPass());
        passes.AddPass(new MarkTypedefsInternalPass());
    }

    public void Preprocess(Driver driver, ASTContext ctx)
    {
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
}
