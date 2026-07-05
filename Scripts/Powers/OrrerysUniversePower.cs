using marisamod.Scripts.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace marisamod.Scripts.Powers;

public class OrrerysUniversePower : AbstractMarisaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
    {
        Flash();
        if (power is StarlitPower && target == Owner && amount > 0)
        {
            foreach (var card in Owner.Player!.PlayerCombatState!.Hand.Cards.Where(x => x.Type == CardType.Attack).ToArray())
            {
                PowerUp.UpgradeCardDamage(card, Amount);
            }
        }

        return base.ModifyPowerAmountGivenMultiplicative(power, giver, amount, target, cardSource);
    }
}