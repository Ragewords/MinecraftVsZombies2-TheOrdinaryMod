using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.vomitSplash)]
    public class VomitSplash : EffectBehaviour
    {

        #region 公有方法
        public VomitSplash(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskHostile = EntityCollisionHelper.MASK_PLANT | EntityCollisionHelper.MASK_ENEMY | EntityCollisionHelper.MASK_OBSTACLE | EntityCollisionHelper.MASK_BOSS;
        }
        #endregion
        public override void Update(Entity entity)
        {
            base.Update(entity);
            entity.SetTint(new Color(1, 1, 1, Mathf.Clamp01(entity.Timeout / 15f)));
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            var other = collision.Other;
            var self = collision.Entity;
            other.TakeDamage(self.GetDamage(), new DamageEffectList(VanillaDamageEffects.MUTE), self);
        }
    }
}