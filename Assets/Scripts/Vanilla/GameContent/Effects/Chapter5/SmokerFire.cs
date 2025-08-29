using System.Collections.Generic;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.smokerFire)]
    public class SmokerFire : Fireblock
    {

        #region 公有方法
        public SmokerFire(string nsp, string name) : base(nsp, name)
        {
            igniteDetector = new HellfireIgniteDetector(0)
            {
                factionTarget = FactionTarget.Friendly,
                mask = EntityCollisionHelper.MASK_PROJECTILE,
            };
        }
        #endregion
        public override void Update(Entity entity)
        {
            base.Update(entity);
            UpdateIgnite(entity);
            collideBuffer.Clear();
            collideDetector.DetectMultiple(entity, collideBuffer);
            foreach (var collider in collideBuffer)
            {
                var other = collider.Entity;
                if (entity.IsHostile(other))
                {
                    collider.TakeDamage(entity.GetDamage(), new DamageEffectList(VanillaDamageEffects.FIRE, VanillaDamageEffects.MUTE), entity);
                }
            }
        }
        public override void PostRemove(Entity entity)
        {
            base.PostRemove(entity);
            entity.PlaySound(VanillaSoundID.fizz);
            var param = entity.GetSpawnParams();
            param.SetProperty(EngineEntityProps.SIZE, entity.GetScaledSize());
            param.SetProperty(EngineEntityProps.TINT, Color.black);
            entity.Spawn(VanillaEffectID.smoke, entity.GetCenter(), param);
        }
        private void UpdateIgnite(Entity fire)
        {
            igniteBuffer.Clear();
            igniteDetector.DetectEntities(fire, igniteBuffer);
            foreach (Entity target in igniteBuffer)
            {
                var behaviour = target.Definition?.GetBehaviour<IHellfireIgniteBehaviour>();
                if (behaviour == null)
                    return;
                behaviour.Ignite(target, fire, false);
            }
        }
        private Detector collideDetector = new CollisionDetector();
        private List<IEntityCollider> collideBuffer = new List<IEntityCollider>();
        private Detector igniteDetector;
        private List<Entity> igniteBuffer = new List<Entity>();
    }
}