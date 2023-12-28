using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Passes;

namespace SharpQt.Passes;

/// <summary>
/// Ignores any function containing the native "char" type. Qt has QChar overloads everywhere, which we should use instead to support the same range as C# chars.
/// </summary>
class RemoveCharPass : TranslationUnitPass
{
    public override bool VisitFunctionDecl(Function function)
    {
        var returnType = function.ReturnType.Type.Desugar();
        var paramTypes = function.Parameters.Select(o => o.Type.Desugar());

        if (
            returnType.IsPrimitiveType(PrimitiveType.Char)
            || returnType.IsPointerToPrimitiveType(PrimitiveType.Char)
            || paramTypes.Any(
                type =>
                    type.IsPrimitiveType(PrimitiveType.Char)
                    || type.IsPointerToPrimitiveType(PrimitiveType.Char)
            )
        )
        {
            function.ExplicitlyIgnore();
        }

        return true;
    }
}
