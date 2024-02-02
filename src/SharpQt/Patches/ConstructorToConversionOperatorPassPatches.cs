using CppSharp.AST;
using CppSharp.Passes;
using HarmonyLib;

namespace SharpQt.Patches;

[HarmonyPatch(typeof(ConstructorToConversionOperatorPass))]
static class ConstructorToConversionOperatorPassPatches
{
    [HarmonyPrefix]
    [HarmonyPatch("VisitMethodDecl")]
    static bool VisitMethodDeclPrefix(
        ConstructorToConversionOperatorPass __instance,
        Method method,
        ref bool __result
    )
    {
        __result = false;
        return false;
    }
}
