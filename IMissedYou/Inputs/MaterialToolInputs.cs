using FrooxEngine;
using Renderite.Shared;

namespace IMissedYou.Inputs;

public class MaterialToolInputs : SidedInputGroup
{
    public readonly DigitalAction Inspector;

    public override string Key => "MaterialTool";

    public override string Name => "Material Tool";

    public override string Description => "Keyboard shortcuts for the Material Tool";

    public MaterialToolInputs(Chirality side)
        : base(side)
    {
    }
}
