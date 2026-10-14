using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Cards.Uncommon;

public class FullSlide() : TidusCard(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
       new DamageVar(18m, ValueProp.Move)
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature =
            Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null &&
            Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();
            await tidus.DashTo(ownerCreature, play.Target, distance: 300f, durationSeconds: 0.2f);
            tidus.PlayAnimation(ownerCreature, "full_slide");
            await tidus.Delay(ownerCreature, 0.05f);
            SfxCmd.Play("res://Tidus/sfx/swing_2.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_2.wav");
            await tidus.DashTo(ownerCreature, play.Target, distance: -200f, durationSeconds: 0.05f, overrideAnim: "full_slide");
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "hit");
            await tidus.Delay(ownerCreature, 0.3f);
            SfxCmd.Play("res://Tidus/sfx/swing_2.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_1.wav");
            await tidus.DashTo(ownerCreature, play.Target, distance: -300f, durationSeconds: 0.05f, overrideAnim: "full_slide");
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "hit");
            await tidus.Delay(ownerCreature, 0.3f);
            AudioHelper.PlayRandomLastHit();
            await tidus.Delay(ownerCreature, 0.2f);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "hit");
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Tidus/sfx/Tidus/slash_2.wav")
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.2f);
            await tidus.Retreat(ownerCreature, duration: 0.2f);
        }
        else
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(
                    null,
                    "res://Tidus/sfx/Tidus/slash_3.wav")
                .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        base.DynamicVars.Damage.UpgradeValueBy(6m);
    }
    
    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this)
        {
            return Task.CompletedTask;
        }
        if (base.IsClone)
        {
            return Task.CompletedTask;
        }
        int amount = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Player == base.Owner && e.HappenedThisTurn(base.CombatState));
        ReduceCostBy(amount);
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner)
        {
            return Task.CompletedTask;
        }
        if (cardPlay.Card.Type != CardType.Attack)
        {
            return Task.CompletedTask;
        }
        ReduceCostBy(1);
        return Task.CompletedTask;
    }

    private void ReduceCostBy(int amount)
    {
        base.EnergyCost.AddThisTurn(-amount);
    }
}