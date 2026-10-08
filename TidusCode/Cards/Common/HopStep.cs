using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Cards.Common;

public class HopStep() : TidusCard(
    2,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AnyEnemy)
{
    private bool HasPlayedBlitzCardThisTurn =>
        CombatManager.Instance.History.CardPlaysFinished.Any(
            entry =>
                entry.HappenedThisTurn(CombatState) &&
                entry.CardPlay.Card.Owner == Owner &&
                entry.CardPlay.Card.Keywords.Contains(
                    TidusEnums.Blitz));
    
    protected override bool ShouldGlowGoldInternal => HasPlayedBlitzCardThisTurn;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
        new EnergyVar(1)
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
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
    
    public override Task AfterCardEnteredCombat(
        CardModel card)
    {
        if (card != this)
            return Task.CompletedTask;

        if (HasPlayedBlitzCardThisTurn)
        {
            EnergyCost.SetThisTurn(0);
        }

        return Task.CompletedTask;
    }
    
    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
            return Task.CompletedTask;

        if (cardPlay.Card.Keywords.Contains(
                TidusEnums.Blitz))
        {
            EnergyCost.SetThisTurn(0);
        }

        return Task.CompletedTask;
    }
}