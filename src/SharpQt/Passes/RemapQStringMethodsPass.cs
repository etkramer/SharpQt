using CppSharp.AST;
using CppSharp.Passes;

namespace SharpQt.Passes;

class RemapQStringMethodsPass : TranslationUnitPass
{
    public override bool VisitClassDecl(Class decl)
    {
        if (decl.Name == "QString")
        {
            decl.GenerationKind = GenerationKind.Generate;
            decl.Access = AccessSpecifier.Internal; // This is currently a no-op due to this line: https://github.com/mono/CppSharp/blob/9071cd2a591d5dcb5c584d2774f60905db12ce56/src/Generator/Generators/CSharp/CSharpSources.cs#L750

            foreach (var method in decl.Methods)
            {
                // Ideally these wouldn't be generated at all - we only care about the P/Invoke part.
                if (
                    method.OriginalName == "constData"
                    || method.OriginalName == "fromUtf16"
                    || method.OriginalName == "size"
                )
                {
                    method.GenerationKind = GenerationKind.Generate;
                    method.Access = AccessSpecifier.Internal;
                }
                else
                {
                    method.ExplicitlyIgnore();
                }
            }
        }

        if (decl.Name == "QByteArray")
        {
            decl.GenerationKind = GenerationKind.Generate;
            decl.Access = AccessSpecifier.Internal; // This is currently a no-op due to this line: https://github.com/mono/CppSharp/blob/9071cd2a591d5dcb5c584d2774f60905db12ce56/src/Generator/Generators/CSharp/CSharpSources.cs#L750

            foreach (var method in decl.Methods)
            {
                // Ideally these wouldn't be generated at all - we only care about the P/Invoke part.
                if (
                    method.OriginalName == "constData"
                    || method.OriginalName == "fromRawData"
                    || method.OriginalName == "size"
                )
                {
                    method.GenerationKind = GenerationKind.Generate;
                    method.Access = AccessSpecifier.Internal;
                }
                else
                {
                    method.ExplicitlyIgnore();
                }
            }
        }

        return base.VisitClassDecl(decl);
    }
}
