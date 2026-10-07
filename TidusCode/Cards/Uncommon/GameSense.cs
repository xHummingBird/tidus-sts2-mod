using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;

namespace Tidus.TidusCode.Cards.Uncommon;

public class GameSense() : TidusCard(
    1,
    CardType.Power,
    CardRarity.Uncommon,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<GameSensePower>(1)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<GameSensePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["GameSensePower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["GameSensePower"].UpgradeValueBy(1m);
    }
}