using CppSharp.AST;
using CppSharp.Passes;
using HarmonyLib;

namespace SharpQt.Patches;

[HarmonyPatch(typeof(MultipleInheritancePass))]
static class MultipleInheritancePassPatches
{
    [HarmonyPrefix]
    [HarmonyPatch("GetNewInterface")]
    static bool GetNewInterfacePrefix(
        MultipleInheritancePass __instance,
        ref string name,
        Class @base
    )
    {
        // Avoid double prefix when creating interfaces for Qt types (i.e. IPaintDevice over IQPaintDevice)
        if (name.StartsWith("IQ"))
        {
            name = $"I{name[2..]}";
        }

        return true;
    }
}
