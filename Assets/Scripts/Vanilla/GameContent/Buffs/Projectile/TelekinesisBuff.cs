using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using System.Collections.Generic;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Projectiles
{
    [BuffDefinition(VanillaBuffNames.telekinesis)]
    public class TelekinesisBuff : BuffDefinition
    {
        public TelekinesisBuff(string nsp, string name) : base(nsp, name)
        {
            absorbDetector = new SphereDetector(ABSORB_RADIUS)
            {
                mask = EntityCollisionHelper.MASK_VULNERABLE,
                invulnerableFilter = (param, e) => e.Type == EntityTypes.PROJECTILE
            };
            AddModifier(new BooleanModifier(VanillaProjectileProps.PIERCING, false));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();

            detectBuffer.Clear();
            var target = absorbDetector.DetectEntityWithTheLeast(entity, e => Mathf.Abs(e.Position.magnitude - entity.Position.magnitude));
            if (target != null)
            {
                var vel = entity.Velocity;
                var speed = Mathf.Min(vel.magnitude + ABSORB_SPEED, ABSORB_MAX_SPEED);
                vel += (target.GetCenter() - entity.GetCenter()).normalized * ABSORB_SPEED;
                vel = vel.normalized * speed;
                entity.Velocity = vel;
            }
        }
        public const float ABSORB_RADIUS = 80;
        public const float ABSORB_SPEED = 4;
        public const float ABSORB_MAX_SPEED = 10;
        private List<Entity> detectBuffer = new List<Entity>();
        private Detector absorbDetector;
    }
}
