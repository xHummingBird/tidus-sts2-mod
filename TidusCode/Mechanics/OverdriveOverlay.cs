using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Mechanics;

public partial class TidusOverdriveDisplay : Control
{
    public static TidusOverdriveDisplay? Instance
    {
        get;
        private set;
    }

    private const string ScenePath =
        "res://Tidus/scenes/overdrive_display.tscn";

    /*
     * Position of the entire Overdrive display
     * relative to the energy counter.
     */
    private static readonly Vector2 DisplayPosition =
        new(-3f, -75f);

    /*
     * Additional position offset for only the number.
     *
     * Negative X = left
     * Positive X = right
     * Negative Y = up
     * Positive Y = down
     */
    private static readonly Vector2 LabelPositionOffset =
        new(0f, 14f);
    
    private static readonly Texture2D EmptyTexture =
        GD.Load<Texture2D>(
            "res://Tidus/images/charui/overdrive_empty.png");

    private static readonly Texture2D FullTexture =
        GD.Load<Texture2D>(
            "res://Tidus/images/charui/overdrive_full.png");

    private const int MaxOverdrive = 100;

    /*
     * Hover-tip position relative to the display.
     */
    private static readonly Vector2 HoverTipOffset =
        new(-60f, -350f);

    private Control? _display;
    private TextureRect? _overlay;
    private RichTextLabel? _overdriveLabel;

    private Player? _player;

    private IHoverTip? _overdriveHoverTip;

    private int _lastOverdrive =
        int.MinValue;

    public override void _Ready()
    {
        Instance = this;

        Name =
            "TidusOverdriveDisplay";

        MouseFilter =
            MouseFilterEnum.Pass;

        CallDeferred(
            nameof(Setup));
    }

    private async void Setup()
    {
        if (!IsInsideTree())
            return;

        /*
         * Wait for the local combat player.
         */
        for (int i = 0; i < 60; i++)
        {
            var state =
                CombatManager.Instance?
                    .DebugOnlyGetState();

            Player? player =
                state?.Players.FirstOrDefault(
                    currentPlayer =>
                        LocalContext.IsMe(currentPlayer));

            if (player != null)
            {
                if (player.Character is not Character.Tidus)
                {
                    QueueFree();
                    return;
                }

                _player =
                    player;

                break;
            }

            SceneTree? tree =
                GetTree();

            if (tree == null)
                return;

            await ToSignal(
                tree,
                SceneTree.SignalName.ProcessFrame);
        }

        if (_player == null)
        {
            QueueFree();
            return;
        }

        PackedScene? scene =
            GD.Load<PackedScene>(
                ScenePath);

        if (scene == null)
        {
            GD.PushError(
                $"[Tidus Overdrive] Failed to load {ScenePath}");

            QueueFree();
            return;
        }

        _display =
            scene.Instantiate<Control>();

        AddChild(
            _display);

        _display.SetAnchorsPreset(
            LayoutPreset.BottomLeft);

        _display.Position =
            DisplayPosition;

        GetSceneNodes();

        if (!ValidateRequiredNodes())
        {
            QueueFree();
            return;
        }

        ConfigureDisplay();
        ConfigureLabel();
        ConfigureHoverTip();

        ResetDisplayState();
        UpdateDisplay();
    }

    private void GetSceneNodes()
    {
        if (_display == null)
            return;

        _overlay =
            _display.GetNodeOrNull<TextureRect>(
                "Overlay");

        _overdriveLabel =
            _display.GetNodeOrNull<RichTextLabel>(
                "RichTextLabel");
    }

    private bool ValidateRequiredNodes()
    {
        bool valid =
            _display != null &&
            _overlay != null &&
            _overdriveLabel != null;

        if (!valid)
        {
            GD.PushError(
                "[Tidus Overdrive] overdrive_display.tscn must contain " +
                "Overlay and RichTextLabel.");
        }

        return valid;
    }

    private void ConfigureDisplay()
    {
        if (_display == null ||
            _overlay == null)
        {
            return;
        }

        _display.MouseFilter =
            MouseFilterEnum.Stop;

        _overlay.Visible =
            true;

        /*
         * Prevent the image from blocking mouse input
         * to the root Control.
         */
        _overlay.MouseFilter =
            MouseFilterEnum.Ignore;

        _display.MouseEntered +=
            OnOverdriveHovered;

        _display.MouseExited +=
            OnOverdriveUnhovered;
    }
    
    private void PlayGainAnimation()
    {
        if (_overdriveLabel == null)
            return;

        _overdriveLabel.Scale =
            Vector2.One;

        Tween tween =
            CreateTween();

        tween.SetParallel();

        tween.TweenProperty(
            _overdriveLabel,
            "scale",
            new Vector2(1.35f, 1.35f),
            0.08f);

        tween.TweenProperty(
                _overdriveLabel,
                "scale",
                Vector2.One,
                0.12f)
            .SetDelay(0.08f);
    }

