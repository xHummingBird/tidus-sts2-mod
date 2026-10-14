using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Ancient;

public class JechtShotTwo() : TidusCard(
    0,
    CardType.Attack,
    CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(25m),
        new ExtraDamageVar(2),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier((card, _) =>
            {
                return card.Owner.PlayerCombatState.AllCards
                    .Count(c => c.Keywords.Contains(TidusEnums.Blitz));
            })
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
            tidus.PlayAnimation(ownerCreature, "jecht_shot_2", true);
            SfxCmd.Play("res://Tidus/sfx/Tidus/limit_start.wav");
            SfxCmd.Play("res://Tidus/sounds/jecht_shot.wav");
            await Task.Delay(250);
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_swing.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_impact.wav");
            await Task.Delay(50);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, "res://Tidus/sfx/hit_2.wav", hitVfx: "blitzball_hit_1");
            await Task.Delay(400);
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_swing.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_impact.wav");
            await Task.Delay(50);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, "res://Tidus/sfx/hit_2.wav", hitVfx: "blitzball_hit_2");
            await Task.Delay(350);
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_swing.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_impact.wav");
            await Task.Delay(50);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, "res://Tidus/sfx/hit_2.wav", hitVfx: "blitzball_hit_3");
            await Task.Delay(150);
            SfxCmd.Play("res://Tidus/sfx/Tidus/jecht_shot_wind.wav");
            await Task.Delay(600);
            SfxCmd.Play("res://Tidus/sounds/special (1).wav");
            await Task.Delay(100);
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_swing.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_impact.wav");
            await Task.Delay(50);
            tidus.PlayVfxOnTarget(
                play.Target,
                "res://Tidus/scenes/vfx.tscn",
                "jecht_shot_hit"
            );
        }
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .WithValueProp(ValueProp.Move)
            .WithHitFx(null, "res://Tidus/sfx/Tidus/jecht_shot_hit.wav")
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(choiceContext);
        await Task.Delay(350);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        OverdriveManager.SetOverdrive(Owner, 0);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(7m);
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}