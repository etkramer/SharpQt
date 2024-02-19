using CppSharp.AST;
using CppSharp.AST.Extensions;
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
        ctx.Before.WriteLine(
            @$"var __ptr{ctx.ParameterIndex} = {(ctx.ReturnType.Type.IsReference() ? ctx.ReturnVarName : $"new IntPtr(&{ctx.ReturnVarName})")};
            var __size{ctx.ParameterIndex} = QByteArray.{Helpers.InternalStruct}.Size(__ptr{ctx.ParameterIndex});
            var __constData{ctx.ParameterIndex} = QByteArray.{Helpers.InternalStruct}.ConstData(__ptr{ctx.ParameterIndex});
            var __res{ctx.ParameterIndex} = new byte[__size{ctx.ParameterIndex}];
            Marshal.Copy(__constData{ctx.ParameterIndex}, __res{ctx.ParameterIndex}, 0, __size{ctx.ParameterIndex});"
        );

        ctx.Return.Write($"__res{ctx.ParameterIndex}");
    }

    public override void CSharpMarshalToNative(CSharpMarshalContext ctx)
    {
        // NOTE: FromRawData() does not create a permanant copy of its input. This could become a problem if the managed array is cleaned up or moved.
        ctx.Before.WriteLine(
            $@"var _size{ctx.ParameterIndex} = {ctx.Parameter.Name}.Length;
            var _res{ctx.ParameterIndex} = new QByteArray.{Helpers.InternalStruct}();
            fixed (byte* _data{ctx.ParameterIndex} = {ctx.Parameter.Name})
            {{
                QByteArray.{Helpers.InternalStruct}.FromRawData(new IntPtr(&_res{ctx.ParameterIndex}), new IntPtr(_data{ctx.ParameterIndex}), _size{ctx.ParameterIndex});
            }}
            "
        );

        ctx.Return.Write($"new IntPtr(&_res{ctx.ParameterIndex})");
    }
}
