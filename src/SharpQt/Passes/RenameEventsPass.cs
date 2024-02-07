using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Generators;

namespace CppSharp.Passes;

public class RenameEventsPass : TranslationUnitPass
{
    public override bool VisitMethodDecl(Method method)
    {
        if (
            !method.IsConstructor
            && method.OriginalName.EndsWith("Event")
            && method.Parameters.Count == 1
        )
        {
            method.Name =
                "On"
                + char.ToUpper(method.OriginalName[0])
                + method.OriginalName[1..method.OriginalName.LastIndexOf("Event")];

            method.Parameters[0].Name = "args";
        }

        return true;
    }
}
