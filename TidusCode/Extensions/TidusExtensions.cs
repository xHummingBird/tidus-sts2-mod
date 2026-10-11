using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;

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