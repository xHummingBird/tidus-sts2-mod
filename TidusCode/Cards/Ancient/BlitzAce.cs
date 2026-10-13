using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Ancient;

public class BlitzAce() : TidusCard(
    0,
    CardType.Attack,
    CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(35m),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) =>
            {
                var creature = card.Owner.Creature;

                int hpLost =
                    (int)(creature.MaxHp - creature.CurrentHp);

                int hpPerDamage =
                    card.IsUpgraded
                        ? 2
                        : 3;

                return hpLost / hpPerDamage;
            }),
        new DynamicVar("HpThreshold", 3)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];


    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Remove<OverdriveReadyPower>(Owner.Creature);
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Tidus tidus)
        {
            tidus.PlayAnimation(ownerCreature, "blitz_ace", true);
            SfxCmd.Play("res://Tidus/sfx/Tidus/limit_thunder_2.wav");
            SfxCmd.Play("res://Tidus/sounds/run_start.wav");
            await Task.Delay(650);
            await tidus.DashTo(ownerCreature, play.Target, durationSeconds: 0.15f, distance: 300F, overrideAnim: "blitz_ace", normalSpeed: true);
            SfxCmd.Play("res://Tidus/sfx/swing_2.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_2.wav");
            await tidus.Delay(ownerCreature, 0.05f, true);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "slice_1");
            
            await tidus.Delay(ownerCreature, 0.15f, true);
            SfxCmd.Play("res://Tidus/sfx/swing_1.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_1.wav");
            await tidus.Delay(ownerCreature, 0.033f, true);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "slice_2");
            
            await tidus.Delay(ownerCreature, 0.217f, true);
            SfxCmd.Play("res://Tidus/sfx/swing_2.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_2.wav");
            await tidus.Delay(ownerCreature, 0.05f, true);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "slice_1");
            
            await tidus.Delay(ownerCreature, 0.15f, true);
            SfxCmd.Play("res://Tidus/sfx/swing_1.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_1.wav");
            await tidus.Delay(ownerCreature, 0.033f, true);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "slice_2");
            
            SfxCmd.Play("res://Tidus/sounds/last_hit (19).wav");
            await tidus.Delay(ownerCreature, 0.217f, true);
            SfxCmd.Play("res://Tidus/sfx/swing_2.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_2.wav");
            await tidus.Delay(ownerCreature, 0.05f, true);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "slice_1");
            
            await tidus.Delay(ownerCreature, 0.23f, true);
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_3.wav");
            await tidus.Delay(ownerCreature, 0.02f, true);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "slice_2");
            
            await tidus.Delay(ownerCreature, 0.2f, true);
            SfxCmd.Play("res://Tidus/sfx/swing_2.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_2.wav");
            await tidus.Delay(ownerCreature, 0.033f, true);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "slice_1");
            AudioHelper.PlayRandomAttackHard();
            
            
            await tidus.Delay(ownerCreature, 0.4f, true);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, "res://Tidus/sfx/Tidus/slice_and_dice_last_hit.wav", hitVfx: "slice_2");
            await tidus.Delay(ownerCreature, 0.317f, true);
            
            await tidus.DashTo(ownerCreature, play.Target, durationSeconds: 0.1f, distance: 550F, overrideAnim: "blitz_ace", normalSpeed: true);
            SfxCmd.Play("res://Tidus/sfx/Tidus/sword_sheathe_2.wav");
            await tidus.Delay(ownerCreature, 0.4f, true);
            SfxCmd.Play("res://Tidus/sfx/Tidus/sword_sheathe.wav");
            await tidus.Delay(ownerCreature, 0.1f, true);
            SfxCmd.Play("res://Tidus/sfx/jecht_shot_hit.wav");
            await tidus.Delay(ownerCreature, 0.15f, true);
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_ace_impact.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_impact.wav");
            await tidus.Delay(ownerCreature, 0.3f, true);
            SfxCmd.Play("res://Tidus/sounds/special (1).wav");
            await tidus.Delay(ownerCreature, 0.1f, true);
            tidus.PlayVfxOnTarget(
                play.Target,
                "res://Tidus/scenes/vfx.tscn",
                "hit"
            );
            await DamageCmd.Attack(DynamicVars.CalculatedDamage.BaseValue)
                .WithValueProp(ValueProp.Move)
                .WithHitFx(null, "res://Tidus/sfx/Tidus/jecht_shot_hit.wav")
                .FromCard(this, play)
                .Targeting(play.Target)
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.8f, true);
            await tidus.Retreat(ownerCreature, duration: 0.2F);
        }
        else await DamageCmd.Attack(DynamicVars.CalculatedDamage.BaseValue)
            .WithValueProp(ValueProp.Move)
            .WithHitFx(null, "res://Tidus/sfx/Tidus/jecht_shot_hit.wav")
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        OverdriveManager.SetOverdrive(Owner, 0);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(10m);
        DynamicVars["HpThreshold"].UpgradeValueBy(-1);
    }
}