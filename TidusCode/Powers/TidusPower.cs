using BaseLib.Abstracts;
using BaseLib.Extensions;
using Tidus.TidusCode.Extensions;
using Godot;

namespace Tidus.TidusCode.Powers;

public abstract class TidusPower : CustomPowerModel
{
    //Loads from Tidus/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}