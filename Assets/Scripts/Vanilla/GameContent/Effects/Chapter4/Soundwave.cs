using System.Collections.Generic;
using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.soundwave)]
    public class Soundwave : EffectBehaviour
    {

        #region 公有方法
        public Soundwave(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskHostile |= EntityCollisionHelper.MASK_VULNERABLE;
        }
        public override void Update(Entity entity)
        {
            entity.SetAnimationBool("Loud", IsLoud(entity));
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (state != EntityCollisionHelper.STATE_ENTER)
                return;
            var wave = collision.Entity;
            var other = collision.Other;
            other.TakeDamage(wave.GetDamage(), new DamageEffectList(VanillaDamageEffects.MUTE), wave);
        }
        public static void SetLoud(Entity entity, bool value) => entity.SetProperty(PROP_LOUD, value);
        public static bool IsLoud(Entity entity) => entity.GetProperty<bool>(PROP_LOUD);
        public static readonly VanillaBuffPropertyMeta<bool> PROP_LOUD = new VanillaBuffPropertyMeta<bool>("loud");
    }
}