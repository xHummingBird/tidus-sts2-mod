using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Tidus.TidusCode.Extensions;

public static class BattleTheme
{
    private const string MusicBus =
        "bus:/master/music";

    private const float MusicVolume = 8f;
    private const float FanfareVolume = 8f;
    private const float GameOverVolume = 8f;

    private static bool _vanillaMusicMuted;
    private static float _savedMusicBusVolume = 1f;

    private static AudioStreamPlayer? _battlePlayer;
    private static AudioStreamPlayer? _fanfarePlayer;
    private static AudioStreamPlayer? _gameOverPlayer;

    public static bool IsBattleThemeActive =>
        _vanillaMusicMuted;

    private const string VictoryFanfare =
        "res://Tidus/music/110 Victory!.mp3";

    private const string VictoryFanfareShort =
        "res://Tidus/music/110 Victory!.mp3";

    private const string GameOverTheme =
        "res://Tidus/music/111 Game Over.mp3";

    // ============================================================
    // NORMAL
    // ============================================================

    private static readonly string[] Act1Normal =
    [
        "res://Tidus/music/109 Battle.mp3",
        "res://Tidus/music/211 Blitz Off.mp3"
    ];

    private static readonly string[] Act2Normal =
    [
        "res://Tidus/music/109 Battle.mp3",
        "res://Tidus/music/211 Blitz Off.mp3",
        "res://Tidus/music/106 Hurry!!.mp3",
        "res://Tidus/music/308 Pursuit.mp3"
    ];

    private static readonly string[] Act3Normal =
    [
        "res://Tidus/music/109 Battle.mp3",
        "res://Tidus/music/211 Blitz Off.mp3",
        "res://Tidus/music/106 Hurry!!.mp3",
        "res://Tidus/music/308 Pursuit.mp3",
        "res://Tidus/music/314 Assault.mp3",
        "res://Tidus/music/408 Someday the Dream Will End.mp3"
    ];

    // ============================================================
    // ELITE
    // ============================================================

    private static readonly string[] Act1Elite =
    [
        "res://Tidus/music/106 Hurry!!.mp3"
    ];

    private static readonly string[] Act2Elite =
    [
        "res://Tidus/music/106 Hurry!!.mp3",
        "res://Tidus/music/116 Enemy Attack.mp3",
        "res://Tidus/music/310 Crisis.mp3"
    ];

    private static readonly string[] Act3Elite =
    [
        "res://Tidus/music/106 Hurry!!.mp3",
        "res://Tidus/music/116 Enemy Attack.mp3",
        "res://Tidus/music/310 Crisis.mp3"
    ];

    // ============================================================
    // BOSS
    // ============================================================

    private static readonly string[] Act1Boss =
    [
        "res://Tidus/music/116 Enemy Attack.mp3",
        "res://Tidus/music/310 Crisis.mp3"
    ];

    private static readonly string[] Act2Boss =
    [
        "res://Tidus/music/410 Challenge.mp3",
        "res://Tidus/music/415 Seymour Battle.mp3"
    ];

    private static readonly string[] Act3Boss =
    [
        "res://Tidus/music/105b Other World (Alternate 2).mp3",
        "res://Tidus/music/417 Aeon Battle.mp3",
        "res://Tidus/music/418 Decisive Battle.mp3"
    ];

    // ============================================================
    // VANILLA MUSIC CONTROL
    // ============================================================

    public static void StopVanillaMusicOnly()
    {
        var controller = NRunMusicController.Instance;

        if (controller == null)
            return;

        var proxy = AccessTools.Field(
                typeof(NRunMusicController),
                "_proxy")
            .GetValue(controller) as Node;

        proxy?.Call("stop_music");
    }
    
    public static void ResumeVanillaMusic()
    {
        var controller = NRunMusicController.Instance;

        if (controller == null)
            return;

        var proxy = AccessTools.Field(
                typeof(NRunMusicController),
                "_proxy")
            .GetValue(controller) as Node;

        string? currentTrack =
            AccessTools.Field(
                    typeof(NRunMusicController),
                    "_currentTrack")
                .GetValue(controller) as string;

        proxy?.Call("stop_music");

        if (!string.IsNullOrEmpty(currentTrack))
        {
            proxy.Call(
                "update_music",
                currentTrack);

            proxy.Call(
                "update_global_parameter",
                "Progress",
                7);
        }
    }

