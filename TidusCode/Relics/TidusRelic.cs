using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Tidus.TidusCode.Character;
using Tidus.TidusCode.Extensions;
using Godot;

namespace Tidus.TidusCode.Relics;

[Pool(typeof(TidusRelicPool))]
public abstract class TidusRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".BigRelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}