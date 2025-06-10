using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.shikaisenPot)]
    public class ShikaisenPot : StateEnemy
    {
        public ShikaisenPot(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 20f);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationInt("HealthState", entity.GetHealthState(3));
            var pot = entity;
            if (pot.Parent == null)
                return;
            if (pot.Parent.HasBuff<ShikaisenReviveBuff>())
                return;
            if (pot.Parent.IsDead)
            {
                pot.Parent.Remove();
                var revive_id = pot.Parent.GetDefinitionID();
                var shikaisen = pot.SpawnWithParams(revive_id, pot.Position);
                ShikaisenZombie.SetPot(shikaisen, false);
                pot.Die(pot);
            }
        }
        public override void PreTakeDamage(DamageInput input, CallbackResult result)
        {
            base.PreTakeDamage(input, result);
            if (!input.HasEffect(VanillaDamageEffects.EXPLOSION) && !input.HasEffect(VanillaDamageEffects.PUNCH))
            {
                input.SetAmount(1);
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            entity.Remove();
        }
    }
}
