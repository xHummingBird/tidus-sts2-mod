using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Tidus.TidusCode.Extensions;

public static class TidusAssets
{
    private static PackedScene? _tidusScene;
    private static PackedScene? _vfxScene;

    private const string TidusScenePath = "res://Tidus/scenes/tidus.tscn";
    private const string VfxPath = "res://Tidus/scenes/vfx.tscn";
    

    public static PackedScene? TidusScene
    {
        get
        {
            _tidusScene = LoadOrReload(_tidusScene, TidusScenePath, "Tidus scene");
            return _tidusScene;
        }
    }

    public static PackedScene? VfxScene
    {
        get
        {
            _vfxScene = LoadOrReload(_vfxScene, VfxPath, "VFX");
            return _vfxScene;
        }
    }
    
    private static PackedScene? LoadOrReload(PackedScene? cachedScene, string path, string label)
    {
        if (cachedScene != null && GodotObject.IsInstanceValid(cachedScene))
            return cachedScene;

        GD.Print($"TidusAssets: Loading {label} from {path}");

        var scene = GD.Load<PackedScene>(path);

        if (scene == null)
        {
            GD.PrintErr($"TidusAssets: FAILED to load {label}: {path}");
            return null;
        }

        GD.Print($"TidusAssets: Loaded {label}");
        return scene;
    }

    public static void EnsurePreloaded()
    {
        _ = TidusScene;
        _ = VfxScene;

        GD.Print("TidusAssets: EnsurePreloaded finished");
    }
}

[HarmonyPatch(typeof(Hook), nameof(Hook.AfterActEntered))]
public static class TidusAfterActEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState)
    {
        var player = runState?.Players?.FirstOrDefault();

        if (player?.Character is not Character.Tidus)
            return;

        GD.Print("AfterActEntered: Tidus detected → preloading");

        TidusAssets.EnsurePreloaded();
    }
}


[HarmonyPatch(typeof(Hook), nameof(Hook.AfterRoomEntered))]
public static class TidusAfterRoomEnteredPreloadPatch
{
    [HarmonyPrefix]
    public static void Prefix(IRunState runState, AbstractRoom room)
    {
        var player = runState?.Players?.FirstOrDefault();

        if (player?.Character is not Character.Tidus)
            return;

        GD.Print($"AfterRoomEntered: Tidus detected → preloading. Room = {room.GetType().Name}");

        TidusAssets.EnsurePreloaded();
    }
}
