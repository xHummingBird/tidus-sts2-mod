using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Ancient;

public class EnergyRain() : TidusCard(0, CardType.Attack,
    CardRarity.Ancient, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(28m, ValueProp.Move),
        new PowerVar<DelayPower>(1m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DelayPower>()
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var enemies = CombatState.HittableEnemies.ToList();
        var leftMostEnemy = enemies[0];
        await PowerCmd.Remove<OverdriveReadyPower>(Owner.Creature);
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        if (ownerCreature != null && Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();
            SfxCmd.Play("res://Tidus/sfx/Tidus/limit_thunder_2.wav");
            tidus.PlayAnimation(ownerCreature, "overdrive_cast");
            await tidus.Delay(ownerCreature, 0.6f);
            await tidus.DashTo(ownerCreature, leftMostEnemy, distance: 200f, durationSeconds: 0.20F, overrideAnim: "energy_rain", normalSpeed: true);
            await tidus.Delay(ownerCreature, 0.2f, true);
            TidusExtensions.CombatHelpers.TidusFakeHit(ownerCreature, leftMostEnemy, "res://Tidus/sfx/Tidus/energy_rain_1.wav", "res://Tidus/sfx/Tidus/kick_1.wav", hitVfx: "hit");
            await tidus.Delay(ownerCreature, 0.2f, true);
            SfxCmd.Play("res://Tidus/sfx/Tidus/energy_rain_2.wav");
            await tidus.Delay(ownerCreature, 0.4f, true);
            SfxCmd.Play("res://Tidus/sfx/Tidus/energy_rain_3.wav");
            AudioHelper.PlayRandomLastHit();
            await tidus.Delay(ownerCreature, 0.533f, true);
            foreach (var enemy in enemies)
            {
                tidus.PlayVfxOnTarget(
                    enemy,
                    "res://Tidus/scenes/vfx.tscn",
                    "hit"
                );
            }
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null, "res://Tidus/sfx/Tidus/energy_rain_hit.wav")
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.45f, true);
            await tidus.Retreat(ownerCreature, duration: 0.2F);
        }
        else  await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Tidus/sfx/Tidus/energy_rain_hit.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        await PowerCmd.Apply<DelayPower>(choiceContext, CombatState.HittableEnemies, 2, Owner.Creature, this);
        await PowerCmd.Apply<DelayPower>(choiceContext, CombatState.HittableEnemies, DynamicVars["DelayPower"].BaseValue, Owner.Creature, this);
        OverdriveManager.SetOverdrive(Owner, 0);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(8m);
        DynamicVars["DelayPower"].UpgradeValueBy(1m);
    }
}