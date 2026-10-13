using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Tidus.TidusCode.Cards.Ancient;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Powers;

public class BlitzManiaPower : TidusPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != base.Owner.Player)
        {
            return Task.CompletedTask;
        }
        
        if (cardPlay.Card is Overdrive)
        {
            return Task.CompletedTask;
        }
        
        if (cardPlay.Card.Type != CardType.Skill)
        {
            return Task.CompletedTask;
        }
        Flash();
        CardCmd.ApplyKeyword(cardPlay.Card, TidusEnums.Blitz);
        return Task.CompletedTask;
    }
}