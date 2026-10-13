using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models.Cards;

namespace Tidus.TidusCode.Relics;

public class Apocalypse : TidusRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    
    public override Task BeforeCombatStart()
    {
        Flash();
        return Task.CompletedTask;
    }
}