    private void ConfigureLabel()
    {
        if (_overdriveLabel == null)
            return;

        _overdriveLabel.Visible =
            true;

        _overdriveLabel.MouseFilter =
            MouseFilterEnum.Ignore;

        Font? font =
            GD.Load<Font>(
                "res://themes/kreon_bold_shared.tres");

        if (font != null)
        {
            _overdriveLabel.AddThemeFontOverride(
                "font",
                font);

            _overdriveLabel.AddThemeFontOverride(
                "normal_font",
                font);
        }

        _overdriveLabel.AddThemeFontSizeOverride(
            "normal_font_size",
            24);

        _overdriveLabel.AddThemeColorOverride(
            "default_color",
            Colors.White);

        _overdriveLabel.AddThemeColorOverride(
            "font_outline_color",
            new Color(
                0.15f,
                0.08f,
                0.02f));

        _overdriveLabel.AddThemeConstantOverride(
            "outline_size",
            7);

        _overdriveLabel.BbcodeEnabled =
            true;

        _overdriveLabel.FitContent =
            true;

        /*
         * Adjust this using LabelPositionOffset.
         */
        _overdriveLabel.Position +=
            LabelPositionOffset;
    }

    private void ConfigureHoverTip()
    {
        _overdriveHoverTip =
            TidusStaticHoverTips.Overdrive;
    }

    public override void _Process(
        double delta)
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_player == null ||
            _overdriveLabel == null)
        {
            return;
        }

        int overdrive =
            OverdriveManager.GetOverdrive(
                _player);

        if (overdrive == _lastOverdrive)
            return;

        SetOverdriveDisplay(
            overdrive);
    }

    private void SetOverdriveDisplay(
        int overdrive)
    {
        int previous = _lastOverdrive;
       
        _lastOverdrive =
            Mathf.Max(
                overdrive,
                0);
      
        if (_lastOverdrive > previous)
        {
            PlayGainAnimation();
        }

        if (_overlay == null ||
            _overdriveLabel == null)
        {
            return;
        }
        
        bool isFull =
            _lastOverdrive >= MaxOverdrive;

        if (isFull)
        {
            _overlay.Texture =
                FullTexture;

            _overdriveLabel.Visible =
                false;
        }
        else
        {
            _overlay.Texture =
                EmptyTexture;

            _overdriveLabel.Visible =
                true;

            _overdriveLabel.Text =
                $"[center]{_lastOverdrive}[/center]";
        }
    }

    private void OnOverdriveHovered()
    {
        if (_display == null ||
            _overdriveHoverTip == null)
        {
            return;
        }

        NHoverTipSet.Clear();

        Control? tip =
            NHoverTipSet.CreateAndShow(
                _display,
                _overdriveHoverTip);

        if (tip == null)
            return;

        tip.GlobalPosition =
            _display.GlobalPosition +
            HoverTipOffset;

        tip.MouseFilter =
            MouseFilterEnum.Ignore;
    }

    private void OnOverdriveUnhovered()
    {
        if (_display == null)
            return;

        NHoverTipSet.Remove(
            _display);
    }

    private void ResetDisplayState()
    {
        _lastOverdrive =
            int.MinValue;

        if (_overlay != null)
        {
            _overlay.Texture =
                EmptyTexture;
        }

        if (_overdriveLabel != null)
        {
            _overdriveLabel.Visible =
                true;

            _overdriveLabel.Text =
                "[center]0[/center]";
        }
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance =
                null;
        }

        if (_display != null)
        {
            _display.MouseEntered -=
                OnOverdriveHovered;

            _display.MouseExited -=
                OnOverdriveUnhovered;

            NHoverTipSet.Remove(
                _display);
        }

        _overdriveHoverTip =
            null;

        _player =
            null;

        _overlay =
            null;

        _overdriveLabel =
            null;

        _display =
            null;
    }
}

[HarmonyPatch(
    typeof(NEnergyCounter),
    nameof(NEnergyCounter._Ready))]
public static class TidusOverdriveDisplayOverlayPatch
{
    public static void Postfix(
        NEnergyCounter __instance)
    {
        if (__instance == null)
            return;

        if (!GodotObject.IsInstanceValid(
                __instance))
        {
            return;
        }

        var state =
            CombatManager.Instance?
                .DebugOnlyGetState();

        Player? player =
            state?.Players.FirstOrDefault(
                currentPlayer =>
                    LocalContext.IsMe(currentPlayer));

        if (player?.Character is not Character.Tidus)
            return;

        if (__instance.GetNodeOrNull<TidusOverdriveDisplay>(
                "TidusOverdriveDisplay") != null)
        {
            return;
        }

        __instance.AddChild(
            new TidusOverdriveDisplay
            {
                Name =
                    "TidusOverdriveDisplay"
            });
    }
}