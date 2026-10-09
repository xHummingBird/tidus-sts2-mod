using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;

namespace Tidus.TidusCode.Cards.Uncommon;

public class Playbook() : TidusCard(
    1,
    CardType.Power,
    CardRarity.Uncommon,
    TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];


    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? attackCard =
            CardFactory.GetDistinctForCombat(
                    Owner,
                    from card in Owner.Character.CardPool.GetUnlockedCards(
                        Owner.UnlockState,
                        Owner.RunState.CardMultiplayerConstraint)
                    where card.Type is CardType.Attack
                    select card,
                    1,
                    Owner.RunState.Rng.CombatCardGeneration)
                .FirstOrDefault();

        if (attackCard == null)
            return;

        attackCard.SetToFreeThisTurn();

        await CardPileCmd.AddGeneratedCardToCombat(
            attackCard,
            PileType.Hand,
            Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}