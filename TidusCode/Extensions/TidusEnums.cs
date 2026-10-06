using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Tidus.TidusCode.Extensions;

public class TidusEnums
{
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Blitz;
    
    [CustomEnum]
    public static AutoPlayType BlitzDiscard;
}