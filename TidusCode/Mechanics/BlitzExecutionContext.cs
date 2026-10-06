using MegaCrit.Sts2.Core.Models;

namespace Tidus.TidusCode.Mechanics;

public static class BlitzExecutionContext
{
    private static readonly AsyncLocal<CardModel?> ActiveCard =
        new();

    public static bool IsResolving(
        CardModel card)
    {
        return ReferenceEquals(
            ActiveCard.Value,
            card);
    }

    public static bool IsBlitzPlay(
        CardModel card)
    {
        return IsResolving(card)
               && card is IBlitz;
    }

    public static async Task Run(
        CardModel card,
        Func<Task> action)
    {
        CardModel? previousCard =
            ActiveCard.Value;

        ActiveCard.Value = card;

        try
        {
            await action();
        }
        finally
        {
            ActiveCard.Value = previousCard;
        }
    }
}