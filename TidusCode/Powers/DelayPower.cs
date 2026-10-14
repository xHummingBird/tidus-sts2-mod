using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Tidus.TidusCode.Powers;

public sealed class DelayPower : TidusPower
{
    private const string DamageDecrease = "DamageDecrease";
    private const string DamageIncrease = "DamageIncrease";

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(DamageDecrease, 0.75m),
        new DynamicVar(DamageIncrease, 1.25m)
    ];

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (!props.IsPoweredAttack())
        {
            return 1m;
        }

        decimal multiplier = 1m;

        // Weak portion:
        // The creature with Delay deals 25% less Attack damage.
        if (dealer == Owner)
        {
            decimal damageDecrease = DynamicVars[DamageDecrease].BaseValue;

            if (dealer.HasPower<AmplifyDelayPower>())
            {
                damageDecrease = 0.5m;
            }

            multiplier *= damageDecrease;
        }

        // Vulnerable portion:
        // The creature with Delay takes 25% more Attack damage,
        // but only if the attacker has Haste.
        if (target == Owner &&
            dealer != null &&
            dealer.HasPower<HastePower>())
        {
            decimal damageIncrease =
                DynamicVars[DamageIncrease].BaseValue;

            decimal breakDamageLimit =
                dealer.GetPowerAmount<BreakDamageLimitPower>();

            if (target.HasPower<AmplifyDelayPower>())
            {
                damageIncrease = 1.5m;
            }

            decimal totalBonus =
                (damageIncrease - 1m)
                + (breakDamageLimit / 100m);

            totalBonus *= 2m;

            multiplier *= (1m + totalBonus);
        }

        return multiplier;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            await PowerCmd.TickDownDuration(this);
        }
    }
}