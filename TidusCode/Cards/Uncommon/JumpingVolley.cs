using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Uncommon;

public class JumpingVolley() : TidusCard(
    3,
    CardType.Attack,
    CardRarity.Uncommon,
    TargetType.AnyEnemy),
    IBlitz
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<HastePower>();
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        TidusEnums.Blitz
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        bool isBlitzPlay =
            BlitzExecutionContext.IsBlitzPlay(this);

        bool hasHaste =
            Owner.HasPower<HastePower>();

        /*
         * Normal play performs the Attack effect.
         * A Blitz play also performs the Attack effect while Haste is active.
         */
        if (!isBlitzPlay || hasHaste)
        {
            ArgumentNullException.ThrowIfNull(play.Target);

            await ExecuteAttack(
                choiceContext,
                play,
                play.Target);
        }

        /*
         * Blitz grants Free Skill.
         * A normal play also triggers Blitz while Haste is active.
         */
        if (isBlitzPlay || hasHaste)
        {
            await OnBlitz(
                choiceContext,
                play.Target);
        }
    }

    public async Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        await PowerCmd.Apply<FreeAttackPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature, this);
    }

    private async Task ExecuteAttack(
        PlayerChoiceContext choiceContext,
        CardPlay play,
        Creature target)
    {
        Creature ownerCreature =
            Owner.Creature;

        CenterCardCinematic.Start(
            RunManager.Instance.NetService.NetId);

        
        if (Owner.Character is Character.Tidus tidus)
            {
                AudioHelper.PlayRandomPhrase();

                tidus.PlayAnimation(
                    ownerCreature,
                    "jumping_kick");

                await tidus.Delay(
                    ownerCreature,
                    0.3f);

                SfxCmd.Play(
                    "res://Tidus/sfx/Tidus/blitz_swing.wav");

                SfxCmd.Play(
                    "res://Tidus/sfx/Tidus/blitz_impact.wav");

                await tidus.Delay(
    
                    ownerCreature,
                    0.05f);

                tidus.PlayVfxOnTarget(
                    target,
                    "res://Tidus/scenes/vfx.tscn",
                    "blitzball_hit_2");

                await DamageCmd.Attack(
                        DynamicVars.Damage.BaseValue)
                    .FromCard(this, play)
                    .WithHitFx(
                        null,
                        "res://Tidus/sfx/hit_2.wav")
                    .Targeting(target)
                    .Execute(choiceContext);
                
                if (!ownerCreature.HasPower<HastePower>())
                    await tidus.Delay(ownerCreature, 0.15f);

                
            }
            else
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, play).WithHitFx(
                    null,
                    "res://Tidus/sfx/hit_2.wav")
                .Targeting(target)
                .Execute(choiceContext);
        
            CenterCardCinematic.End(
                RunManager.Instance.NetService.NetId);
        }
    

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
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
        int amount = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.CardPlay.Card.Type == CardType.Skill && e.CardPlay.Player == base.Owner && e.HappenedThisTurn(base.CombatState));
        ReduceCostBy(amount);
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner)
        {
            return Task.CompletedTask;
        }
        if (cardPlay.Card.Type != CardType.Skill)
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