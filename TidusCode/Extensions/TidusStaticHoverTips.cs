using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Tidus.TidusCode.Extensions;

public static class TidusStaticHoverTips
    {
        public static readonly IHoverTip Overdrive =
            new HoverTip(
                new LocString(
                    "static_hover_tips",
                    "TIDUS_OVERDRIVE.title"),
                new LocString(
                    "static_hover_tips",
                    "TIDUS_OVERDRIVE.description"));
        
        public static readonly IHoverTip Overkill =
            new HoverTip(
                new LocString(
                    "static_hover_tips",
                    "TIDUS_OVERKILL.title"),
                new LocString(
                    "static_hover_tips",
                    "TIDUS_OVERKILL.description"));
    }
