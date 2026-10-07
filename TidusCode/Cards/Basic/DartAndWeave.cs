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

namespace Tidus.TidusCode.Cards.Basic;

public class DartAndWeave() : TidusCard(
    2,
    CardType.Attack,
    CardRarity.Basic,
    TargetType.AnyEnemy),
    IBlitz
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
        new BlockVar(6m, ValueProp.Move)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        TidusEnums.Blitz
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (BlitzExecutionContext.IsBlitzPlay(this))
        {
            if (Owner.HasPower<HastePower>())
                await ExecuteAttack(
                    choiceContext,
                    play.Target);
            
            await OnBlitz(
                choiceContext,
                play.Target);
            await CommonActions.CardBlock(this, play);

            return;
        }
        
        await ExecuteAttack(
            choiceContext,
            play.Target);
        
        if (Owner?.Creature.HasPower<HastePower>() == true)
        {
            await OnBlitz(choiceContext, play.Target);
            await CommonActions.CardBlock(this, play);
        }
    }

    public async Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        AudioHelper.PlayRandomDefend();
    }

    private async Task ExecuteAttack(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Tidus tidus)
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
            return;
        }

        await CommonActions.CardAttack(this, target)
            .WithHitFx(
                null,
                "res://Tidus/sfx/hit_2.wav")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}