using System.Runtime.CompilerServices;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using IMissedYou.Inputs;
using Renderite.Shared;
using ResoniteModLoader;

namespace IMissedYou;

public class IMissedYouMod : ResoniteMod
{
    internal const string VERSION_CONSTANT = "1.0.0";
    public override string Name => "I Missed You";
    public override string Author => "nalathethird";
    public override string Version => VERSION_CONSTANT;
    public override string Link => "https://github.com/nalathethird/I-missed-you";

    private static ModConfiguration? _config;

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<Key> MaterialToolInspectorKey = new(
        "MaterialToolInspectorKey",
        "Key to open material editor in MaterialTool",
        () => Key.I);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<Key> ProtoFluxNodeBrowserKey = new(
        "ProtoFluxNodeBrowserKey",
        "Key to open node browser in ProtoFluxTool",
        () => Key.I);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<Key> ProtoFluxOverviewKey = new(
        "ProtoFluxOverviewKey",
        "Key to toggle overview mode in ProtoFluxTool",
        () => Key.O);

    internal static readonly ConditionalWeakTable<MaterialTool, MaterialToolInputs> MaterialToolInputsTable = new();
    internal static readonly ConditionalWeakTable<ProtoFluxTool, ProtoFluxToolInputs> ProtoFluxToolInputsTable = new();

    internal static Key ConfiguredMaterialInspectorKey => _config?.GetValue(MaterialToolInspectorKey) ?? Key.I;
    internal static Key ConfiguredProtoFluxNodeBrowserKey => _config?.GetValue(ProtoFluxNodeBrowserKey) ?? Key.I;
    internal static Key ConfiguredProtoFluxOverviewKey => _config?.GetValue(ProtoFluxOverviewKey) ?? Key.O;

    public override void OnEngineInit()
    {
        _config = GetConfiguration();
        _config?.Save(true);

        Harmony harmony = new("com.nalathethird.IMissedYou");
        harmony.PatchAll();
    }
}
