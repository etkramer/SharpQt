using CppSharp.AST;
using CppSharp.Passes;

namespace SharpQt.Passes;

class UseWhitelistPass2 : TranslationUnitPass
{
    public override bool VisitClassDecl(Class decl)
    {
        if (Library.Instance.IsDeclWhitelisted(decl))
        {
            ExplicitlyUnignore(decl);
        }

        return base.VisitClassDecl(decl);
    }

    public override bool VisitEnumDecl(Enumeration decl)
    {
        if (Library.Instance.IsDeclWhitelisted(decl))
        {
            ExplicitlyUnignore(decl);
        }

        return base.VisitEnumDecl(decl);
    }

    void ExplicitlyUnignore(Declaration decl)
    {
        decl.GenerationKind = GenerationKind.Generate;
        if (decl.Namespace != null)
        {
            ExplicitlyUnignore(decl.Namespace);
        }
    }
}
