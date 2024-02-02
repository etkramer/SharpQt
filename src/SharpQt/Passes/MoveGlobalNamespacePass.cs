using CppSharp.AST;
using CppSharp.Passes;

namespace SharpQt.Passes;

class MoveGlobalNamespacePass : TranslationUnitPass
{
    public override bool VisitDeclaration(Declaration decl)
    {
        if (decl.Namespace?.Name == "Qt" && !decl.Ignore)
        {
            var currentNamespace = decl.Namespace;
            var targetNamespace = decl.Namespace.Namespace;

            decl.Namespace = targetNamespace;
            currentNamespace.Declarations.Remove(decl);
            targetNamespace.Declarations.Add(decl);
        }

        return base.VisitDeclaration(decl);
    }
}
