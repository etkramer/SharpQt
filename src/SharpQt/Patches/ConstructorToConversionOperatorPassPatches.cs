using CppSharp.AST;
using CppSharp.Passes;
using HarmonyLib;

namespace SharpQt.Patches;

[HarmonyPatch(typeof(ConstructorToConversionOperatorPass))]
[HarmonyPatch("VisitMethodDecl")]
static class VisitMethodDeclPatch
{
    static bool Prefix(
        ConstructorToConversionOperatorPass __instance,
        Method method,
        ref bool __result
    )
    {
        __result = false;
        return false;
    }
}
