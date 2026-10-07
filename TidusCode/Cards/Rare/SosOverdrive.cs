using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;

namespace Tidus.TidusCode.Cards.Rare;

public class SosOverdrive() : TidusCard(2, CardType.Skill,
    CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new BlockVar(25, ValueProp.Move),
        new DynamicVar("Overdrive", 25)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [
            CardKeyword.Exhaust
        ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<OverdriveReadyPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, play);
        OverdriveManager.GainOverdrive(Owner, DynamicVars["Overdrive"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(7m);
        DynamicVars["Overdrive"].UpgradeValueBy(7m);
    }
}