using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace Tidus.TidusCode.Mechanics;

public interface IBlitz
{
    Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target);
}