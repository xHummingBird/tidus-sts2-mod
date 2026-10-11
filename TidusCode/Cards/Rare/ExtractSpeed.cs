using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Rare;

public sealed class ExtractSpeed() : TidusCard(
    0,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DelayPower>(),
        HoverTipFactory.FromPower<HastePower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<DelayPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        decimal totalDelay = CombatState?.Enemies
                                 .Where(enemy => enemy.IsAlive)
                                 .Sum(enemy => enemy.GetPowerAmount<DelayPower>())
                             ?? 0m;

        if (totalDelay <= 0m)
        {
            return;
        }

        await PowerCmd.Apply<HastePower>(choiceContext, Owner.Creature, totalDelay, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}