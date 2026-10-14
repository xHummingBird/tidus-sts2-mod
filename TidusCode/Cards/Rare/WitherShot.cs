using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Rare;

public class WitherShot() : TidusCard(
    0,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy),
    IBlitz
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<HastePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<DelayPower>(1m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        TidusEnums.Blitz
    ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<DelayPower>()
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        bool isBlitzPlay =
            BlitzExecutionContext.IsBlitzPlay(this);

        bool hasHaste =
            Owner.HasPower<HastePower>();

        /*
         * Normal play performs the Attack effect.
         * A Blitz play also performs the Attack effect while Haste is active.
         */
        if (!isBlitzPlay || hasHaste)
        {
            ArgumentNullException.ThrowIfNull(play.Target);

            await ExecuteAttack(
                choiceContext,
                play.Target);
        }

        /*
         * Blitz grants Free Skill.
         * A normal play also triggers Blitz while Haste is active.
         */
        if (isBlitzPlay || hasHaste)
        {
            await OnBlitz(
                choiceContext,
                play.Target);
        }
    }

    public async Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        await PowerCmd.Apply<DelayPower>(choiceContext, CombatState.HittableEnemies, DynamicVars["DelayPower"].BaseValue,
            Owner.Creature, this);
    }

    private async Task ExecuteAttack(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        Dictionary<PowerModel, int> debuffAmounts = (from p in target.Powers
            where p.TypeForCurrentAmount == PowerType.Debuff
            select ((PowerModel)p.ClonePreservingMutability(), Amount: p.Amount)).ToDictionary();
        foreach (KeyValuePair<PowerModel, int> item in debuffAmounts)
        {
            PowerModel key = item.Key;
            ITemporaryPower temporaryPower = key as ITemporaryPower;
            if (temporaryPower != null)
            {
                KeyValuePair<PowerModel, int> keyValuePair = debuffAmounts.FirstOrDefault<KeyValuePair<PowerModel, int>>((KeyValuePair<PowerModel, int> p) => p.Key.Id == temporaryPower.InternallyAppliedPower.Id);
                if (keyValuePair.Key != null)
                {
                    debuffAmounts[keyValuePair.Key] += item.Value;
                }
            }
        }
        
        Creature ownerCreature = Owner.Creature;

        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        
        if (ownerCreature != null && Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();
            tidus.PlayAnimation(ownerCreature, "throw");
            await tidus.Delay(ownerCreature, 0.1f);
            SfxCmd.Play("res://Tidus/sfx/Tidus/blitz_swing.wav");
            await tidus.Delay(ownerCreature, 0.05f);
            tidus.PlayVfxOnTarget(
                target,
                "res://Tidus/scenes/vfx.tscn",
                "blitzball_hit_3"
            );
        }
        await CommonActions.CardAttack(this, target)
            .WithHitFx(null, "res://Tidus/sfx/hit_2.wav")
            .Execute(choiceContext);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        foreach (Creature enemy in base.CombatState.HittableEnemies)
        {
            if (enemy == target)
            {
                continue;
            }
            foreach (KeyValuePair<PowerModel, int> item2 in debuffAmounts)
            {
                if (item2.Value != 0)
                {
                    PowerModel powerModel = PowerCmd.FindExistingInstanceForStacking(item2.Key, enemy, item2.Key.Applier);
                    if (powerModel != null)
                    {
                        await PowerCmd.ModifyAmount(choiceContext, powerModel, item2.Value, item2.Key.Applier, this);
                        continue;
                    }
                    PowerModel power = (PowerModel)item2.Key.ClonePreservingMutability();
                    await PowerCmd.Apply(choiceContext, power, enemy, item2.Value, item2.Key.Applier, this);
                }
            }
        }
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
        DynamicVars["DelayPower"].UpgradeValueBy(1m);
    }
}