using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Common;

public class Windup() : TidusCard(
    1,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new EnergyVar(1m)
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature =Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();
            tidus.PlayAnimation(ownerCreature, "attack_2");
            await tidus.DashTo(ownerCreature, target, durationSeconds: 0.15f, overrideAnim: null);
            SfxCmd.Play("res://Tidus/sfx/swing_1.wav");
            await tidus.Delay(ownerCreature, 0.05f);
            tidus.PlayVfxOnTarget(target, "res://Tidus/scenes/vfx.tscn", "hit");
            await CommonActions.CardAttack(this, target)
                .WithHitFx(null, "res://Tidus/sfx/hit_2.wav")
                .Execute(choiceContext);
            await tidus.delay(ownerCreature, 0.1f);
            await tidus.Retreat(ownerCreature, duration: 0.2f);
            base.EnergyCost.SetThisCombat(0);
            return;
        }

        await CommonActions.CardAttack(this, target)
            .WithHitFx( null, "res://Tidus/sfx/hit_2.wav")
            .Execute(choiceContext);
        base.EnergyCost.SetThisCombat(0);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}