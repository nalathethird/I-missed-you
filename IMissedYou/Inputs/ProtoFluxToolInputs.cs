using FrooxEngine;
using Renderite.Shared;

namespace IMissedYou.Inputs;

public class ProtoFluxToolInputs : SidedInputGroup
{
    public readonly DigitalAction NodeBrowser;

    public readonly DigitalAction Overview;

    public override string Key => "ProtoFluxTool";

    public override string Name => "ProtoFlux Tool";

    public override string Description => "Keyboard shortcuts for the ProtoFlux Tool";

    public ProtoFluxToolInputs(Chirality side)
        : base(side)
    {
    }
}
