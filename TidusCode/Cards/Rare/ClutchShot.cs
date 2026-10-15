using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Rare;

public class ClutchShot() : TidusCard(
    2,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy),
    IBlitz
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<HastePower>();
    
    private const string Increase = "Increase";

    private const int BaseDamage = 13;
    private const int UpgradedBaseDamage = 15;

    private int _currentDamage = BaseDamage;
    private int _increasedDamage;

    [SavedProperty]
    public int CurrentDamage
    {
        get => _currentDamage;

        set
        {
            AssertMutable();
            _currentDamage = value;
            DynamicVars.Damage.BaseValue = _currentDamage;
        }
    }

    [SavedProperty]
    public int IncreasedDamage
    {
        get => _increasedDamage;

        set
        {
            AssertMutable();
            _increasedDamage = value;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(CurrentDamage, ValueProp.Move),
        new IntVar(Increase, 5m)
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
            await ExecuteAttack(
                choiceContext,
                play,
                play.Target);
            int increaseAmount =
                DynamicVars[Increase].IntValue;

            BuffFromBlitz(increaseAmount);

            if (DeckVersion is ClutchShot deckVersion)
            {
                deckVersion.BuffFromBlitz(increaseAmount);
            }
        }

        /*
         * Blitz permanently increases this card's damage.
         * A normal play also triggers Blitz while Haste is active.
         */
        if (isBlitzPlay || hasHaste)
        {
            await OnBlitz(
                choiceContext,
                play.Target);
        }
    }

    public Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        int increaseAmount =
            DynamicVars[Increase].IntValue;

        BuffFromBlitz(increaseAmount);

        if (DeckVersion is ClutchShot deckVersion)
        {
            deckVersion.BuffFromBlitz(increaseAmount);
        }

        return Task.CompletedTask;
    }

    private async Task ExecuteAttack(
        PlayerChoiceContext choiceContext,
        CardPlay play,
        Creature target)
    {
        Creature? ownerCreature =
            Owner?.Creature;

        CenterCardCinematic.Start(
            RunManager.Instance.NetService.NetId);
        
        if (ownerCreature != null &&
                Owner?.Character is Character.Tidus tidus)
            {
                AudioHelper.PlayRandomPhrase();

                tidus.PlayAnimation(
                    ownerCreature,
                    "kick");

                await tidus.Delay(
                    ownerCreature,
                    0.25f);

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
                    "blitzball_hit_1");

                await DamageCmd.Attack(
                        DynamicVars.Damage.BaseValue)
                    .FromCard(this, play)
                    .WithHitFx(
                        null,
                        "res://Tidus/sfx/hit_2.wav")
                    .Targeting(target)
                    .Execute(choiceContext);

                await tidus.Delay(
                    ownerCreature,
                    0.15f);
            }
            else
                await DamageCmd.Attack(
                        DynamicVars.Damage.BaseValue)
                    .FromCard(this, play)
                    .WithHitFx(
                        null,
                        "res://Tidus/sfx/hit_2.wav")
                    .Targeting(target)
                    .Execute(choiceContext);
            CenterCardCinematic.End(
                                    RunManager.Instance.NetService.NetId);
        }
    

    private void BuffFromBlitz(
        int extraDamage)
    {
        IncreasedDamage += extraDamage;
        UpdateDamage();
    }

    private void UpdateDamage()
    {
        int baseDamage =
            IsUpgraded
                ? UpgradedBaseDamage
                : BaseDamage;

        CurrentDamage =
            baseDamage + IncreasedDamage;
    }

    protected override void OnUpgrade()
    {
        DynamicVars[Increase].UpgradeValueBy(1m);
        UpdateDamage();
    }

    protected override void AfterDowngraded()
    {
        UpdateDamage();
    }
}