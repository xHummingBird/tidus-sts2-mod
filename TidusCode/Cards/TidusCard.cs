using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Tidus.TidusCode.Character;
using Tidus.TidusCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;

namespace Tidus.TidusCode.Cards;

[Pool(typeof(TidusCardPool))]
public abstract class TidusCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    
    protected override void AddExtraArgsToDescription(
        LocString description)
    {
        base.AddExtraArgsToDescription(description);

        description.Add(
            "BlitzIcon",
            "[img]res://Tidus/images/charui/blitz_icon_small.png[/img]");
    }
}