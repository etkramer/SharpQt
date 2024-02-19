using CppSharp;
using CppSharp.Generators;
using CppSharp.Passes;

namespace SharpQt.Passes;

class HideInstanceFieldPass : GeneratorOutputPass
{
    public override void VisitGeneratorOutput(GeneratorOutput output)
    {
        foreach (var block in output.Outputs.SelectMany(o => o.FindBlocks(BlockKind.Field)))
        {
            block.Text.StringBuilder.Replace(
                "public __IntPtr __Instance { get; protected set; }",
                "internal __IntPtr __Instance { get; set; }"
            );
        }

        base.VisitGeneratorOutput(output);
    }
}
