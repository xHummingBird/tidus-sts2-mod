using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Relics;

public abstract class OverdriveRelicBase : TidusRelic
{
    [SavedProperty]
    public int StoredOverdrive { get; internal set; }
    
    private bool _triggerOverkill;

    public virtual int MaxOverdrive => 100;
    
    protected virtual int OverdrivePerTurn => 5;
    
    protected virtual int OverdrivePerAttack => 3;
    
    protected virtual int OverdrivePerBlitzDiscard => 6;
    
    protected virtual int HasteAttackBonus => 3;
    
    protected virtual int HasteBlitzBonus => 3;

    public override bool ShowCounter => false;

    public override Task BeforeCombatStart()
    {
        Status = RelicStatus.Normal;
        _triggerOverkill = false;
        OverdriveManager.ResetOverdrive(Owner);

        return Task.CompletedTask;
    }
    
    public override async Task AfterSideTurnStartLate(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (!CombatManager.Instance.IsInProgress)
            return;

        if (Owner?.Creature == null)
            return;

        if (side != Owner.Creature.Side)
            return;

        if (Owner.Creature.IsDead)
            return;

        int amount = CalculateTurnStartGain();

        amount = ModifyTurnStartOverdriveGain(
            amount);

        GainOverdrive(amount);
        
        await OverdriveManager.CheckOverdriveReady(
            Owner.Creature,
            null,
            Owner.Creature,
            null);
        
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!CombatManager.Instance.IsInProgress)
            return;

        CardModel? card = cardPlay.Card;

        if (card == null)
            return;

        if (card.Owner != Owner)
            return;

        int amount = 0;
        
        if (BlitzExecutionContext.IsResolving(card))
        {
            amount = CalculateBlitzDiscardGain(card);

            amount = ModifyBlitzDiscardOverdriveGain(
                card,
                amount);
        }
        else if (card.Type == CardType.Attack)
        {
            amount = CalculateAttackGain(card);

            amount = ModifyAttackOverdriveGain(
                card,
                amount);
        }

        amount = ModifyTotalCardOverdriveGain(
            card,
            amount);

        GainOverdrive(amount);
        
