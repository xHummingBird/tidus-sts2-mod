using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Ancient;

public class Overdrive() : TidusCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<OverdriveReadyPower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(13, ValueProp.Move),
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        
        var js = CombatState.CreateCard<JechtShotTwo>(base.Owner);
        var er = CombatState.CreateCard<EnergyRain>(base.Owner);
        var ba = CombatState.CreateCard<BlitzAce>(base.Owner);
        
      
            if (IsUpgraded)
            {
                CardCmd.Upgrade(js);
                CardCmd.Upgrade(er);
                CardCmd.Upgrade(ba);
            }

            List<CardModel> cards = [js, er, ba];
            
            CardModel selectedFinisher = await CardSelectCmd.FromChooseACardScreen(choiceContext, cards.ToList(), base.Owner, canSkip: false);
            await CardCmd.AutoPlay(choiceContext, selectedFinisher, null);
         
    }
    protected override void OnUpgrade()
    {
        
    }
}