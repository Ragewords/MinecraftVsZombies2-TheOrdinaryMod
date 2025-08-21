using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.executioner)]
    public class Executioner : EffectBehaviour
    {
        #region 公有方法
        public Executioner(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.PlaySound(VanillaSoundID.dirtRise);
            entity.CollisionMaskHostile = EntityCollisionHelper.MASK_PLANT;
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            var entity = collision.Entity;
            var other = collision.Other;
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            if (other.IsDead)
                return;
            if (entity.GetGrid() != other.GetGrid())
                return;
            other.Die(entity);
        }
    }
}