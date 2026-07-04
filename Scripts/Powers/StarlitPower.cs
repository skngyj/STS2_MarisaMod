using Godot;
using marisamod.Scenes.Vfx.StarLit;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace marisamod.Scripts.Powers;

public class StarlitPower : AbstractMarisaPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // protected override IEnumerable<DynamicVar> CanonicalVars =>
    // [
    //     new DynamicVar("DamageEot", 1),
    //     new DynamicVar("BlockEot", 1)
    // ];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side)
        {
            Flash();
            var removeAfterEvoke = !Owner.HasPower<SprinkleStarNHeartPower>();
            Vfx?.StartExplosion(removeAfterEvoke);
            //CalculateVars();
            await CreatureCmd.GainBlock(Owner,
                Amount
                //DynamicVars["BlockEot"].IntValue
                , ValueProp.Unpowered, null);
            await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies,
                Amount
                //DynamicVars["DamageEot"].IntValue
                , ValueProp.Unpowered, Owner);
            if (removeAfterEvoke)
                await PowerCmd.Remove(this);
        }
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        //CalculateVars();
        UpdateVfx();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        //CalculateVars();
        UpdateVfx();
        return Task.CompletedTask;
    }

    // public override Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    // {
    //     CalculateVars();
    //     return Task.CompletedTask;
    // }

    // private void CalculateVars()
    // {
    //     DynamicVars["DamageEot"].BaseValue = Amount * (1 + Owner.GetPowerAmount<OrrerysUniversePower>() + Owner.GetPowerAmount<OrrerysGalaxyPower>());
    //     DynamicVars["BlockEot"].BaseValue = Amount * (1 + Owner.GetPowerAmount<OrrerysGalaxyBlockPower>() + Owner.GetPowerAmount<OrrerysGalaxyPower>());
    // }

    public StarlitVfx? Vfx;

    public void UpdateVfx()
    {
        Vfx ??= StarlitVfx.Create(this);
        if (!GodotObject.IsInstanceValid(Vfx)) return;
        Vfx.ApplySize(Amount);
    }
}