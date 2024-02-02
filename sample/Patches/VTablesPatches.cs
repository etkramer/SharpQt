using HarmonyLib;
using CppSharp.Runtime;
using VTables = CppSharp.Runtime.VTables;
using System.Runtime.InteropServices;

namespace SharpQt.Patches;

[HarmonyPatch(typeof(VTables))]
static class VTablesPatches
{
    [HarmonyPrefix]
    [HarmonyPatch("CloneTable")]
    static unsafe bool CloneTablePrefix(
        List<SafeUnmanagedMemoryHandle> cache,
        IntPtr instance,
        int offset,
        int size,
        int offsetRTTI,
        ref IntPtr* __result
    )
    {
        var sizeInBytes = (size + offsetRTTI) * sizeof(IntPtr);
        var src = (*(IntPtr**)(instance + offset) - offsetRTTI);
        var entries = (IntPtr*)Marshal.AllocHGlobal(sizeInBytes);

        Buffer.MemoryCopy(src, entries, sizeInBytes, sizeInBytes);
        cache.Add(new SafeUnmanagedMemoryHandle((IntPtr)entries, true));
        __result = entries + offsetRTTI;

        return false;
    }
}
