using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Relics;

public class Brotherhood : OverdriveRelicBase
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        TidusStaticHoverTips.Overkill
    ];
}