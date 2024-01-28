using CppSharp.AST;
using CppSharp.Generators.CSharp;
using CppSharp.Types;
using Type = CppSharp.AST.Type;

// Based on https://github.com/ddobrev/QtSharp/blob/master/QtSharp/QFlags.cs

namespace SharpQt.TypeMaps;

[TypeMap("QChar")]
class QChar : TypeMap
{
    public override Type CSharpSignatureType(TypePrinterContext ctx) => new CustomType("char");

    public override void CSharpMarshalToManaged(CSharpMarshalContext ctx)
    {
        ctx.Return.Write($"*(char*)(&{ctx.ReturnVarName})");
    }

    public override void CSharpMarshalToNative(CSharpMarshalContext ctx)
    {
        ctx.Return.Write(ctx.Parameter.Name);
    }

    public override string CSharpConstruct() => "";
}
