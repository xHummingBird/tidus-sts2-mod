using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using Tidus.TidusCode.Cards.Ancient;
using Tidus.TidusCode.Relics;

namespace Tidus.TidusCode.Powers;

public class OverdriveReadyPower : TidusPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;
    
    private IEnumerable<CardModel> GetOverdrive()
    {
        var hand =
            PileType.Hand.GetPile(Owner.Player);

        return hand.Cards
            .OfType<Overdrive>();
    }
    
    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        var existingCard = Owner.Player.PlayerCombatState?
            .AllCards
            .OfType<Overdrive>()
            .FirstOrDefault();

        if (existingCard == null)
        {
            var card =
                CombatState.CreateCard<Overdrive>(
                    Owner.Player);

            if (Owner.Player.GetRelic<Caladbolg>() != null)
            {
                CardCmd.Upgrade(card);
            }

            await CardPileCmd.AddGeneratedCardToCombat(
                card,
                PileType.Hand,
                Owner.Player);
        }
        else if (existingCard.Pile?.Type != PileType.Hand)
        {
            await CardPileCmd.Add(
                existingCard,
                PileType.Hand);
        }
        
        var ownerCreature = Owner;
        var tidus = Owner?.Player.Character as Character.Tidus;
        if (ownerCreature != null && tidus != null)
        {
            SfxCmd.Play("res://Tidus/sfx/Tidus/limit_thunder_2.wav");
            SfxCmd.Play("res://Tidus/sounds/ready_go.wav");
            tidus.PlayAnimation(ownerCreature, "overdrive_cast", true);
            await Task.Delay((int)(0.6f * 1000f));
        }
    }
    
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != base.Owner.Side)
            return;

        var player = Owner.Player;
        var playerState = player.PlayerCombatState;

        if (playerState == null)
            return;
        
        if (playerState.AllCards.OfType<Overdrive>().All(c => c.Pile?.Type != PileType.Hand))
        {
            var cards = playerState.AllCards
                .OfType<Overdrive>()
                .Where(c =>
                    c.Pile == null ||
                    c.Pile.Type != PileType.Hand);

            await CardPileCmd.Add(
                cards,
                PileType.Hand);
        }
    }
}