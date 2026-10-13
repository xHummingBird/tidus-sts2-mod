using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Tidus.TidusCode.Cards.Ancient;

namespace Tidus.TidusCode.Mechanics;

public static class OverdriveCardPatch
{
    private sealed class IconConfig
    {
        public string ContainerName { get; }
        public string ScenePath { get; }
        public Vector2 Position { get; }
        public Func<CardModel, IHoverTip> HoverTipFactory { get; }
        public Func<CardModel, bool> ShouldShow { get; }

        public IconConfig(
            string containerName,
            string scenePath,
            Vector2 position,
            Func<CardModel, IHoverTip> hoverTipFactory,
            Func<CardModel, bool>? shouldShow = null)
        {
            ContainerName = containerName;
            ScenePath = scenePath;
            Position = position;
            HoverTipFactory = hoverTipFactory;
            ShouldShow = shouldShow ?? (_ => true);
        }
    }

    private static readonly Dictionary<string, PackedScene> SceneCache = new();

    private static readonly IconConfig[] Icons =
    {
        new(
            "JechtShotTwoIconContainer",
            "res://Tidus/scenes/jecht_shot_two.tscn",
            new Vector2(225f, 8f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<JechtShotTwo>(true)
                : HoverTipFactory.FromCard<JechtShotTwo>()
        ),

        new(
            "EnergyRainIconContainer",
            "res://Tidus/scenes/energy_rain.tscn",
            new Vector2(225f, 48f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<EnergyRain>(true)
                : HoverTipFactory.FromCard<EnergyRain>()
        ),

        new(
            "BlitzAceIconContainer",
            "res://Tidus/scenes/blitz_ace.tscn",
            new Vector2(225f, 88f),
            model => model.IsUpgraded
                ? HoverTipFactory.FromCard<BlitzAce>(true)
                : HoverTipFactory.FromCard<BlitzAce>()
        )
    };

    public static void EnsureAndRefresh(NHandCardHolder holder)
    {
        var model = holder.CardNode?.Model;
        var hitbox = holder.Hitbox;

        if (hitbox == null)
            return;

        if (model is not Overdrive)
        {
            HideAll(hitbox);
            return;
        }

        foreach (var config in Icons)
        {
            EnsureSingleIcon(holder, hitbox, config);
        }
    }

    private static void EnsureSingleIcon(
        NHandCardHolder holder,
        Control hitbox,
        IconConfig config)
    {
        var model = holder.CardNode?.Model;

        if (model == null)
            return;

        var container = hitbox.GetNodeOrNull<Control>(config.ContainerName);
        bool shouldShow = config.ShouldShow(model);

        if (!shouldShow)
        {
            if (container != null)
                container.Visible = false;

            return;
        }

        if (container == null)
        {
            var scene = GetScene(config.ScenePath);

            if (scene == null)
            {
                GD.PushError(
                    $"[Tidus Overdrive Card] Failed to load {config.ScenePath}");
                return;
            }

            container = scene.Instantiate<Control>();
            container.Name = config.ContainerName;

            // This is the only interactive node in the icon scene.
            container.MouseFilter = Control.MouseFilterEnum.Pass;

            hitbox.AddChild(container);
            hitbox.MoveChild(container, hitbox.GetChildCount() - 1);

            // Prevent child controls from stealing hover.
            SetChildControlsToIgnore(container);

            var capturedContainer = container;
            var capturedHolder = holder;
            var capturedConfig = config;

            container.MouseEntered += () =>
                OnHovered(
                    capturedContainer,
                    capturedHolder,
                    capturedConfig);

            container.MouseExited += () =>
                OnUnhovered(capturedHolder);
        }

        container.Visible = true;
        container.Position = config.Position;
    }

    private static void OnHovered(
        Control owner,
        NHandCardHolder holder,
        IconConfig config)
    {
        var model = holder.CardNode?.Model;
        var cardNode = holder.CardNode;

        if (model == null || cardNode == null)
            return;

        // Remove any previous icon hover tip owned by this card.
        NHoverTipSet.Remove(cardNode);

        var tip = NHoverTipSet.CreateAndShow(
            cardNode,
            config.HoverTipFactory(model));

        if (tip == null)
            return;

        tip.MouseFilter = Control.MouseFilterEnum.Ignore;

        // Position the hover card to the left of the Overdrive card.
        tip.GlobalPosition =
            cardNode.GlobalPosition + new Vector2(-400f, -200f);
    }

    private static void OnUnhovered(NHandCardHolder holder)
    {
        var cardNode = holder.CardNode;

        if (cardNode != null)
            NHoverTipSet.Remove(cardNode);
    }

    private static void HideAll(Control hitbox)
    {
        foreach (var config in Icons)
        {
            var container =
                hitbox.GetNodeOrNull<Control>(config.ContainerName);

            if (container == null)
                continue;

            container.Visible = false;
        }
    }

    private static void SetChildControlsToIgnore(Node root)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is Control control)
                control.MouseFilter = Control.MouseFilterEnum.Ignore;

            SetChildControlsToIgnore(child);
        }
    }

    private static PackedScene? GetScene(string path)
    {
        if (SceneCache.TryGetValue(path, out var cachedScene))
            return cachedScene;

        var scene = GD.Load<PackedScene>(path);

        if (scene != null)
        {
            SceneCache[path] = scene;
        }
        else
        {
            GD.PushError(
                $"[Tidus Overdrive Card] Could not load scene: {path}");
        }

        return scene;
    }
}

#region Hooks

[HarmonyPatch(typeof(NHandCardHolder), nameof(NHandCardHolder._Ready))]
public static class TidusOverdriveCardPatchReady
{
    public static void Postfix(NHandCardHolder __instance)
    {
        Callable.From(() =>
        {
            try
            {
                OverdriveCardPatch.EnsureAndRefresh(__instance);
            }
            catch (Exception exception)
            {
                GD.PushWarning(
                    $"[Tidus Overdrive Card] Initial refresh failed: {exception}");
            }
        }).CallDeferred();
    }
}

[HarmonyPatch(
    typeof(NHandCardHolder),
    nameof(NHandCardHolder.UpdateCard))]
public static class TidusOverdriveCardPatchUpdateCard
{
    public static void Postfix(NHandCardHolder __instance)
    {
        try
        {
            OverdriveCardPatch.EnsureAndRefresh(__instance);
        }
        catch (Exception exception)
        {
            GD.PushWarning(
                $"[Tidus Overdrive Card] UpdateCard refresh failed: {exception}");
        }
    }
}

#endregion
