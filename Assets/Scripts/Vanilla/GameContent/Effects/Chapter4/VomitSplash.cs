using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Detections;
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
        }
        #endregion
        public override void Update(Entity entity)
        {
            base.Update(entity);
            entity.SetTint(new Color(1, 1, 1, Mathf.Clamp01(entity.Timeout / 15f)));

            collideBuffer.Clear();
            collideDetector.DetectMultiple(entity, collideBuffer);
            foreach (var collider in collideBuffer)
            {
                var other = collider.Entity;
                if (entity.IsHostile(other))
                {
                    collider.TakeDamage(entity.GetDamage(), new DamageEffectList(VanillaDamageEffects.MUTE), entity);
                }
            }
        }
        private Detector collideDetector = new CollisionDetector();
        private List<IEntityCollider> collideBuffer = new List<IEntityCollider>();
    }
}