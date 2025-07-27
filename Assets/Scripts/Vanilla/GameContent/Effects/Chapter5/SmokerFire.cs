using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.smokerFire)]
    public class SmokerFire : Fireblock
    {

        #region 公有方法
        public SmokerFire(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion
        public override void Init(Entity block)
        {
            base.Init(block);
            block.CollisionMaskHostile =
                EntityCollisionHelper.MASK_VULNERABLE;
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (!collision.Collider.IsMainCollider())
                return;
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            var self = collision.Entity;
            collision.OtherCollider.TakeDamage(self.GetDamage(), new DamageEffectList(VanillaDamageEffects.FIRE, VanillaDamageEffects.MUTE), self);
        }
    }
}