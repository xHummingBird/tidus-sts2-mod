using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Tidus.TidusCode.Cards.Ancient;

namespace Tidus.TidusCode.Mechanics;

public static class OverdriveDisplayUi
{
    private sealed class IconConfig
    {
        public string Name { get; init; } = string.Empty;
        public string Scene { get; init; } = string.Empty;
        public Vector2 Position { get; init; }
        public Func<CardModel, bool> ShouldShow { get; init; } =
            _ => true;
    }

    private static readonly Dictionary<string, PackedScene> Cache = new();

    private static readonly IconConfig[] Icons =
    {
        new()
        {
            Name = "JechtShotTwo_UI",
            Scene = "res://Tidus/scenes/jecht_shot_two.tscn",
            Position = new Vector2(75f, -205f),
            ShouldShow = _ => true
        },

        new()
        {
            Name = "EnergyRain_UI",
            Scene = "res://Tidus/scenes/energy_rain.tscn",
            Position = new Vector2(75f, -165f),
            ShouldShow = _ => true
        },

        new()
        {
            Name = "BlitzAce_UI",
            Scene = "res://Tidus/scenes/blitz_ace.tscn",
            Position = new Vector2(75f, -125f),
            ShouldShow = _ => true
        }
    };

    public static void EnsureAndRefresh(NCard cardNode)
    {
        if (cardNode == null)
            return;

        var model = cardNode.Model;
        var body = cardNode.Body;

        if (model == null || body == null)
            return;

        if (model is not Overdrive)
        {
            HideAll(body);
            return;
        }

        foreach (var icon in Icons)
        {
            EnsureSingleIcon(body, model, icon);
        }
    }

    private static void EnsureSingleIcon(
        Control body,
        CardModel model,
        IconConfig config)
    {
        var node = body.GetNodeOrNull<Control>(config.Name);

        bool shouldShow;

        try
        {
            shouldShow = config.ShouldShow(model);
        }
        catch (Exception exception)
        {
            GD.PushWarning(
                $"[Tidus Overdrive Display UI] " +
                $"ShouldShow failed for {config.Name}: {exception}");

            shouldShow = false;
        }

        if (!shouldShow)
        {
            if (node != null)
                node.Visible = false;

            return;
        }

        if (node == null)
        {
            var scene = GetScene(config.Scene);

            if (scene == null)
            {
                GD.PushError(
                    $"[Tidus Overdrive Display UI] " +
                    $"Failed to load {config.Scene}");

                return;
            }

            node = scene.Instantiate<Control>();
            node.Name = config.Name;

            // These icons are visual only.
            node.MouseFilter = Control.MouseFilterEnum.Ignore;

            SetChildControlsToIgnore(node);

            body.AddChild(node);
            body.MoveChild(node, body.GetChildCount() - 1);

            node.ZIndex = 0;
        }

        node.Visible = true;
        node.Position = config.Position;
    }

    private static void HideAll(Control body)
    {
        foreach (var icon in Icons)
        {
            var node = body.GetNodeOrNull<Control>(icon.Name);

            if (node != null)
                node.Visible = false;
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
        if (Cache.TryGetValue(path, out var cachedScene))
            return cachedScene;

        var loadedScene = GD.Load<PackedScene>(path);

        if (loadedScene != null)
        {
            Cache[path] = loadedScene;
        }
        else
        {
            GD.PushError(
                $"[Tidus Overdrive Display UI] " +
                $"Could not load scene: {path}");
        }

        return loadedScene;
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class TidusOverdriveDisplayUiReady
{
    public static void Postfix(NCard __instance)
    {
        if (__instance == null)
            return;

        __instance.ModelChanged += _ =>
        {
            Callable.From(() =>
            {
                try
                {
                    OverdriveDisplayUi.EnsureAndRefresh(__instance);
                }
                catch (Exception exception)
                {
                    GD.PushWarning(
                        $"[Tidus Overdrive Display UI] " +
                        $"Deferred refresh failed: {exception}");
                }
            }).CallDeferred();
        };

        Callable.From(() =>
        {
            try
            {
                OverdriveDisplayUi.EnsureAndRefresh(__instance);
            }
            catch (Exception exception)
            {
                GD.PushWarning(
                    $"[Tidus Overdrive Display UI] " +
                    $"Initial refresh failed: {exception}");
            }
        }).CallDeferred();
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class TidusOverdriveDisplayUIUpdateVisuals
{
    public static void Postfix(NCard __instance)
    {
        if (__instance == null)
            return;

        try
        {
            OverdriveDisplayUi.EnsureAndRefresh(__instance);
        }
        catch (Exception exception)
        {
            GD.PushWarning(
                $"[Tidus Overdrive Display UI] " +
                $"UpdateVisuals refresh failed: {exception}");
        }
    }
}