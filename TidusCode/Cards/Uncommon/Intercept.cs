using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Cards.Uncommon;

public class Intercept() : TidusCard(1, CardType.Skill,
    CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new BlockVar(6, ValueProp.Move),
        new PowerVar<DelayPower>(1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DelayPower>(),
        HoverTipFactory.FromPower<HastePower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        AudioHelper.PlayRandomDefend();
        await CommonActions.CardBlock(this, play);
        var powerAmount = 1;
        if (Owner.Creature.HasPower<HastePower>())
            powerAmount = 1 + DynamicVars["DelayPower"].BaseValue;
        await PowerCmd.Apply<DelayPower>(choiceContext, play.Target, powerAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(2m);
        DynamicVars["DelayPower"].UpgradeValueBy(1m);

    }
}