        await OverdriveManager.CheckOverdriveReady(
                Owner.Creature,
                null,
                Owner.Creature,
                null);
    }

    // =====================================================================
    // BASE CALCULATIONS
    // =====================================================================

    protected virtual int CalculateTurnStartGain()
    {
        int amount = OverdrivePerTurn;

        if (HasHaste())
            amount *= 2;

        return amount;
    }

    protected virtual int CalculateAttackGain(
        CardModel card)
    {
        int amount = OverdrivePerAttack;

        if (HasHaste())
            amount += HasteAttackBonus;

        return amount;
    }

    protected virtual int CalculateBlitzDiscardGain(
        CardModel card)
    {
        int amount = OverdrivePerBlitzDiscard;

        if (HasHaste())
            amount += HasteBlitzBonus;

        return amount;
    }

    // =====================================================================
    // OVERRIDE POINTS FOR BROTHERHOOD AND CALADBOLG
    // =====================================================================

    protected virtual int ModifyTurnStartOverdriveGain(
        int amount)
    {
        return amount;
    }

    protected virtual int ModifyAttackOverdriveGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    protected virtual int ModifyBlitzDiscardOverdriveGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    protected virtual int ModifyTotalCardOverdriveGain(
        CardModel card,
        int amount)
    {
        return amount;
    }

    /// <summary>
    /// Called whenever this relic's stored Overdrive changes.
    /// Brotherhood and Caladbolg can override this for UI or other effects.
    /// </summary>
    protected virtual void OnOverdriveChanged(
        int previousAmount,
        int newAmount)
    {
    }

    // =====================================================================
    // MANAGER COMMUNICATION
    // =====================================================================

    internal void NotifyOverdriveChanged(
        int previousAmount,
        int newAmount)
    {
        OnOverdriveChanged(
            previousAmount,
            newAmount);
    }

    // =====================================================================
    // HELPERS
    // =====================================================================

    protected bool HasHaste()
    {
        return Owner?.Creature?.HasPower<HastePower>()
               == true;
    }

    private void GainOverdrive(
        int requestedAmount)
    {
        requestedAmount = Math.Max(
            0,
            requestedAmount);

        if (requestedAmount <= 0)
            return;

        int actualGain =
            OverdriveManager.GainOverdrive(
                Owner,
                requestedAmount);

        /*
         * Only flash when Overdrive actually changed.
         *
         * For example, attempting to gain Overdrive while already
         * at 100 will not flash the relic.
         */
    }
    
    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner.Creature)
            return;

        if (!target.Powers.All(
                p => p.ShouldOwnerDeathTriggerFatal()))
        {
            return;
        }

        int overkillThreshold =
            (int)Math.Ceiling(
                target.MaxHp * 0.20m);

        if (Owner.GetRelic<TamingSword>() != null)
        {
            overkillThreshold =
                (int)Math.Ceiling(
                    target.MaxHp * 0.12m);
        }

        /*
         * Amount reaching HP after block.
         */
        decimal hpDamage =
            Math.Max(
                amount - target.Block,
                0);

        bool lethal =
            hpDamage >= target.CurrentHp;

        bool overkill =
            amount >= overkillThreshold;

        if (!lethal || !overkill)
            return;

        _triggerOverkill =
            true;

        if (dealer?.Player?.Character is Character.Tidus tidus)
        {
            tidus.PlayVfxOnTarget(
                target,
                "res://Tidus/scenes/vfx.tscn",
                "overkill");
        }
    }
    
    // public override async Task AfterDamageReceived(
    //     PlayerChoiceContext choiceContext,
    //     Creature target,
    //     DamageResult result,
    //     ValueProp props,
    //     Creature? dealer,
    //     CardModel? cardSource)
    // {
    //     GD.Print("OVERKILL RELIC HOOK");
    //     
    //     int overkillThreshold = (int)Math.Ceiling(target.MaxHp * 0.20m);
    //
    //     var relic = Owner.GetRelic<TamingSword>();
    //    
    //     if (relic != null)
    //         overkillThreshold = (int)Math.Ceiling(target.MaxHp * 0.12m);
    //
    //     int totalDamage =
    //         result.BlockedDamage +
    //         result.UnblockedDamage +
    //         result.OverkillDamage;
    //
    //     bool triggerOverkill =
    //         result.WasTargetKilled &&
    //         totalDamage >= overkillThreshold;
    //     
    //     GD.Print($"WasKilled: {result.WasTargetKilled}");
    //     GD.Print($"Blocked: {result.BlockedDamage}");
    //     GD.Print($"Unblocked: {result.UnblockedDamage}");
    //     GD.Print($"Overkill: {result.OverkillDamage}");
    //     GD.Print($"TotalDamage: {totalDamage}");
    //     GD.Print($"Threshold: {overkillThreshold}");
    //     bool fatal =
    //         target.Powers.All(
    //             p => p.ShouldOwnerDeathTriggerFatal());
    //     GD.Print($"Fatal: {fatal}");
    //
    //     if (!triggerOverkill)
    //         return;
    //     
    //     if (!target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal()))
    //         return;
    //     
    //     _triggerOverkill =
    //         true;
    //     
    //     GD.Print(
    //         $"Overkill! WasKilled={result.WasTargetKilled} " +
    //         $"Blocked={result.BlockedDamage} " +
    //         $"Unblocked={result.UnblockedDamage}");
    //     
    //     if (triggerOverkill &&
    //         dealer?.Player.Character is Character.Tidus tidus)
    //         tidus.PlayVfxOnTarget(
    //             target,
    //             "res://Tidus/scenes/vfx.tscn",
    //             "overkill"
    //         );
    // }
    
    public override bool TryModifyRewards(
        Player player,
        List<Reward> rewards,
        AbstractRoom? room)
    {
        if (player != Owner)
            return false;

        if (room == null)
            return false;

        if (!room.RoomType.IsCombatRoom())
            return false;

        if (!_triggerOverkill)
            return false;

        rewards.Add(
            new GoldReward(
                10,
                player
            )
        );
        
        if (room?.RoomType == RoomType.Boss)
            rewards.Add(new RelicReward(player));
        return true;
    }
    
    public override bool TryModifyCardRewardOptions(
        Player player,
        List<CardCreationResult> rewardOptions,
        CardCreationOptions creationOptions)
    {
        if (player != Owner)
            return false;
        if (!_triggerOverkill)
            return false;
/*
 * Only modify encounter card rewards.
 */
        if (creationOptions.Source !=
            CardCreationSource.Encounter)
        {
            return false;
        }
        if (!creationOptions.Flags.HasFlag(
                CardCreationFlags.IsCardReward))
        {
            return false;
        }
        if (!creationOptions.Flags.HasFlag(
                CardCreationFlags.IsFromCombat))
        {
            return false;
        }
/*
 * Prevent this newly generated card from recursively
 * running reward-modification hooks.
 */
        CardCreationOptions extraCardOptions =
            new CardCreationOptions(
                    creationOptions.CardPools,
                    CardCreationSource.Other,
                    creationOptions.RarityOdds,
                    creationOptions.CardPoolFilter)
                .WithFlags(
                    CardCreationFlags.NoModifyHooks |
                    CardCreationFlags.NoCardPoolModifications);
        CardModel? additionalCard =
            CardFactory.CreateForReward(
                    player,
                    1,
                    extraCardOptions)
                .FirstOrDefault()
                ?.Card;
        if (additionalCard == null)
            return false;
        CardCreationResult additionalResult =
            new(additionalCard);
        additionalResult.ModifyCard(
            additionalCard,
            this);
        rewardOptions.Add(
            additionalResult);
        return true;
    }
}