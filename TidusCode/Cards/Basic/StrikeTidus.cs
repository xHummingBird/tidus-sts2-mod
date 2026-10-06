using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Cards.Basic;

public class StrikeTidus() : TidusCard(1, CardType.Attack,
    CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6m, ValueProp.Move),
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
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}