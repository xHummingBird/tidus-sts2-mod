using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Uncommon;

public class UpAndUnder() : TidusCard(
    1,
    CardType.Attack,
    CardRarity.Uncommon,
    TargetType.AnyEnemy),
    IBlitz
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<HastePower>();

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
    ];
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        TidusEnums.Blitz
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay play)
    {
        if (BlitzExecutionContext.IsBlitzPlay(this))
        {

            if (Owner.HasPower<HastePower>())
            {
                CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
                await ExecuteAttack(
                    choiceContext,
                    play.Target);
                CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
            }

            await OnBlitz(
                choiceContext,
                play.Target);

            return;
        }
        CenterCardCinematic.Start(RunManager.Instance.NetService.NetId);
        await ExecuteAttack(
            choiceContext,
            play.Target);
        CenterCardCinematic.End(RunManager.Instance.NetService.NetId);
        
        if (Owner?.Creature.HasPower<HastePower>() == true)
        {
            await OnBlitz(choiceContext, play.Target);
        }
    }

    public async Task OnBlitz(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        await PowerCmd.Apply<FreeSkillPower>(
            choiceContext,
            Owner.Creature,
            1m,
            Owner.Creature, this);
    }

    private async Task ExecuteAttack(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        var ownerCreature =
            Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();
            tidus.PlayAnimation(ownerCreature, "attack_2");
            await tidus.DashTo(ownerCreature, target, distance: 300f,  durationSeconds: 0.15f, overrideAnim: null);
            SfxCmd.Play("res://Tidus/sfx/swing_1.wav");
            await tidus.Delay(ownerCreature, 0.05f);
            tidus.PlayVfxOnTarget(target, "res://Tidus/scenes/vfx.tscn", "hit");
            await CommonActions.CardAttack(this, target)
                .WithHitFx(null, "res://Tidus/sfx/Tidus/slash_2.wav")
                .Execute(choiceContext);
            await tidus.Delay(ownerCreature, 0.15f);
            await tidus.Retreat(ownerCreature, duration: 0.2f);
            return;
        }

        await CommonActions.CardAttack(this, target)
            .WithHitFx(
                null,
                "res://Tidus/sfx/Tidus/slash_2.wav")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}