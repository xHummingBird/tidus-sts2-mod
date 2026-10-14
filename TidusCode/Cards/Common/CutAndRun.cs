using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Common;

public class CutAndRun() : TidusCard(
    1,
    CardType.Attack,
    CardRarity.Common,
    TargetType.AllEnemies),
    IBlitz
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<HastePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move)
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        TidusEnums.Blitz
    ];
    
    public async Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (BlitzExecutionContext.IsBlitzPlay(this))
        {
            CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);

            await ExecuteAttack(choiceContext, play);

            CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            return;
        }

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);

        await ExecuteAttack(choiceContext, play);

        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
    }
    
    private async Task ExecuteAttack(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        var totalDamage = DynamicVars.Damage.BaseValue;

        if (Owner.HasPower<HastePower>())
            totalDamage *= 2;

        var ownerCreature = Owner?.Creature;
        var enemies = CombatState.HittableEnemies.ToList();

        if (enemies.Count == 0)
            return;

        var leftMostEnemy = enemies.First();
        var rightMostEnemy = enemies.Last();

        if (ownerCreature != null &&
            Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();

            await tidus.DashTo(
                ownerCreature,
                leftMostEnemy,
                distance: 300f,
                durationSeconds: 0.20f);

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
                    "hit");
            }
            
            await DamageCmd.Attack(totalDamage)
                .FromCard(this, play)
                .TargetingAllOpponents(CombatState)
                .WithValueProp(ValueProp.Move)
                .WithHitFx(null)
                .Execute(choiceContext);

            await tidus.Delay(ownerCreature, 0.15f);
            await tidus.Retreat(ownerCreature, duration: 0.2f);
        }
        else
            await DamageCmd.Attack(totalDamage)
                .FromCard(this, play)
                .TargetingAllOpponents(CombatState)
                .WithValueProp(ValueProp.Move)
                .WithHitFx(null)
                .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}