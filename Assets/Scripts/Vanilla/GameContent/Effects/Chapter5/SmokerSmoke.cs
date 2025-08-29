using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
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
        public override void Update(Entity entity)
        {
            base.Update(entity);
            var inactive = entity.Timeout <= 15;
            if (inactive)
                return;

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