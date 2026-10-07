using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;

namespace Tidus.TidusCode.Cards.Rare;

public class Initiative() : TidusCard(
    0,
    CardType.Skill,
    CardRarity.Rare,
    TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        new CardsVar(1),
        new DynamicVar("Overdrive", 10)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<OverdriveReadyPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        await PlayerCmd.GainEnergy(
            Owner,
            DynamicVars.Energy.IntValue);

        OverdriveManager.GainOverdrive(
            Owner,
            DynamicVars["Overdrive"].IntValue);

        await CardPileCmd.Draw(
            choiceContext,
            DynamicVars.Cards.IntValue,
            Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Overdrive"].UpgradeValueBy(5);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}