using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Ancient;

public class SliceAndDice() : TidusCard(
    1,
    CardType.Attack,
    CardRarity.Ancient,
    TargetType.AnyEnemy),
    IBlitz
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<HastePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(20m, ValueProp.Move),
        new RepeatVar(6)
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

            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
            await ExecuteAttack(
                choiceContext,
                play.Target,
                play);
            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);

            return;
        }
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        await ExecuteAttack(
            choiceContext,
            play.Target,
            play);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    public async Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        
    }

    private async Task ExecuteAttack(
        PlayerChoiceContext choiceContext,
        Creature target,
        CardPlay play)
    {
        var totalDamage = DynamicVars.Damage.BaseValue;
        var ownerCreature =
            Owner?.Creature;
        var animationString = "slice_2";
        if (Owner.HasPower<HastePower>())
        {
            animationString = "slice_1";
            totalDamage *= 2;
        }

        if (ownerCreature != null &&
            Owner?.Character is Character.Tidus tidus)
        {
            SfxCmd.Play("res://Tidus/sounds/ready_go.wav");
            tidus.PlayAnimation(ownerCreature, "overdrive_cast");
            SfxCmd.Play("res://Tidus/sfx/Tidus/limit_thunder_2.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/limit_start.wav");
            await tidus.Delay(ownerCreature, 0.6f);
            await tidus.DashTo(ownerCreature, target, durationSeconds: 0.2f);
            tidus.PlayAnimation(ownerCreature, animationString);
           
            await tidus.Delay(ownerCreature, 0.15f);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, target, "res://Tidus/sfx/Tidus/slash_2.wav", "res://Tidus/sfx/swing_1.wav", hitVfx: "slice_1");
            await tidus.DashTo(ownerCreature, target, 0.05f, -250, false, animationString);
            
            await tidus.Delay(ownerCreature, 0.35f);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, target, "res://Tidus/sfx/Tidus/slash_2.wav", "res://Tidus/sfx/swing_1.wav", hitVfx: "slice_2");
            await tidus.DashTo(ownerCreature, target, 0.05f, -250, false, animationString);
            
            await tidus.Delay(ownerCreature, 0.35f);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, target, "res://Tidus/sfx/Tidus/slash_2.wav", "res://Tidus/sfx/swing_1.wav", hitVfx: "slice_1");
            await tidus.DashTo(ownerCreature, target, 0.05f, -250, false, animationString);

            if (Owner.HasPower<HastePower>())
            {
                await tidus.Delay(ownerCreature, 0.35f);
                TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, target, "res://Tidus/sfx/Tidus/slash_2.wav", "res://Tidus/sfx/swing_1.wav", hitVfx: "slice_2");
                await tidus.DashTo(ownerCreature, target, 0.05f, -250, false, animationString);
                
                await tidus.Delay(ownerCreature, 0.35f);
                TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, target, "res://Tidus/sfx/Tidus/slash_2.wav", "res://Tidus/sfx/swing_1.wav", hitVfx: "slice_1");
                await tidus.DashTo(ownerCreature, target, 0.05f, -250, false, animationString);
            }

            await tidus.Delay(ownerCreature, 0.3f);
            AudioHelper.PlayRandomLastHit();
            SfxCmd.Play("res://Tidus/sfx/Tidus/swing_1.wav");
            tidus.DashTo(ownerCreature, target, 0.05f, -250, false, animationString);
            tidus.PlayVfxOnTarget(target, "res://Tidus/scenes/vfx.tscn", "slice_2");
            await DamageCmd.Attack(totalDamage).FromCard(this,play).Targeting(target).WithValueProp(ValueProp.Move)
                .WithHitFx(null, "res://Tidus/sfx/Tidus/slice_and_dice_last_hit.wav")
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.25f);
            await tidus.Retreat(ownerCreature, duration: 0.2F);
            return;
        }

        await DamageCmd.Attack(totalDamage).FromCard(this,play).Targeting(target).WithValueProp(ValueProp.Move)
            .WithHitFx(null, "res://Tidus/sfx/Tidus/slice_and_dice_last_hit.wav")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(7m);
    }
}