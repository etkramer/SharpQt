using CppSharp.AST;
using CppSharp.Generators;
using CppSharp.Generators.CSharp;
using CppSharp.Types;
using Type = CppSharp.AST.Type;

namespace SharpQt.TypeMaps;

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
            $@"var __size{ctx.ParameterIndex} = QString.{Helpers.InternalStruct}.Size(new IntPtr(&{ctx.ReturnVarName}));
            var __constData{ctx.ParameterIndex} = QString.{Helpers.InternalStruct}.ConstData(new IntPtr(&{ctx.ReturnVarName}));"
        );

        ctx.Return.Write(
            $"Marshal.PtrToStringUni(__constData{ctx.ParameterIndex}, __size{ctx.ParameterIndex})"
        );
    }

    public override void CSharpMarshalToNative(CSharpMarshalContext ctx)
    {
        ctx.Before.Write(
            $@"var _data{ctx.ParameterIndex} = Marshal.StringToHGlobalUni({ctx.Parameter.Name});
            var _size{ctx.ParameterIndex} = {ctx.Parameter.Name}.Length;
            var _res{ctx.ParameterIndex} = new QString.{Helpers.InternalStruct}();
            QString.{Helpers.InternalStruct}.FromUtf16(new IntPtr(&_res{ctx.ParameterIndex}), (ushort*)(_data{ctx.ParameterIndex}.ToPointer()), _size{ctx.ParameterIndex});"
        );

        ctx.Return.Write($"new IntPtr(&_res{ctx.ParameterIndex})");
        ctx.Cleanup.WriteLine($"Marshal.FreeHGlobal(_data{ctx.ParameterIndex});");
    }
}
