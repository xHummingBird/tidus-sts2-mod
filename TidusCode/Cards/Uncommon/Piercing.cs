using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
 
namespace Tidus.TidusCode.Cards.Uncommon;

public class Piercing() : TidusCard(
    1,
    CardType.Skill,
    CardRarity.Uncommon,
    TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<AmplifyDelayPower>(1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DelayPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PowerCmd.Apply<AmplifyDelayPower>(
            choiceContext,
            play.Target,
            DynamicVars["AmplifyDelayPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["AmplifyDelayPower"].UpgradeValueBy(1m);
    }
}