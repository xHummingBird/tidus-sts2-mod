using MegaCrit.Sts2.Core.Entities.Powers;

namespace Tidus.TidusCode.Powers;

public class GoldenGoalPower : TidusPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}