    public static void RestoreVanillaMusic()
    {
        if (!_vanillaMusicMuted)
            return;
    
        if (FmodAudio.BusExists(MusicBus))
        {
            /*
             * Use the current saved BGM setting rather than restoring
             * a stale value captured before the user adjusted it.
             */
            float currentBgmVolume =
                SaveManager.Instance?
                    .SettingsSave?
                    .VolumeBgm
                ?? _savedMusicBusVolume;
    
            FmodAudio.SetBusVolume(
                MusicBus,
                currentBgmVolume);
    
            GD.Print(
                $"Restored STS2 music bus to " +
                $"{currentBgmVolume}");
        }
    
        _vanillaMusicMuted = false;
    }

    // ============================================================
    // VOLUME
    // ============================================================

    private static float ScaleDbByBgmSetting(
        float baseVolumeDb)
    {
        float bgmSetting =
            SaveManager.Instance?
                .SettingsSave?
                .VolumeBgm
            ?? 1f;

        if (bgmSetting <= 0f)
            return -80f;

        float linearVolume =
            Mathf.DbToLinear(baseVolumeDb);

        linearVolume *= bgmSetting;

        return Mathf.LinearToDb(
            Math.Max(linearVolume, 0.0001f));
    }

    public static void RefreshCustomMusicVolumes()
    {
        SetPlayerVolume(
            _battlePlayer,
            MusicVolume);

        SetPlayerVolume(
            _fanfarePlayer,
            FanfareVolume);

        SetPlayerVolume(
            _gameOverPlayer,
            GameOverVolume);
    }

    private static void SetPlayerVolume(
        AudioStreamPlayer? player,
        float baseVolumeDb)
    {
        if (player == null)
            return;

        if (!GodotObject.IsInstanceValid(player))
            return;

        player.VolumeDb =
            ScaleDbByBgmSetting(baseVolumeDb);
    }

    // ============================================================
    // BATTLE MUSIC
    // ============================================================

    public static void Play(
        ulong netId,
        int actIndex,
        RoomType roomType)
    {
        if (!IsLocalPlayer(netId))
            return;

        StopBattleMusic();
        StopFanfare();
        StopGameOverMusic();

        //MuteVanillaMusic();
        StopVanillaMusicOnly();

        string[] pool =
            GetPool(actIndex, roomType);

        // if (pool.Length == 0)
        // {
        //     RestoreVanillaMusic();
        //     return;
        // }

        string path =
            pool[Random.Shared.Next(pool.Length)];

        AudioStream? stream =
            GD.Load<AudioStream>(path);

        if (stream == null)
        {
            GD.PrintErr(
                $"Failed to load Tidus battle music: {path}");

            //RestoreVanillaMusic();
            return;
        }

        /*
         * All current battle tracks are MP3 files.
         * Enabling Loop prevents the finished-player lifecycle issue.
         */
        if (stream is AudioStreamMP3 mp3)
        {
            mp3.Loop = true;
        }

        var player =
            new AudioStreamPlayer
            {
                Name = "TidusBattleMusic",
                Stream = stream,
                Bus = "Master",
                VolumeDb =
                    ScaleDbByBgmSetting(
                        MusicVolume)
            };

        _battlePlayer = player;

        /*
         * This normally will not fire because the MP3 loops.
         * It is kept as protection for unsupported stream types.
         */
        player.Finished += () =>
        {
            if (ReferenceEquals(
                    _battlePlayer,
                    player))
            {
                _battlePlayer = null;
            }

            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }
        };

        Node? parent = NRun.Instance;

        if (parent == null)
            parent = NGame.Instance;

        if (parent == null)
        {
            _battlePlayer = null;
            player.QueueFree();
            //RestoreVanillaMusic();
            return;
        }

        parent.AddChild(player);
        player.Play();

