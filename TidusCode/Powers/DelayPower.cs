namespace Tidus.TidusCode.Powers;

public class DelayPower : TidusPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;
}