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

    public override bool VisitFunctionDecl(Function decl)
    {
        // Ignore all free functions
        if (!decl.IsNativeMethod())
        {
            decl.ExplicitlyIgnore();
        }

        return base.VisitFunctionDecl(decl);
    }
}
