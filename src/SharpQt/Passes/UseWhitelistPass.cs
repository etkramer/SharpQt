using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Extensions;
using CppSharp.Passes;

namespace SharpQt.Passes;

class UseWhitelistPass : TranslationUnitPass
{
    // Note: if a whitelisted type inherits from another type, its parent also needs to be whitelisted. Could probably do this automatically in the future.
    readonly HashSet<string> whitelist = ["QObject", "QCoreApplication", "QGuiApplication"];

    // TODO: Mark these internal instead of including them in the API. We only need these because they're data members in another type's __Internal struct.
    readonly HashSet<string> whitelistInternal = ["QScopedPointer", "QExplicitlySharedDataPointer"];

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

        // Ignore methods from internal whitelisted types.
        if (decl.IsNativeMethod() && whitelistInternal.Contains(decl.Namespace.OriginalName))
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