        GD.Print(
            $"Playing looping Tidus music: {path}");
    }

    private static void StopBattleMusic()
    {
        StopPlayer(
            ref _battlePlayer);
    }

    // ============================================================
    // VICTORY FANFARE
    // ============================================================

    public static void PlayVictoryFanfare(
        ulong netId,
        bool isBoss)
    {
        if (!IsLocalPlayer(netId))
            return;

        StopBattleMusic();
        StopFanfare();
        StopGameOverMusic();

        /*
         * Keep vanilla music muted through the victory sequence.
         */
        //MuteVanillaMusic();
        StopVanillaMusicOnly();

        string path =
            isBoss
                ? VictoryFanfare
                : VictoryFanfareShort;

        AudioStream? stream =
            GD.Load<AudioStream>(path);

        if (stream == null)
            return;

        var player =
            new AudioStreamPlayer
            {
                Name = "TidusVictoryFanfare",
                Stream = stream,
                Bus = "Master",
                VolumeDb =
                    ScaleDbByBgmSetting(
                        FanfareVolume)
            };

        _fanfarePlayer = player;

        player.Finished += () =>
        {
            if (ReferenceEquals(
                    _fanfarePlayer,
                    player))
            {
                _fanfarePlayer = null;
            }

            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }
        };

        Node? parent = NRun.Instance;

        if (parent == null)
            parent = NGame.Instance;

        if (parent == null)
        {
            _fanfarePlayer = null;
            player.QueueFree();
            return;
        }

        parent.AddChild(player);
        player.Play();

        GD.Print(
            $"Playing Tidus victory fanfare: {path}");
    }

    private static void StopFanfare()
    {
        StopPlayer(
            ref _fanfarePlayer);
    }

    // ============================================================
    // GAME OVER
    // ============================================================

    public static void PlayGameOverTheme()
    {
        StopBattleMusic();
        StopFanfare();
        StopGameOverMusic();

        //MuteVanillaMusic();
        StopVanillaMusicOnly();

        AudioStream? stream =
            GD.Load<AudioStream>(
                GameOverTheme);

        if (stream == null)
            return;

        var player =
            new AudioStreamPlayer
            {
                Name = "TidusGameOverMusic",
                Stream = stream,
                Bus = "Master",
                VolumeDb =
                    ScaleDbByBgmSetting(
                        GameOverVolume)
            };

        _gameOverPlayer = player;

        player.Finished += () =>
        {
            if (ReferenceEquals(
                    _gameOverPlayer,
                    player))
            {
                _gameOverPlayer = null;
            }

            if (GodotObject.IsInstanceValid(player))
            {
                player.QueueFree();
            }
        };

        Node? parent = NRun.Instance;

        if (parent == null)
            parent = NGame.Instance;

        if (parent == null)
        {
            _gameOverPlayer = null;
            player.QueueFree();
            return;
        }

        parent.AddChild(player);
        player.Play();

        GD.Print(
            "Playing Tidus game over theme");
    }

    private static void StopGameOverMusic()
    {
        StopPlayer(
            ref _gameOverPlayer);
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    public static void Stop()
    {
        StopBattleMusic();
        StopFanfare();
        StopGameOverMusic();

        //RestoreVanillaMusic();
        ResumeVanillaMusic();

        GD.Print(
            "Stopped all Tidus music");
    }

    private static void StopPlayer(
        ref AudioStreamPlayer? playerField)
    {
        AudioStreamPlayer? player =
            playerField;

        /*
         * Clear the static field before touching the node.
         * This prevents deferred callbacks from handling it twice.
         */
        playerField = null;

        if (player == null)
            return;

        if (!GodotObject.IsInstanceValid(player))
            return;

        player.Stop();
        player.QueueFree();
    }

    // ============================================================
    // POOL SELECTION
    // ============================================================

    private static string[] GetPool(
        int actIndex,
        RoomType roomType)
    {
        if (roomType == RoomType.Boss)
        {
            return actIndex switch
            {
                1 => Act1Boss,
                2 => Act2Boss,
                _ => Act3Boss
            };
        }

        if (roomType == RoomType.Elite)
        {
            return actIndex switch
            {
                1 => Act1Elite,
                2 => Act2Elite,
                _ => Act3Elite
            };
        }

        return actIndex switch
        {
            1 => Act1Normal,
            2 => Act2Normal,
            _ => Act3Normal
        };
    }

    private static bool IsLocalPlayer(
        ulong netId)
    {
        return RunManager.Instance != null
            && RunManager.Instance.NetService != null
            && netId ==
               RunManager.Instance.NetService.NetId;
    }
}

// ================================================================
// PROCEED CLEANUP
// ================================================================

[HarmonyPatch(
    typeof(NProceedButton),
    "OnRelease")]
public static class StopTidusMusicOnProceedPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        BattleTheme.Stop();
    }
}

// ================================================================
// MAIN MENU RESTORE
// ================================================================

[HarmonyPatch(
    typeof(NMainMenu),
    "_Ready")]
public static class RestoreMusicOnMainMenuPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        BattleTheme.Stop();
    }
}

// ================================================================
// BGM SLIDER SYNC
// ================================================================

[HarmonyPatch(
    typeof(NAudioManager),
    nameof(NAudioManager.SetBgmVol))]
public static class KeepBgmMutedPatch
{
    [HarmonyPostfix]
    public static void Postfix(
        float __0)
    {
        /*
         * Update the volume of every currently playing Tidus track.
         */
        BattleTheme.RefreshCustomMusicVolumes();

        /*
         * If Tidus has muted vanilla music, the game's slider has just
         * made that bus audible again. Immediately force it back to zero.
         *
         * IsBattleThemeActive only becomes true from BattleTheme.Play(),
         * so this does not affect other characters.
         */
        if (!BattleTheme.IsBattleThemeActive)
            return;

        // BattleTheme.ForceMuteMusicBus();
    }
}