using CppSharp.AST;
using CppSharp.Passes;

namespace SharpQt.Passes;

class MarkTypedefsInternalPass : TranslationUnitPass
{
    public override bool VisitTypedefNameDecl(TypedefNameDecl typedef)
    {
        typedef.Access = AccessSpecifier.Internal;

        return base.VisitTypedefNameDecl(typedef);
    }
}
