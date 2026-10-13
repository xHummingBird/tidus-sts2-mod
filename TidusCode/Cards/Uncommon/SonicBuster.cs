using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Uncommon;

public class SonicBuster() : TidusCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new CalculationBaseVar(8m),
        new ExtraDamageVar(4m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? _) => CombatManager.Instance.History.Entries.OfType<CardDiscardedEntry>().Count((CardDiscardedEntry e) => e.HappenedThisTurn(card.CombatState) && e.Actor == card.Owner.Creature))
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var ownerCreature =
            Owner?.Creature;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null &&
            Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomLastHit();
            tidus.PlayAnimation(ownerCreature, "sonic_buster");
            await tidus.DashTo(ownerCreature, play.Target, distance: 300f, durationSeconds: 0.2f, overrideAnim: null);
            SfxCmd.Play("res://Tidus/sfx/swing_1.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_1.wav");
            await tidus.Delay(ownerCreature, 0.05f);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, hitVfx: "hit");
            await tidus.Delay(ownerCreature, 0.3f);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, play.Target, "res://Tidus/sfx/swing_1.wav", "res://Tidus/sfx/Tidus/slash_3.wav", hitVfx: "hit");
            await tidus.Delay(ownerCreature, 0.3f);
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Tidus/sfx/Tidus/slash_2.wav")
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.2f);
            await tidus.Retreat(ownerCreature, duration: 0.2f);
        }
        else
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(
                    null,
                    "res://Tidus/sfx/Tidus/slash_3.wav")
                .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    protected override void OnUpgrade()
    {
        base.DynamicVars.CalculationBase.UpgradeValueBy(3m);
        base.DynamicVars.ExtraDamage.UpgradeValueBy(1m);
    }
}