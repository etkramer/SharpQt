using CppSharp.Generators.CSharp;
using HarmonyLib;
using CppSharp.AST;

namespace SharpQt.Patches;

[HarmonyPatch(typeof(CSharpSources))]
[HarmonyPatch("GenerateClassInternalsFields")] // if possible use nameof() here
static class CSharpSourcesPatches
{
    static bool Prefix(CSharpSources __instance, Class @class, bool sequentialLayout)
    {
        return false;
    }
}
