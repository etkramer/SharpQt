using CppSharp.AST;
using CppSharp.Extensions;
using CppSharp.Passes;

namespace SharpQt.Passes;

class UseWhitelistPass1 : TranslationUnitPass
{
    public override bool VisitClassDecl(Class decl)
    {
        decl.ExplicitlyIgnore();

        return base.VisitClassDecl(decl);
    }

    public override bool VisitEnumDecl(Enumeration decl)
    {
        decl.ExplicitlyIgnore();

        return base.VisitEnumDecl(decl);
    }

    public override bool VisitFieldDecl(Field field)
    {
        field.ExplicitlyIgnore();

        return base.VisitFieldDecl(field);
    }

    public override bool VisitFunctionDecl(Function decl)
    {
        // Ignore all free functions
        if (!decl.IsNativeMethod())
        {
            decl.ExplicitlyIgnore();
        }

        return base.VisitFunctionDecl(decl);
    }

    public override bool VisitDeclaration(Declaration decl)
    {
        // Ignore *anything* that's not a class or namespace but exists in the global namespace
        if (decl is not Class && decl is not Namespace && decl.Namespace is TranslationUnit)
        {
            decl.ExplicitlyIgnore();
        }

        return base.VisitDeclaration(decl);
    }
}
