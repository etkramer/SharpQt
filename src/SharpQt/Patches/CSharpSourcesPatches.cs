using CppSharp.Generators.CSharp;
using HarmonyLib;
using CppSharp.AST;

namespace SharpQt.Patches;

[HarmonyPatch(typeof(CSharpSources))]
[HarmonyPatch("GenerateClassInternalsFields")]
static class GenerateClassInternalsFieldsPatch
{
    static bool Prefix(CSharpSources __instance, Class @class, bool sequentialLayout)
    {
        return false;
    }
}

[HarmonyPatch(typeof(CSharpSources))]
[HarmonyPatch("GetClassInternalHead")]
static class GetClassInternalHeadPatch
{
    static void Postfix(ref string __result)
    {
        __result = __result.Replace("public", "internal");
    }
}

[HarmonyPatch(typeof(CSharpSources))]
[HarmonyPatch("GenerateClassSpecifier")]
static class GenerateClassSpecifierPatch
{
    // From https://github.com/mono/CppSharp/blob/9071cd2a591d5dcb5c584d2774f60905db12ce56/src/Generator/Generators/CSharp/CSharpSources.cs#L750. We could also do this with a transpiler patch.
    static bool Prefix(CSharpSources __instance, Class @class)
    {
        // private classes must be visible to because the internal structs can be used in dependencies
        // the proper fix is InternalsVisibleTo
        var keywords = new List<string>
        {
            @class.Access switch
            {
                AccessSpecifier.Protected => "protected internal",
                AccessSpecifier.Internal => "internal",
                _ => "public"
            }
        };

        var isBindingGen = __instance.GetType() == typeof(CSharpSources);
        if (isBindingGen)
            keywords.Add("unsafe");

        if (@class.IsAbstract)
            keywords.Add("abstract");

        if (@class.IsStatic)
            keywords.Add("static");

        // This token needs to directly precede the "class" token.
        keywords.Add("partial");

        keywords.Add(@class.IsInterface ? "interface" : (@class.IsValueType ? "struct" : "class"));
        keywords.Add(@class.Name);

        __instance.Write(string.Join(" ", keywords));
        if (@class.IsDependent && @class.TemplateParameters.Any())
            __instance.Write(
                $"<{string.Join(", ", @class.TemplateParameters.Select(p => p.Name))}>"
            );

        var bases = new List<string>();

        if (@class.NeedsBase)
        {
            foreach (
                var @base in @class.Bases.Where(
                    b => b.IsGenerated && b.IsClass && b.Class.IsGenerated
                )
            )
            {
                var printedBase = __instance.GetBaseClassTypeName(@base);
                bases.Add(printedBase);
            }
        }

        if (@class.IsGenerated && isBindingGen && NeedsDispose(@class) && !@class.IsOpaque)
        {
            bases.Add("IDisposable");
        }

        if (bases.Count > 0 && !@class.IsStatic)
            __instance.Write(" : {0}", string.Join(", ", bases));

        return false;
    }

    static bool NeedsDispose(Class @class)
    {
        return @class.IsRefType
            || @class.IsValueType
                && (@class.GetConstCharFieldProperties().Any() || @class.HasNonTrivialDestructor);
    }
}
