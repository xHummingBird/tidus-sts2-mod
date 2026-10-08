using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using Tidus.TidusCode.Cards.Rare;

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

}