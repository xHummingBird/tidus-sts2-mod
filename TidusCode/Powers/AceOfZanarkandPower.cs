using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace Tidus.TidusCode.Powers;

public class AceOfZanarkandPower : TidusPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override bool ShouldFlush(Player player)
    {
        if (player != base.Owner.Player)
        {
            return true;
        }
        return false;
    }
}