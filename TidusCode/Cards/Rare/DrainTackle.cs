using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Rare;

public class DrainTackle() : TidusCard(1, CardType.Attack,
    CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11m, ValueProp.Move),
        new HealVar(2)
    ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var heal = false;
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (play.Target.HasPower<DelayPower>())
            heal = true;
        if (ownerCreature != null && Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();
            tidus.PlayAnimation(ownerCreature, "tackle");
            await tidus.DashTo(ownerCreature, play.Target, distance: 250f, durationSeconds: 0.2F, overrideAnim: null);
            SfxCmd.Play("res://Tidus/sfx/Tidus/kick_up.wav");
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Tidus/sfx/Tidus/kick_1.wav")
                .Execute(choiceContext);
            await tidus.Retreat(ownerCreature, duration: 0.2F);
        }
        else  await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Tidus/sfx/Tidus/slash_1.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        if (heal)
            await CreatureCmd.Heal(Owner.Creature, DynamicVars["Heal"].BaseValue, true);

        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5m);
        DynamicVars.Heal.UpgradeValueBy(1);
    }
}