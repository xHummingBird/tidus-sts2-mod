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
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Rare;

public class SphereShot() : TidusCard(
    0,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy),
    IBlitz
{
    private const string MinimumDamage = "MinimumDamage";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11m, ValueProp.Move),
        new DynamicVar(MinimumDamage, 3m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        TidusEnums.Blitz
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {

        int maximumDamage = DynamicVars.Damage.IntValue;
        int finalDamage;

        if (BlitzExecutionContext.IsBlitzPlay(this))
        {
            // Blitz normally deals maximum damage.
            finalDamage = maximumDamage;

            // Haste combines the normal random effect with the Blitz effect.
            if (Owner.HasPower<HastePower>())
            {
                finalDamage += RollRandomDamage();
            }
        }
        else
        {
            // Normal play deals random damage.
            finalDamage = RollRandomDamage();

            // Haste also adds the Blitz maximum damage.
            if (Owner.HasPower<HastePower>())
            {
                finalDamage += maximumDamage;
            }
        }
        var ownerCreature = Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomPhrase();
            tidus.PlayAnimation(ownerCreature, "kick");
            await tidus.Delay(ownerCreature, 0.25f);
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_swing.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_impact.wav");
            await tidus.Delay(ownerCreature, 0.05f);
            tidus.PlayVfxOnTarget(
                play.Target,
                "res://Tidus/scenes/vfx.tscn",
                "blitzball_hit_1"
            );
            await DamageCmd.Attack(finalDamage)
                .FromCard(this, play)
                .WithHitFx(null, "res://Tidus/sfx/hit_2.wav")
                .Targeting(play.Target)
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.15f);
        }
        else
            await DamageCmd.Attack(finalDamage)
                .FromCard(this, play)
                .WithHitFx(null, "res://Tidus/sfx/hit_2.wav")
                .Targeting(play.Target)
                .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    public async Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
    }

    private int RollRandomDamage()
    {
        int minimumDamage = DynamicVars[MinimumDamage].IntValue;
        int maximumDamage = DynamicVars.Damage.IntValue;

        return Owner.RunState.Rng.CombatCardSelection.NextInt(
            minimumDamage,
            maximumDamage + 1);
    }

    protected override void OnUpgrade()
    {
        DynamicVars[MinimumDamage].UpgradeValueBy(2m);
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}