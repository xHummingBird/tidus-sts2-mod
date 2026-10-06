using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using Tidus.TidusCode.Extensions;

namespace Tidus.TidusCode.Relics;

public class TamingSword : TidusRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        TidusStaticHoverTips.Overkill
    ];
}