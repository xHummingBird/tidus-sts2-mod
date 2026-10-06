using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Tidus.TidusCode.Extensions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Cards.Basic;
using Tidus.TidusCode.Powers;
using Tidus.TidusCode.Relics;

namespace Tidus.TidusCode.Character;

public class Tidus : PlaceholderCharacterModel
{
    public const string CharacterId = "Tidus";

    public static readonly Color Color = new("ffffff");

    public const float HasteSpeedMultiplier = 1.5f;

    private Vector2? _originalPosition;

    public override Color NameColor => Color;

    public override CharacterGender Gender =>
        CharacterGender.Masculine;

    public override int StartingHp => 75;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeTidus>(),
        ModelDb.Card<StrikeTidus>(),
        ModelDb.Card<StrikeTidus>(),
        ModelDb.Card<StrikeTidus>(),
        ModelDb.Card<StrikeTidus>(),

        ModelDb.Card<DefendTidus>(),
        ModelDb.Card<DefendTidus>(),
        ModelDb.Card<DefendTidus>(),
        ModelDb.Card<DefendTidus>(),
        ModelDb.Card<Guard>(),

        // Replace this with Tidus's additional starting card.
        ModelDb.Card<DartAndWeave>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        // Replace this if Tidus's starting relic uses another name.
        ModelDb.Relic<Brotherhood>()
    ];

    public override CardPoolModel CardPool =>
        ModelDb.CardPool<TidusCardPool>();

    public override RelicPoolModel RelicPool =>
        ModelDb.RelicPool<TidusRelicPool>();

    public override PotionPoolModel PotionPool =>
        ModelDb.PotionPool<TidusPotionPool>();

    // =========================================================================
    // CHARACTER UI
    // =========================================================================

    public override Control CustomIcon
    {
        get
        {
            var icon =
                NodeFactory<Control>.CreateFromResource(
                    CustomIconTexturePath);

            icon.SetAnchorsAndOffsetsPreset(
                Control.LayoutPreset.FullRect);

            return icon;
        }
    }

    public override CustomEnergyCounter? CustomEnergyCounter =>
        new(
            EnergyCounterPaths,
            new Color(0.2f, 0.2f, 0.2f),
            new Color(1f, 1f, 1f));

    private string EnergyCounterPaths(int i)
    {
        return i switch
        {
            1 => "charui/big_energy.png".ImagePath(),
            _ => "charui/blank.png".ImagePath()
        };
    }

    public override string CustomIconTexturePath =>
        "character_icon_tidus.png".CharacterUiPath();

    public override string CustomCharacterSelectIconPath =>
        "char_select_tidus.png".CharacterUiPath();

    public override string CustomCharacterSelectLockedIconPath =>
        "char_select_char_name_locked.png".CharacterUiPath();

    public override string CustomMapMarkerPath =>
        "map_marker_tidus.png".CharacterUiPath();

    private const string CustomVisualScenePath =
        "res://Tidus/scenes/tidus.tscn";

    public override string CustomRestSiteAnimPath =>
        "res://Tidus/scenes/tidus_rest_site.tscn";

    public override string CustomCharacterSelectBg =>
        "res://Tidus/images/charui/char_selection_bg_tidus.tscn";

    public override string CustomMerchantAnimPath =>
        "res://Tidus/scenes/tidus_merchant.tscn";

    public override string CharacterSelectSfx =>
        "res://Tidus/sounds/run_start.wav";

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        TidusAssets.EnsurePreloaded();

        return NodeFactory<NCreatureVisuals>.CreateFromScene(
            CustomVisualScenePath);
    }

    // =========================================================================
    // HASTE SPEED
    // =========================================================================

    /// <summary>
    /// Returns Tidus's current animation speed.
    ///
    /// Normal:
    ///     1.0
    ///
    /// Haste:
    ///     1.5
    ///
    /// normalSpeed:
    ///     Always 1.0, even when Tidus has Haste.
    /// </summary>
    public float GetAnimationSpeed(
        Creature creature,
        bool normalSpeed = false)
    {
        if (normalSpeed)
            return 1f;

        if (creature?.Player?.Character is not Tidus)
            return 1f;

        return creature.HasPower<HastePower>()
            ? HasteSpeedMultiplier
            : 1f;
    }

    /// <summary>
    /// Converts a normal duration into the real duration after Haste.
    ///
    /// For example:
    ///     0.30 seconds / 1.5 = 0.20 seconds.
    /// </summary>
    public float GetScaledDuration(
        Creature creature,
        float durationSeconds,
        bool normalSpeed = false)
    {
        if (durationSeconds <= 0f)
            return 0f;

        float speed =
            GetAnimationSpeed(creature, normalSpeed);

        return durationSeconds / speed;
    }

    /// <summary>
    /// Tidus-compatible replacement for Task.Delay when specifying seconds.
    ///
    /// Example:
    ///     await tidus.Delay(ownerCreature, 0.35f);
    ///
    /// Ignore Haste:
    ///     await tidus.Delay(
    ///         ownerCreature,
    ///         0.35f,
    ///         normalSpeed: true);
    /// </summary>
    public Task Delay(
        Creature creature,
        float seconds,
        bool normalSpeed = false)
    {
        if (seconds <= 0f)
            return Task.CompletedTask;

        float scaledSeconds =
            GetScaledDuration(
                creature,
                seconds,
                normalSpeed);

        int milliseconds =
            Math.Max(
                1,
                (int)Math.Round(
                    scaledSeconds * 1000f));

        return Task.Delay(milliseconds);
    }

    /// <summary>
    /// Tidus-compatible replacement for Task.Delay when specifying
    /// milliseconds.
    ///
    /// Example:
    ///     await tidus.DelayMs(ownerCreature, 350);
    /// </summary>
    public Task DelayMs(
        Creature creature,
        int milliseconds,
        bool normalSpeed = false)
    {
        if (milliseconds <= 0)
            return Task.CompletedTask;

        float speed =
            GetAnimationSpeed(
                creature,
                normalSpeed);

        int scaledMilliseconds =
            Math.Max(
                1,
                (int)Math.Round(
                    milliseconds / speed));

        return Task.Delay(scaledMilliseconds);
    }

    // =========================================================================
    // ANIMATION
    // =========================================================================

    public (
        float total,
        float[] impacts
    ) PlayAnimation(
        Creature creature,
        string trigger,
        bool normalSpeed = false)
    {
        if (creature == null ||
            string.IsNullOrEmpty(trigger))
        {
            return (
                0f,
                Array.Empty<float>());
        }

        var node =
            NCombatRoom.Instance?
                .GetCreatureNode(creature);

        if (node?.Visuals == null)
        {
            return (
                0f,
                Array.Empty<float>());
        }

        var animPlayer =
            node.Visuals.GetNodeOrNull<AnimationPlayer>(
                "AnimationPlayer");

        if (animPlayer == null)
        {
            return (
                0f,
                Array.Empty<float>());
        }

        string godotTrigger =
            trigger.ToLowerInvariant() switch
            {
                "hit" => "hurt",
                "idle" => "idle",
                "attack" => "attack_tidus",
                "dead" => "die",
                "die" => "die",
                _ => trigger
            };

        if (!animPlayer.HasAnimation(godotTrigger))
        {
            return (
                0f,
                Array.Empty<float>());
        }

        var animation =
            animPlayer.GetAnimation(godotTrigger);

        float speed =
            GetAnimationSpeed(
                creature,
                normalSpeed);

        animPlayer.SpeedScale = speed;
        animPlayer.Play(godotTrigger);

        if (godotTrigger != "idle" &&
            godotTrigger != "die" &&
            animPlayer.HasAnimation("idle"))
        {
            animPlayer.Queue("idle");
        }

        float effectiveLength =
            (float)animation.Length / speed;

        return (
            effectiveLength,
            Array.Empty<float>());
    }

    // =========================================================================
    // DASH TO
    // =========================================================================

    public async Task DashTo(
        Creature player,
        Creature target,
        float durationSeconds = 0.3f,
        float distance = 200f,
        bool dashBehind = false,
        string? overrideAnim = null,
        bool normalSpeed = false)
    {
        var node =
            NCombatRoom.Instance?
                .GetCreatureNode(player);

        var targetNode =
            NCombatRoom.Instance?
                .GetCreatureNode(target);

        if (node == null ||
            targetNode == null)
        {
            return;
        }

        if (!_originalPosition.HasValue)
            _originalPosition = node.Position;

        if (!string.IsNullOrEmpty(overrideAnim))
        {
            PlayAnimation(
                player,
                overrideAnim,
                normalSpeed);
        }
        else
        {
            PlayAnimation(
                player,
                "dash",
                normalSpeed);
        }

        bool playerIsLeftOfTarget =
            node.Position.X <
            targetNode.Position.X;

        Vector2 offsetDirection =
            playerIsLeftOfTarget
                ? Vector2.Left
                : Vector2.Right;

        if (dashBehind)
            offsetDirection = -offsetDirection;

        Vector2 targetPosition =
            targetNode.Position +
            offsetDirection * distance;

        float effectiveDuration =
            GetScaledDuration(
                player,
                durationSeconds,
                normalSpeed);

        if (effectiveDuration <= 0f)
        {
            node.Position = targetPosition;
            return;
        }

        var tween = node.CreateTween();

        tween.TweenProperty(
                node,
                "position",
                targetPosition,
                effectiveDuration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(
            tween,
            Tween.SignalName.Finished);
    }

    // =========================================================================
    // DASH PAST
    // =========================================================================

    public async Task DashPast(
        Creature player,
        Creature target,
        string? attackAnim = null,
        float durationSeconds = 0.3f,
        float behindDistance = 200f,
        float overshoot = 0f,
        bool normalSpeed = false)
    {
        var node =
            NCombatRoom.Instance?
                .GetCreatureNode(player);

        var targetNode =
            NCombatRoom.Instance?
                .GetCreatureNode(target);

        if (node == null ||
            targetNode == null)
        {
            return;
        }

        if (!_originalPosition.HasValue)
            _originalPosition = node.Position;

        Vector2 frontDirection =
            player.Side == CombatSide.Player
                ? Vector2.Left
                : Vector2.Right;

        Vector2 behindDirection =
            -frontDirection;

        Vector2 endPosition =
            targetNode.Position +
            behindDirection *
            (behindDistance + overshoot);

        if (!string.IsNullOrEmpty(attackAnim))
        {
            PlayAnimation(
                player,
                attackAnim,
                normalSpeed);
        }

        float effectiveDuration =
            GetScaledDuration(
                player,
                durationSeconds,
                normalSpeed);

        if (effectiveDuration <= 0f)
        {
            node.Position = endPosition;
            return;
        }

        var tween = node.CreateTween();

        tween.TweenProperty(
                node,
                "position",
                endPosition,
                effectiveDuration)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        await node.ToSignal(
            tween,
            Tween.SignalName.Finished);
    }

    // =========================================================================
    // RETREAT
    // =========================================================================

    public async Task Retreat(
        Creature player,
        string? animation = "retreat",
        bool goIdle = true,
        float duration = 0.3f,
        bool normalSpeed = false)
    {
        var node =
            NCombatRoom.Instance?
                .GetCreatureNode(player);

        if (node == null ||
            !_originalPosition.HasValue)
        {
            return;
        }

        if (!string.IsNullOrEmpty(animation))
        {
            PlayAnimation(
                player,
                animation,
                normalSpeed);
        }

        float effectiveDuration =
            GetScaledDuration(
                player,
                duration,
                normalSpeed);

        if (effectiveDuration <= 0f)
        {
            node.Position =
                _originalPosition.Value;
        }
        else
        {
            var tween = node.CreateTween();

            tween.TweenProperty(
                    node,
                    "position",
                    _originalPosition.Value,
                    effectiveDuration)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.InOut);

            await node.ToSignal(
                tween,
                Tween.SignalName.Finished);
        }

        _originalPosition = null;

        if (node.Visuals != null)
            node.Visuals.Position = Vector2.Zero;

        if (goIdle)
        {
            PlayAnimation(
                player,
                "idle",
                normalSpeed);
        }
    }

    // =========================================================================
    // ORIGINAL POSITION MANAGEMENT
    // =========================================================================

    public void RememberCombatPosition(
        Creature creature)
    {
        var node =
            NCombatRoom.Instance?
                .GetCreatureNode(creature);

        if (node == null)
            return;

        if (!_originalPosition.HasValue)
            _originalPosition = node.Position;
    }

    public void ForgetCombatPosition()
    {
        _originalPosition = null;
    }

    public Vector2? GetOriginalCombatPosition()
    {
        return _originalPosition;
    }

    // =========================================================================
    // SCREEN SHAKE
    // =========================================================================

    public void DoScreenShake(
        ShakeStrength strength =
            ShakeStrength.Medium,
        ShakeDuration duration =
            ShakeDuration.Short)
    {
        NGame.Instance?.ScreenShake(
            strength,
            duration);
    }

    // =========================================================================
    // VFX
    // =========================================================================

    public Node2D PlayVfxOnTarget(
        Creature target,
        string path,
        string animName)
    {
        var targetNode = NCombatRoom.Instance?.GetCreatureNode(target);

        if (targetNode?.Visuals == null)
            return null;

        var scene = GD.Load<PackedScene>(path);
        var vfx = scene.Instantiate<Node2D>();

        targetNode.Visuals.AddChild(vfx);
        vfx.Position = Vector2.Zero;

        var animPlayer =
            vfx.GetNode<AnimationPlayer>("AnimationPlayer");

        if (animPlayer.HasAnimation(animName))
            animPlayer.Play(animName);

        return vfx;
    }

    /// <summary>
    /// Use this overload when the owner creature is already available.
    /// This avoids searching the combat creature list.
    /// </summary>
    public Node2D? PlayVfxOnTarget(
        Creature owner,
        Creature target,
        string path,
        string animName,
        bool normalSpeed = false)
    {
        var targetNode =
            NCombatRoom.Instance?
                .GetCreatureNode(target);

        if (targetNode?.Visuals == null)
            return null;

        var scene =
            GD.Load<PackedScene>(path);

        if (scene == null)
            return null;

        var vfx =
            scene.Instantiate<Node2D>();

        targetNode.Visuals.AddChild(vfx);
        vfx.Position = Vector2.Zero;

        var animPlayer =
            vfx.GetNodeOrNull<AnimationPlayer>(
                "AnimationPlayer");

        if (animPlayer == null)
            return vfx;

        animPlayer.SpeedScale =
            GetAnimationSpeed(
                owner,
                normalSpeed);

        if (animPlayer.HasAnimation(animName))
            animPlayer.Play(animName);

        return vfx;
    }

    // =========================================================================
    // AUTOMATIC ANIMATION TRIGGERS
    // =========================================================================

    [HarmonyPatch(
        typeof(NCreature),
        nameof(NCreature.SetAnimationTrigger))]
    public static class NCreatureSetTriggerPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            NCreature __instance,
            string trigger)
        {
            if (__instance.Entity?
                    .Player?
                    .Character
                is Tidus character)
            {
                character.PlayAnimation(
                    __instance.Entity,
                    trigger);

                return false;
            }

            return true;
        }
    }

    // =========================================================================
    // DEATH ANIMATION
    // =========================================================================

    [HarmonyPatch(
        typeof(NCreature),
        nameof(NCreature.StartDeathAnim))]
    public static class TidusStartDeathAnimPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            NCreature __instance,
            ref float __result)
        {
            if (__instance.Entity?
                    .Player?
                    .Character
                is not Tidus character)
            {
                return;
            }

            AudioHelper.PlayRandomGameover();

            var animation =
                character.PlayAnimation(
                    __instance.Entity,
                    "die");

            __result =
                animation.total > 0f
                    ? animation.total
                    : 1.5f;
        }
    }

    // =========================================================================
    // VICTORY ANIMATION
    // =========================================================================

    [HarmonyPatch(
        typeof(Hook),
        nameof(Hook.AfterCombatVictory))]
    public static class TidusVictoryAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            IRunState runState,
            CombatState? combatState)
        {
            var creatures =
                combatState?
                    .Creatures?
                    .Where(creature =>
                        creature.IsPlayer);

            if (creatures == null)
                return;

            foreach (var creature in creatures)
            {
                if (creature.Player?.Character
                    is not Tidus character)
                {
                    continue;
                }

                var node =
                    NCombatRoom.Instance?
                        .GetCreatureNode(creature);

                var animPlayer =
                    node?
                        .Visuals?
                        .GetNodeOrNull<AnimationPlayer>(
                            "AnimationPlayer");

                if (animPlayer == null)
                    continue;

                AudioHelper.PlayRandomVictory();

                float speed =
                    character.GetAnimationSpeed(
                        creature);

                animPlayer.SpeedScale = speed;

                if (animPlayer.HasAnimation(
                    "victory_before"))
                {
                    animPlayer.Play(
                        "victory_before");

                    if (animPlayer.HasAnimation(
                        "victory"))
                    {
                        animPlayer.Queue(
                            "victory");
                    }
                }
                else if (animPlayer.HasAnimation(
                    "victory"))
                {
                    animPlayer.Play(
                        "victory");
                }
            }
        }
    }

    // =========================================================================
    // DAMAGE ANIMATION
    // =========================================================================

    [HarmonyPatch(
        typeof(Hook),
        nameof(Hook.AfterDamageReceived))]
    public static class TidusDamageAnimationPatch
    {
        [HarmonyPostfix]
        public static void Postfix(
            Creature target,
            DamageResult result,
            ValueProp props,
            Creature? dealer)
        {
            if (target.Player?.Character
                is not Tidus character)
            {
                return;
            }

            if (dealer == null ||
                dealer.Side != CombatSide.Enemy)
            {
                return;
            }

            if (props.HasFlag(
                    ValueProp.SkipHurtAnim) ||
                props.HasFlag(
                    ValueProp.Unpowered))
            {
                return;
            }

            if (result.WasFullyBlocked &&
                result.BlockedDamage > 0)
            {
                character.PlayAnimation(
                    target,
                    "block", true);

                return;
            }

            if (result.UnblockedDamage <= 0 ||
                target.IsDead)
            {
                return;
            }

            character.PlayAnimation(
                target,
                "hit", true);

            if (target.CurrentHp < 20)
            {
                AudioHelper
                    .PlayRandomDamagedCritical();
            }
            else if (result.UnblockedDamage < 10)
            {
                AudioHelper
                    .PlayRandomDamaged();
            }
            else
            {
                AudioHelper
                    .PlayRandomDamagedHigh();
            }
        }
    }

    // =========================================================================
    // CHOOSE-A-CARD PATCH
    // =========================================================================

    [HarmonyPatch(
        typeof(CardSelectCmd),
        nameof(CardSelectCmd.FromChooseACardScreen))]
    public static class
        TidusCardSelectCmdFromChooseACardScreenPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            PlayerChoiceContext context,
            IReadOnlyList<CardModel> cards,
            Player player,
            bool canSkip,
            ref Task<CardModel?> __result)
        {
            __result =
                PatchedChoose(
                    context,
                    cards,
                    player,
                    canSkip);

            return false;
        }

        private static async Task<CardModel?>
            PatchedChoose(
                PlayerChoiceContext context,
                IReadOnlyList<CardModel> cards,
                Player player,
                bool canSkip)
        {
            if (cards.Count > 5)
            {
                throw new ArgumentException(
                    "Only works with 5 or fewer cards",
                    nameof(cards));
            }

            if (cards.Count == 0)
                return null;

            uint choiceId =
                RunManager.Instance
                    .PlayerChoiceSynchronizer
                    .ReserveChoiceId(player);

            await context.SignalPlayerChoiceBegun(
                player,
                PlayerChoiceOptions.None);

            CardModel? result;

            if (LocalContext.IsMe(player))
            {
                NPlayerHand.Instance?
                    .CancelAllCardPlay();

                var screen =
                    NChooseACardSelectionScreen
                        .ShowScreen(
                            cards,
                            canSkip);

                if (screen == null)
                {
                    await context
                        .SignalPlayerChoiceEnded();

                    return null;
                }

                foreach (var card in cards)
                {
                    SaveManager.Instance
                        .MarkCardAsSeen(card);
                }

                result =
                    (await screen.CardsSelected())
                    .FirstOrDefault();

                int index =
                    cards.IndexOf(result);

                var choiceResult =
                    PlayerChoiceResult
                        .FromIndex(index);

                RunManager.Instance
                    .PlayerChoiceSynchronizer
                    .SyncLocalChoice(
                        player,
                        choiceId,
                        choiceResult);
            }
            else
            {
                int index =
                    (await RunManager.Instance
                        .PlayerChoiceSynchronizer
                        .WaitForRemoteChoice(
                            player,
                            choiceId))
                    .AsIndex();

                result =
                    index < 0
                        ? null
                        : cards[index];
            }

            await context
                .SignalPlayerChoiceEnded();

            return result;
        }
    }
}