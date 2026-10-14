using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Mechanics;

public static class BlitzManager
{
    [HarmonyPatch(
        typeof(CardCmd),
        nameof(CardCmd.DiscardAndDraw),
        typeof(PlayerChoiceContext),
        typeof(IEnumerable<CardModel>),
        typeof(int))]
    public static class CardCmdDiscardAndDrawPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(
            PlayerChoiceContext choiceContext,
            IEnumerable<CardModel> cardsToDiscard,
            int cardsToDraw,
            ref Task __result)
        {
            __result = DiscardAndDrawWithBlitz(
                choiceContext,
                cardsToDiscard,
                cardsToDraw);

            return false;
        }

        private static async Task DiscardAndDrawWithBlitz(
            PlayerChoiceContext choiceContext,
            IEnumerable<CardModel> cardsToDiscard,
            int cardsToDraw)
        {
            if (CombatManager.Instance.IsOverOrEnding)
                return;

            List<CardModel> discardCards =
                cardsToDiscard.ToList();

            if (discardCards.Count == 0)
                return;

            CardModel firstCard =
                discardCards[0];

            ICombatState combatState =
                firstCard.CombatState ??
                firstCard.Owner.Creature.CombatState;

            CardPile discardPile =
                PileType.Discard.GetPile(
                    firstCard.Owner);

            List<CardModel> slyCards = [];
            List<CardModel> blitzCards = [];

            foreach (CardModel card in discardCards)
            {
                /*
                 * Capture Sly and Blitz before moving the card.
                 *
                 * A temporary Blitz keyword may disappear once the card
                 * leaves the hand, so eligibility must be recorded here.
                 */
                if (card.IsSlyThisTurn)
                {
                    slyCards.Add(card);
                }

                if (IsBlitzCard(card))
                {
                    blitzCards.Add(card);
                }

                await CardPileCmd.Add(
                    card,
                    discardPile);

                CombatManager.Instance.History.CardDiscarded(
                    combatState,
                    card);

                await Hook.AfterCardDiscarded(
                    combatState,
                    choiceContext,
                    card);
            }

            discardPile.InvokeContentsChanged();

            if (cardsToDraw > 0)
            {
                await CardPileCmd.Draw(
                    choiceContext,
                    cardsToDraw,
                    firstCard.Owner);
            }

            /*
             * Preserve the original Sly behavior.
             */
            foreach (CardModel card in slyCards)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                    return;

                if (card.Pile?.Type != PileType.Discard)
                    continue;

                await CardCmd.AutoPlay(
                    choiceContext,
                    card,
                    target: null,
                    type: AutoPlayType.SlyDiscard);
            }

            /*
             * Resolve Blitz after drawing, matching the timing used
             * by the original Sly implementation.
             */
            foreach (CardModel card in blitzCards)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                    return;

                if (!CanResolveCapturedBlitz(card))
                    continue;

                Creature ownerCreature =
                    card.Owner.Creature;

                decimal strengthAmount =
                    ownerCreature.GetPowerAmount<GoldenGoalPower>();

                decimal blockAmount =
                    ownerCreature.GetPowerAmount<ClutchDefensePower>();

                if (card is IBlitz)
                {
                    await BlitzExecutionContext.Run(
                        card,
                        () => CardCmd.AutoPlay(
                            choiceContext,
                            card,
                            target: null,
                            type: TidusEnums.BlitzDiscard));
                }
                else
                {
                    /*
                     * A normal card that temporarily gained Blitz
                     * resolves its ordinary OnPlay effect.
                     */
                    await CardCmd.AutoPlay(
                        choiceContext,
                        card,
                        target: null,
                        type: TidusEnums.BlitzDiscard);
                }

                /*
                 * The autoplayed card may have ended combat.
                 */
                if (CombatManager.Instance.IsOverOrEnding)
                    return;

                if (strengthAmount > 0)
                {
                    await PowerCmd.Apply<StrengthPower>(
                        choiceContext,
                        ownerCreature,
                        strengthAmount,
                        ownerCreature,
                        null);
                }

                if (blockAmount > 0)
                {
                    await CreatureCmd.GainBlock(
                        ownerCreature,
                        blockAmount,
                        ValueProp.Unpowered,
                        null);
                }
            }
        }

        private static bool IsBlitzCard(
            CardModel card)
        {
            return card.Keywords.Contains(
                TidusEnums.Blitz);
        }

        private static bool CanResolveCapturedBlitz(
            CardModel card)
        {
            /*
             * Do not check the Blitz keyword again here.
             *
             * The card was already confirmed as Blitz before it left
             * the hand. Dynamically granted keywords may disappear
             * after the card enters the discard pile.
             */
            if (card.Pile?.Type != PileType.Discard)
                return false;

            if (card.Owner.Creature.IsDead)
                return false;

            if (CombatManager.Instance.IsOverOrEnding)
                return false;

            return true;
        }
    }
}