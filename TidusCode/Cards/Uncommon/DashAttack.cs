using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Uncommon;

public class DashAttack() : TidusCard(1, CardType.Attack,
    CardRarity.Uncommon, TargetType.AllEnemies)
{
    private int PlayCountThisTurn =>
        CombatManager.Instance.History.CardPlaysFinished
            .Count(e =>
                e.HappenedThisTurn(base.CombatState) &&
                e.CardPlay.Player == base.Owner);
    
    protected override bool ShouldGlowGoldInternal => PlayCountThisTurn == 0;
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<DelayPower>(1m),
        new PowerVar<HastePower>(1m)
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DelayPower>()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var ownerCreature = Owner?.Creature;
        var enemies = CombatState.HittableEnemies.ToList();
        var leftMostEnemy = enemies.First();
        var rightMostEnemy = enemies.Last();

        if (ownerCreature != null && Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();
            await tidus.DashTo(ownerCreature, leftMostEnemy, distance: 300f, durationSeconds: 0.20F);
            tidus.PlayAnimation(ownerCreature, "dash_attack");
            await tidus.Delay(ownerCreature, 0.05f);
            SfxCmd.Play("res://Tidus/sfx/swing_2.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_2.wav");
            tidus.DashTo(ownerCreature, rightMostEnemy, distance: -200f, durationSeconds: 0.05F, overrideAnim: "dash_attack");
            foreach (var enemy in enemies)
            {
                tidus.PlayVfxOnTarget(
                    enemy,
                    "res://Tidus/scenes/vfx.tscn",
                    "hit"
                );
            }
            await CommonActions.CardAttack(this, play.Target)
                .WithHitFx(null)
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.15f);
            await tidus.Retreat(ownerCreature, duration: 0.2F);
        }
        else  await CommonActions.CardAttack(this, play.Target)
            .WithHitFx(null, "res://Tidus/sfx/Tidus/slash_1.wav")
            .Execute(choiceContext);
        await PowerCmd.Apply<DelayPower>(choiceContext, CombatState.HittableEnemies, 1, Owner.Creature, this);
        if (PlayCountThisTurn == 0)
        {
            await PowerCmd.Apply<HastePower>(choiceContext, Owner.Creature, DynamicVars["HastePower"].BaseValue, Owner.Creature, this);
        }
        
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["HastePower"].UpgradeValueBy(1m);
    }
}