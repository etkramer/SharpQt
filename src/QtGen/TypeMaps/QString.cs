using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Generators.CSharp;
using CppSharp.Types;
using Type = CppSharp.AST.Type;

namespace QtGen.TypeMaps;

[TypeMap("QString")]
class QString : TypeMap
{
    public override Type CSharpSignatureType(TypePrinterContext ctx) =>
        ctx.Kind switch
        {
            TypePrinterContextKind.Managed => new CustomType("string"),
            TypePrinterContextKind.Native => new CustomType("System.IntPtr"),
            TypePrinterContextKind.Normal => new CustomType("Qt.QString"),
            _ => throw new NotImplementedException(Enum.GetName(ctx.Kind))
        };

    public override void CSharpMarshalToManaged(CSharpMarshalContext ctx)
    {
        ctx.Before.Write(
            $"var size = QString.{Helpers.InternalStruct}.Size(new IntPtr(&{ctx.ReturnVarName}));"
        );
        ctx.Before.Write(
            $"var constData = QString.{Helpers.InternalStruct}.ConstData(new IntPtr(&{ctx.ReturnVarName}));"
        );

        ctx.Return.Write("Marshal.PtrToStringUni(constData, size)");
    }

    public override void CSharpMarshalToNative(CSharpMarshalContext ctx)
    {
        ctx.Before.Write($"var data = Marshal.StringToHGlobalUni({ctx.Parameter.Name});");
        ctx.Before.Write($"var res = new QString.{Helpers.InternalStruct}();");
        ctx.Before.Write($"var size = {ctx.Parameter.Name}.Length;");
        ctx.Before.Write(
            $"QString.{Helpers.InternalStruct}.FromUtf16(new IntPtr(&res), (ushort*)(data.ToPointer()), size);"
        );

        ctx.Return.Write("new IntPtr(&res)");

        ctx.Cleanup.WriteLine("Marshal.FreeHGlobal(data);");
    }

    //public override string CSharpConstruct() => "";
}
