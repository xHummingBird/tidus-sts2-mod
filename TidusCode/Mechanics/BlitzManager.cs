using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Tidus.TidusCode.Character;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Mechanics;

public static class BlitzManager
{
    [HarmonyPatch(
        typeof(CardCmd),
        nameof(CardCmd.DiscardAndDraw))]
    public static class CardCmdDiscardAndDrawPatch
    {
        [HarmonyPrefix]
        public static void Prefix(
            ref IEnumerable<CardModel> cardsToDiscard,
            out List<CardModel> __state)
        {
            List<CardModel> cards =
                cardsToDiscard.ToList();

            // Both the original method and this patch use the same list.
            cardsToDiscard = cards;

            /*
             * Any card with the Blitz keyword qualifies.
             *
             * Do not require IBlitz here because a power or card may
             * temporarily give Blitz to an ordinary card.
             */
            __state = cards
                .Where(IsBlitzCard)
                .ToList();
        }

        [HarmonyPostfix]
        public static void Postfix(
            PlayerChoiceContext choiceContext,
            List<CardModel> __state,
            ref Task __result)
        {
            __result = ResolveBlitzAfterDiscard(
                __result,
                choiceContext,
                __state);
        }

        private static async Task ResolveBlitzAfterDiscard(
            Task originalTask,
            PlayerChoiceContext choiceContext,
            IReadOnlyList<CardModel> blitzCards)
        {
            await originalTask;

            if (CombatManager.Instance.IsOverOrEnding)
                return;

            foreach (CardModel card in blitzCards)
            {
                if (!CanTriggerBlitz(card))
                    continue;

                /*
                 * Only establish the special execution context for cards
                 * that actually implement a separate OnBlitz effect.
                 *
                 * Ordinary cards given Blitz dynamically are autoplayed
                 * without this context and therefore execute OnPlay normally.
                 */
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
                    await CardCmd.AutoPlay(
                        choiceContext,
                        card,
                        target: null,
                        type: TidusEnums.BlitzDiscard);
                }
            }
        }
    }

    private static bool IsBlitzCard(
        CardModel card)
    {
        if (card.Owner?.Character is not Character.Tidus)
            return false;

        return card.Keywords.Contains(
            TidusEnums.Blitz);
    }

    private static bool CanTriggerBlitz(
        CardModel card)
    {
        if (card.Owner?.Character is not Character.Tidus)
            return false;

        if (!card.Keywords.Contains(TidusEnums.Blitz))
            return false;

        if (card.Pile?.Type != PileType.Discard)
            return false;

        if (card.Owner.Creature.IsDead)
            return false;

        if (CombatManager.Instance.IsOverOrEnding)
            return false;

        return true;
    }
}