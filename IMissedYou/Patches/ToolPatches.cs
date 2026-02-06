using System.Reflection;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using IMissedYou.Inputs;
using Renderite.Shared;

namespace IMissedYou.Patches;

[HarmonyPatch(typeof(Tool))]
internal static class ToolPatches
{
    [HarmonyPatch("OnEquipped")]
    [HarmonyPostfix]
    private static void OnEquipped_Postfix(Tool __instance)
    {
        if (__instance is not MaterialTool materialTool)
            return;

        if (materialTool.ActiveHandler?.Side.Value is not Chirality side)
            return;

        var inputs = new MaterialToolInputs(side);
        IMissedYouMod.MaterialToolInputsTable.AddOrUpdate(materialTool, inputs);
        materialTool.Input.RegisterInputGroup(inputs, materialTool);
    }

    [HarmonyPatch("OnDequipped")]
    [HarmonyPostfix]
    private static void OnDequipped_Postfix(Tool __instance)
    {
        if (__instance is not MaterialTool materialTool)
            return;

        if (!IMissedYouMod.MaterialToolInputsTable.TryGetValue(materialTool, out var inputs))
            return;

        materialTool.Input.UnregisterInputGroup(ref inputs);
        IMissedYouMod.MaterialToolInputsTable.Remove(materialTool);
    }

    [HarmonyPatch("Update", typeof(float), typeof(float2), typeof(Digital), typeof(Digital))]
    [HarmonyPostfix]
    private static void Update_Postfix(Tool __instance)
    {
        if (__instance is not MaterialTool materialTool)
            return;

        if (!IMissedYouMod.MaterialToolInputsTable.TryGetValue(materialTool, out var inputs))
            return;

        if (inputs.Inspector.Pressed)
        {
            var editMethod = typeof(MaterialTool).GetMethod(
                "EditCurrentMaterial",
                BindingFlags.NonPublic | BindingFlags.Instance);
            editMethod?.Invoke(materialTool, null);
        }
    }
}
[HarmonyPatch(typeof(ProtoFluxTool))]
internal static class ProtoFluxToolPatches
{
    [HarmonyPatch("OnEquipped")]
    [HarmonyPostfix]
    private static void OnEquipped_Postfix(ProtoFluxTool __instance)
    {
        if (__instance.ActiveHandler?.Side.Value is not Chirality side)
            return;

        var inputs = new ProtoFluxToolInputs(side);
        IMissedYouMod.ProtoFluxToolInputsTable.AddOrUpdate(__instance, inputs);
        __instance.Input.RegisterInputGroup(inputs, __instance);
    }
    [HarmonyPatch("OnDequipped")]
    [HarmonyPostfix]
    private static void OnDequipped_Postfix(ProtoFluxTool __instance)
    {
        if (!IMissedYouMod.ProtoFluxToolInputsTable.TryGetValue(__instance, out var inputs))
            return;

        __instance.Input.UnregisterInputGroup(ref inputs);
        IMissedYouMod.ProtoFluxToolInputsTable.Remove(__instance);
    }
    [HarmonyPatch("Update", typeof(float), typeof(float2), typeof(Digital), typeof(Digital))]
    [HarmonyPostfix]
    private static void Update_Postfix(ProtoFluxTool __instance)
    {
        if (!IMissedYouMod.ProtoFluxToolInputsTable.TryGetValue(__instance, out var inputs))
            return;

        if (inputs.NodeBrowser.Pressed)
        {
            OpenNodeBrowser(__instance);
        }

        if (inputs.Overview.Pressed)
        {
            ToggleOverviewMode(__instance);
        }
    }

    private static void OpenNodeBrowser(ProtoFluxTool tool)
    {
        var localUserSpace = tool.LocalUser?.Root?.Slot;
        if (localUserSpace == null)
            return;

        var slot = localUserSpace.AddSlot("Node Browser");
        slot.PositionInFrontOfUser(float3.Backward);
        var selector = slot.AttachComponent<ComponentSelector>();
        selector.SetupUI(new LocaleString("ProtoFlux.UI.NodeBrowser.Title", null, true, true, null), ComponentSelector.DEFAULT_SIZE);
        selector.BuildUI(ProtoFluxHelper.PROTOFLUX_ROOT, genericType: false, null, doNotGenerateBack: true);

        var onNodeTypeSelectedMethod = AccessTools.Method(typeof(ProtoFluxTool), "OnNodeTypeSelected");
        if (onNodeTypeSelectedMethod != null)
        {
            var del = Delegate.CreateDelegate(typeof(Action<ComponentSelector, Type>), tool, onNodeTypeSelectedMethod);
            selector.ComponentSelected.TrySet(del);
        }

        var isNodeComponentMethod = AccessTools.Method(typeof(ProtoFluxTool), "IsNodeComponent");
        if (isNodeComponentMethod != null)
        {
            var del = Delegate.CreateDelegate(typeof(Func<Type, bool>), tool, isNodeComponentMethod);
            selector.ComponentFilter.TrySet(del);
        }

        var prefillMethod = AccessTools.Method(typeof(ProtoFluxTool), "PrefillGenericArgument");
        if (prefillMethod != null)
        {
            var del = Delegate.CreateDelegate(typeof(Func<Type, Type?>), tool, prefillMethod);
            selector.GenericArgumentPrefiller.TrySet(del);
        }

        var destroyProxy = tool.DestroyWhenDestroyed(selector.Slot);
        destroyProxy.Persistent = false;
        slot.DestroyWhenDestroyed(destroyProxy);

        tool.ActiveHandler?.CloseContextMenu();
    }

    private static void ToggleOverviewMode(ProtoFluxTool tool)
    {
        var overviewProperty = typeof(ProtoFluxTool).GetProperty(
            "OverviewMode",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (overviewProperty != null)
        {
            var currentValue = (bool)(overviewProperty.GetValue(tool) ?? false);
            overviewProperty.SetValue(tool, !currentValue);
        }
    }
}

[HarmonyPatch(typeof(KeyboardAndMouseBindingGenerator))]
internal static class KeyboardAndMouseBindingGeneratorPatches
{
    [HarmonyPatch("Bind")]
    [HarmonyPrefix]
    private static bool Bind_Prefix(InputGroup group)
    {
        if (group is MaterialToolInputs materialInputs)
        {
            materialInputs.Inspector.AddBinding(
                InputNode.PrimarySecondary(
                    InputNode.Key(IMissedYouMod.ConfiguredMaterialInspectorKey),
                    null));
            return false;
        }

        if (group is ProtoFluxToolInputs protoFluxInputs)
        {
            protoFluxInputs.NodeBrowser.AddBinding(
                InputNode.PrimarySecondary(
                    InputNode.Key(IMissedYouMod.ConfiguredProtoFluxNodeBrowserKey),
                    null));
            protoFluxInputs.Overview.AddBinding(
                InputNode.PrimarySecondary(
                    InputNode.Key(IMissedYouMod.ConfiguredProtoFluxOverviewKey),
                    null));
            return false;
        }

        return true;
    }
}
