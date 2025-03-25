using MVZ2.GameContent.Enemies;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.seijaJizo)]
    public class SeijaJizo : EnemyBehaviour
    {
        public SeijaJizo(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new IntModifier(EngineEntityProps.COLLISION_DETECTION, NumberOperator.ForceSet, EntityCollisionHelper.DETECTION_IGNORE));
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.InitFragment();
            entity.Timeout = entity.GetMaxTimeout();
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.UpdateFragment();
            if (entity.Timeout >= 0)
            {
                entity.Timeout--;
                if (entity.Timeout <= 0)
                {
                    entity.Die(entity);
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo damageInfo)
        {
            base.PostDeath(entity, damageInfo);
            entity.PostFragmentDeath(damageInfo);
            entity.Remove();
        }
    }
}
