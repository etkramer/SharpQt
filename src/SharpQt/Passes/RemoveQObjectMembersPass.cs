using CppSharp.AST;
using CppSharp.Passes;

// Based on https://github.com/ddobrev/QtSharp/blob/master/QtSharp/RemoveQObjectMembersPass.cs

namespace SharpQt.Passes;

class RemoveQObjectMembersPass : TranslationUnitPass
{
    public override bool VisitClassDecl(Class decl)
    {
        if (AlreadyVisited(decl) || decl.Name == "QObject")
        {
            return false;
        }

        IEnumerable<MacroExpansion> expansions = decl.PreprocessedEntities.OfType<MacroExpansion>();

        if (expansions.Any(e => e.Text == "Q_OBJECT"))
        {
            RemoveQObjectMembers(decl);
        }

        return true;
    }

    private static void RemoveQObjectMembers(Class decl)
    {
        // Every Qt object "inherits" a lot of members via the Q_OBJECT macro.
        // See the define of Q_OBJECT in qobjectdefs.h for a list of the members.
        // We cannot use the Qt defines for disabling the expansion of these
        // because it would mess up with the object layout size.

        RemoveMethodOverloads(decl, "tr");
        RemoveMethodOverloads(decl, "trUtf8");
        RemoveMethodOverloads(decl, "qt_static_metacall");
        RemoveVariables(decl, "staticMetaObject");
    }

    private static void RemoveMethodOverloads(Class decl, string originalName)
    {
        foreach (var method in decl.Methods.Where(m => m.OriginalName == originalName).ToList())
        {
            decl.Methods.Remove(method);
        }
    }

    private static void RemoveVariables(Class decl, string originalName)
    {
        foreach (var variable in decl.Variables.Where(v => v.OriginalName == originalName).ToList())
        {
            variable.ExplicitlyIgnore();
        }
    }
}
