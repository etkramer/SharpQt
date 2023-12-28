using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Extensions;
using CppSharp.Passes;

namespace QtGen.Passes;

class UseWhitelistPass : TranslationUnitPass
{
    readonly HashSet<string> whitelist = ["QObject", "QCoreApplication"];
    readonly HashSet<string> whitelistInternal = ["QScopedPointer"]; // TODO: Mark these internal instead of including them in the API.

    public override bool VisitClassDecl(Class decl)
    {
        if (!IsWhitelisted(decl))
        {
            decl.ExplicitlyIgnore();
        }

        return base.VisitClassDecl(decl);
    }

    public override bool VisitEnumDecl(Enumeration decl)
    {
        if (!IsWhitelisted(decl))
        {
            decl.ExplicitlyIgnore();
        }

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

    bool IsWhitelisted(Declaration decl)
    {
        // Check if decl is whitelisted
        if (whitelist.Contains(decl.OriginalName) || whitelistInternal.Contains(decl.OriginalName))
        {
            return true;
        }

        // Recursively check if parent class (or namespace) is whitelisted
        if (decl.Namespace != null && IsWhitelisted(decl.Namespace))
        {
            return true;
        }

        return false;
    }
}
