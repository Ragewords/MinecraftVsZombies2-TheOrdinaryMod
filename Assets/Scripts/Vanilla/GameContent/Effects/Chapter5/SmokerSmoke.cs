using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.smokerSmoke)]
    public class SmokerSmoke : EffectBehaviour
    {
        public SmokerSmoke(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskHostile = EntityCollisionHelper.MASK_VULNERABLE;
            entity.Level.AddLoopSoundEntity(VanillaSoundID.fireBreath, entity.ID);
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (!collision.Collider.IsMainCollider())
                return;
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            var self = collision.Entity;
            var inactive = self.Timeout <= 15;
            if (inactive)
                return;
            collision.OtherCollider.TakeDamage(self.GetDamage(), new DamageEffectList(VanillaDamageEffects.MUTE), self);
        }
    }
}