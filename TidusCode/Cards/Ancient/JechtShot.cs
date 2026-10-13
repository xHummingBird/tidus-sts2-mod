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
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Ancient;

public class JechtShot() : TidusCard(
    1,
    CardType.Attack,
    CardRarity.Ancient,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new ExtraDamageVar(2m),
        new CardsVar(3)
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        List<CardModel> discardedCards =
            (await CardSelectCmd.FromHand(
                prefs: new CardSelectorPrefs(
                    SelectionScreenPrompt,
                    0,
                    DynamicVars.Cards.IntValue),
                context: choiceContext,
                player: Owner,
                filter: null,
                source: this))
            .ToList();

        foreach (CardModel card in discardedCards)
        {
            await CardCmd.Discard(choiceContext, card);
        }

        decimal totalDamage =
            DynamicVars.Damage.BaseValue +
            DynamicVars.ExtraDamage.BaseValue * discardedCards.Count;
        
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Tidus tidus)
        {
            tidus.PlayAnimation(ownerCreature, "jecht_shot_1", true);
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
        await DamageCmd.Attack(totalDamage)
            .WithValueProp(ValueProp.Move)
            .WithHitFx(null, "res://Tidus/sfx/Tidus/jecht_shot_hit.wav")
            .FromCard(this, play)
            .Targeting(play.Target)
            .Execute(choiceContext);
        await Task.Delay(350);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
        DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}