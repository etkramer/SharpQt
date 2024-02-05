using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Generators.CSharp;
using CppSharp.Types;
using Type = CppSharp.AST.Type;

namespace SharpQt.TypeMaps;

[TypeMap("QByteArray")]
class QByteArray : TypeMap
{
    public override Type CSharpSignatureType(TypePrinterContext ctx) => new CustomType("byte[]");

    public override void CSharpMarshalToManaged(CSharpMarshalContext ctx)
    {
        ctx.Before.Write(
            $"var __size{ctx.ParameterIndex} = QByteArray.{Helpers.InternalStruct}.Size(new IntPtr(&{ctx.ReturnVarName}));"
        );
        ctx.Before.Write(
            $"var __constData{ctx.ParameterIndex} = QByteArray.{Helpers.InternalStruct}.ConstData(new IntPtr(&{ctx.ReturnVarName}));"
        );
        ctx.Before.Write($"var __res{ctx.ParameterIndex} = new byte[__size{ctx.ParameterIndex}];");
        ctx.Before.Write(
            $"Marshal.Copy(__constData{ctx.ParameterIndex}, __res{ctx.ParameterIndex}, 0, __size{ctx.ParameterIndex});"
        );

        ctx.Return.Write($"__res{ctx.ParameterIndex}");
    }

    public override void CSharpMarshalToNative(CSharpMarshalContext ctx)
    {
        ctx.Before.Write(
            $"var _size{ctx.ParameterIndex} = {ctx.Parameter.Name}.Length * sizeof(byte);"
        );
        ctx.Before.Write(
            $"var _res{ctx.ParameterIndex} = Marshal.AllocHGlobal(_size{ctx.ParameterIndex});"
        );

        ctx.Before.Write(
            $"Marshal.Copy({ctx.Parameter.Name}, 0, _res{ctx.ParameterIndex}, _size{ctx.ParameterIndex});"
        );

        ctx.Return.Write($"_res{ctx.ParameterIndex}");
        ctx.Cleanup.Write($"Marshal.FreeHGlobal(_res{ctx.ParameterIndex});");
    }
}
