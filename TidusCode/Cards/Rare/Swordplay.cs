using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Tidus.TidusCode.Extensions;
using Tidus.TidusCode.Mechanics;
using Tidus.TidusCode.Powers;

namespace Tidus.TidusCode.Cards.Rare;

public class Swordplay() : TidusCard(
    1,
    CardType.Attack,
    CardRarity.Rare,
    TargetType.AnyEnemy),
    IBlitz
{
    protected override bool ShouldGlowGoldInternal => Owner.HasPower<HastePower>();
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12m, ValueProp.Move),
        new DynamicVar("Overdrive", 15m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        TidusEnums.Blitz
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<OverdriveReadyPower>()
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
        CenterCardCinematic.Start(
            RunManager.Instance.NetService.NetId);

        await ExecuteAttack(
            choiceContext,
            play.Target,
            play);

        OverdriveManager.GainOverdrive(
            Owner,
            DynamicVars["Overdrive"].IntValue);

        if (Owner.HasPower<HastePower>())
        {
            OverdriveManager.GainOverdrive(
                Owner,
                DynamicVars["Overdrive"].IntValue);
        }

        CenterCardCinematic.End(
            RunManager.Instance.NetService.NetId);
    }

    private async Task ExecuteAttack(
        PlayerChoiceContext choiceContext,
        Creature target,
        CardPlay play)
    {
        var ownerCreature = Owner?.Creature;

        if (ownerCreature != null &&
            Owner?.Character is Character.Tidus tidus)
        {
            AudioHelper.PlayRandomAttackHard();

            await tidus.DashTo(
                ownerCreature,
                target,
                distance: 300f,
                durationSeconds: 0.20f);

            tidus.PlayAnimation(ownerCreature, "dash_attack");
            await tidus.Delay(ownerCreature, 0.05f);
            SfxCmd.Play("res://Tidus/sfx/swing_2.wav");
            SfxCmd.Play("res://Tidus/sfx/Tidus/slash_2.wav");
            tidus.DashTo(ownerCreature, target, distance: -200f, durationSeconds: 0.05F, overrideAnim: "dash_attack");

            tidus.PlayVfxOnTarget(
                target,
                "res://Tidus/scenes/vfx.tscn",
                "hit");

            await CommonActions.CardAttack(this, target)
                .WithHitFx(null)
                .Execute(choiceContext);

            await tidus.Delay(ownerCreature, 0.15f);

            await tidus.Retreat(
                ownerCreature,
                duration: 0.2f);

            return;
        }

        await CommonActions.CardAttack(this, target)
            .WithHitFx(
                null,
                "res://Tidus/sfx/Tidus/slash_1.wav")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["Overdrive"].UpgradeValueBy(8m);
    }
}