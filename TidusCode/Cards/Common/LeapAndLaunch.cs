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

namespace Tidus.TidusCode.Cards.Common;

public class LeapAndLaunch() : TidusCard(
    1,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy),
    IBlitz
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
        new PowerVar<VigorPower>(6m)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        TidusEnums.Blitz
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<VigorPower>()
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
                Owner.Creature);

            return;
        }
        
        await ExecuteAttack(
            choiceContext,
            play.Target);
        
        if (Owner?.Creature.HasPower<HastePower>() == true)
        {
            await OnBlitz(choiceContext, Owner.Creature);
        }
    }

    public async Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        AudioHelper.PlayRandomDefend();
        await PowerCmd.Apply<VigorPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["VigorPower"].BaseValue,
            Owner.Creature,
            this);
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
            tidus.PlayAnimation(ownerCreature, "attack_3");
            await tidus.DashTo(ownerCreature, target, durationSeconds: 0.15f, overrideAnim: null);
            SfxCmd.Play("res://Tidus/sfx/swing_1.wav");
            tidus.PlayVfxOnTarget(target, "res://Tidus/scenes/vfx.tscn", "hit");
            await CommonActions.CardAttack(this, target)
                .WithHitFx(null, "res://Tidus/sfx/hit_2.wav")
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.15f);
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
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["VigorPower"].UpgradeValueBy(2m);
    }
}