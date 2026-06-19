using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace marisamod.Scripts.Cards
{
    public class NonDirectionalLaser : AbstractMarisaCard
    {
        public NonDirectionalLaser() : base(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
        {
        }

        protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var damage = DynamicVars.Damage.BaseValue;
            var repeat = _hasGeneratedCard ? 2 : 1;
            await DamageCmd.Attack(damage).FromCard(this).TargetingAllOpponents(CombatState!).WithHitCount(repeat)
                .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
                .Execute(choiceContext);
            // await DamageCmd.Attack(damage).FromCard(this)
            //     .TargetingRandomOpponents(CombatState!)
            //     .WithHitFx("vfx/vfx_attack_slash")
            //     .Execute(choiceContext);
        }

        private bool _hasGeneratedCard;

        public override Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
        {
            if (creator != null && creator == Owner)
                _hasGeneratedCard = true;
            return base.AfterCardGeneratedForCombat(card, creator);
        }

        public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            _hasGeneratedCard = false;
            return base.BeforeSideTurnStart(choiceContext, side, participants, combatState);
        }
    }
}