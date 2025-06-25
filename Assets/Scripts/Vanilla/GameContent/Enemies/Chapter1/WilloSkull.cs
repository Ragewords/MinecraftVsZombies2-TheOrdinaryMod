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
    [EntityBehaviourDefinition(VanillaEnemyNames.willoSkull)]
    public class WilloSkull : StateEnemy
    {
        public WilloSkull(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 20f);
            entity.CollisionMaskHostile |= EntityCollisionHelper.MASK_VULNERABLE;
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            var other = collision.Other;
            var self = collision.Entity;
            if (self.IsFriendly(other))
                return;
            other.TakeDamage(0.133f, new DamageEffectList(VanillaDamageEffects.FIRE, VanillaDamageEffects.MUTE), self);
        }
    }
}
