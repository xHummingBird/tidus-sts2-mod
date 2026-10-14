using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Cards.Rare;
using Tidus.TidusCode.Cards.Uncommon;

namespace Tidus.TidusCode.Powers;

public class HastePower : TidusPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
    
    public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Side)
            return;
        
        await PowerCmd.Decrement(this);
        var ownerCreature = Owner;
        var tidus = Owner?.Player.Character as Character.Tidus;
        if (ownerCreature != null && tidus != null)
        {
            tidus.PlayAnimation(ownerCreature, "idle");
        }
    }
    
    public override bool TryModifyEnergyCostInCombatLate(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (card.Owner.Creature != Owner)
            return false;

        if (card is not QuickHit)
            return false;

        bool inPlayablePile =
            card.Pile?.Type is
                PileType.Hand or
                PileType.Play;

        if (!inPlayablePile)
            return false;

        modifiedCost = 0;
        return true;
    }

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        var ownerCreature = Owner;
        var tidus = Owner?.Player.Character as Character.Tidus;
        if (ownerCreature != null && tidus != null)
        {
            tidus.PlayAnimation(ownerCreature, "idle");
        }
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (dealer != Owner)
        {
            return 1m;
        }

        // Only SpiralCut receives the damage bonus.
        if (cardSource is not SpiralCut)
        {
            return 1m;
        }
        
        if (cardSource.IsUpgraded)
            return 1.66m;
        
        return 1.5m;
    }
}