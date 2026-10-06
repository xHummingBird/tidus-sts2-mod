using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Relics;

public class Caladbolg : OverdriveRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    
    protected override int OverdrivePerTurn => 5;
    protected override int OverdrivePerAttack => 6;
    protected override int OverdrivePerBlitzDiscard => 9;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        TidusStaticHoverTips.Overkill
    ];
}