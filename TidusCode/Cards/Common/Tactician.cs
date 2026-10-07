using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Cards.Common;

public class Tactician() : TidusCard(1, CardType.Skill,
    CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DynamicVar("Overdrive", 10)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<OverdriveReadyPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        OverdriveManager.GainOverdrive(Owner, DynamicVars["Overdrive"].IntValue);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, 2, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Overdrive"].UpgradeValueBy(5m);
    }
}