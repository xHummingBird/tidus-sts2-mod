using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace Tidus.TidusCode.Extensions;

public class TidusExtensions
{
    public static class CombatHelpers
    {
        private const string DefaultHitVfxScene =
            "res://Tidus/scenes/vfx.tscn";

        private const string DefaultHitVfx = "hit";

        public const string NoHitVfx = "__NONE__";

        public static async Task TidusFakeHit(
            Creature attacker,
            Creature target,
            string? attackSfx = null,
            string? hitSfx = null,
            string? hitVfx = null,
            bool playHitVfx = true)
        {
            if (target == null)
                return;

            if (attackSfx != null)
                SfxCmd.Play(attackSfx);

            if (hitSfx != null)
                SfxCmd.Play(hitSfx);

            var tidus = attacker.Player.Character as Character.Tidus;

            if (playHitVfx)
            {
                tidus.PlayVfxOnTarget(
                    target,
                    DefaultHitVfxScene,
                    hitVfx ?? DefaultHitVfx);
            }

            await CreatureCmd.TriggerAnim(target, "Hit", 0f);

            if (target.Monster?.HasHurtSfx == true)
                SfxCmd.Play(target.Monster.HurtSfx);
        }
    }
}

[HarmonyPatch(
    typeof(CardSelectCmd),
    nameof(CardSelectCmd.FromHandForDiscard))]
public static class BlitzDiscardGlowPatch
{
    [HarmonyPrefix]
    public static bool Prefix(
        PlayerChoiceContext context,
        Player player,
        CardSelectorPrefs prefs,
        Func<CardModel, bool>? filter,
        AbstractModel source,
        ref Task<IEnumerable<CardModel>> __result)
    {
        prefs.ShouldGlowGold = c =>
            c.IsSlyThisTurn ||
            c.Keywords.Contains(TidusEnums.Blitz);

        __result = CardSelectCmd.FromHand(
            context,
            player,
            prefs,
            filter,
            source);

        return false;
    }
}
