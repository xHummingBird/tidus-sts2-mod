using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Common;

public class Slowstrike() : TidusCard(1, CardType.Attack,
    CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<DelayPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DelayPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null && Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttack();
            tidus.PlayAnimation(ownerCreature, "attack");
            await tidus.DashTo(ownerCreature, play.Target, distance: 300f, durationSeconds: 0.15F, overrideAnim: null);
            SfxCmd.Play("res://Tidus/sfx/swing_1.wav");
            await tidus.Delay(ownerCreature, 0.05f);
            tidus.PlayVfxOnTarget(
                play.Target,
                "res://Tidus/scenes/vfx.tscn",
                "hit"
                );
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Tidus/sfx/hit_2.wav")
                .Execute(choiceContext);
            await tidus.Retreat(ownerCreature, duration: 0.2F);
            
        }
        else  await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Tidus/sfx/hit_2.wav")
            .Execute(choiceContext);
        await PowerCmd.Apply<DelayPower>(choiceContext, play.Target, DynamicVars["DelayPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["DelayPower"].UpgradeValueBy(1m);
    }
}