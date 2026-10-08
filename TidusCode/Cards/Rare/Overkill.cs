using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Rare;

public class Overkill() : TidusCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        TidusStaticHoverTips.Overkill
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();
            tidus.PlayAnimation(ownerCreature, "attack_3");
            await tidus.DashTo(ownerCreature, play.Target, durationSeconds: 0.15f, overrideAnim: null);
            SfxCmd.Play("res://Tidus/sfx/swing_1.wav");
            tidus.PlayVfxOnTarget(play.Target, "res://Tidus/scenes/vfx.tscn", "hit");
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Tidus/sfx/hit_2.wav")
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.15f);
            await tidus.Retreat(ownerCreature, duration: 0.2f);
            return;
        }

        await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(
                null,
                "res://Tidus/sfx/hit_2.wav")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}