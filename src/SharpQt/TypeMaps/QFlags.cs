using CppSharp.AST;
using CppSharp.AST.Extensions;
using CppSharp.Generators.CSharp;
using CppSharp.Types;
using Type = CppSharp.AST.Type;

namespace SharpQt.TypeMaps;

[TypeMap("QFlags")]
public class QFlags : TypeMap
{
    public override Type? CSharpSignatureType(TypePrinterContext ctx) => GetEnumType(ctx.Type);

    public override void CSharpMarshalToNative(CSharpMarshalContext ctx)
    {
        if (ctx.Parameter.Type.Desugar().IsAddress())
        {
            ctx.Return.Write("new global::System.IntPtr(&{0})", ctx.Parameter.Name);
        }
        else
        {
            ctx.Return.Write(ctx.Parameter.Name);
        }
    }

    public override void CSharpMarshalToManaged(CSharpMarshalContext ctx)
    {
        if (ctx.ReturnType.Type.Desugar().IsAddress())
        {
            var finalType = ctx.ReturnType.Type.GetFinalPointee() ?? ctx.ReturnType.Type;
            var enumType = GetEnumType(finalType);
            ctx.Return.Write($"*({enumType}*) {ctx.ReturnVarName}");
        }
        else
        {
            ctx.Return.Write(ctx.ReturnVarName);
        }
    }

    public override string CSharpConstruct() => "";

    public override bool IsIgnored =>
        GetEnumType(Type).TryGetDeclaration<Declaration>(out var decl)
        && !Library.IsDeclWhitelisted(decl);

    private static Type? GetEnumType(Type mappedType)
    {
        var type = mappedType.Desugar();

        var classTemplateSpecialization =
            (type is TemplateSpecializationType templateSpecializationType)
                ? templateSpecializationType.GetClassTemplateSpecialization()
                : (type as TagType)?.Declaration as ClassTemplateSpecialization;

        return classTemplateSpecialization?.Arguments[0].Type.Type;
    }
}
