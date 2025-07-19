using System.Collections.Generic;
using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.blackhole)]
    public class Blackhole : EffectBehaviour
    {

        #region 公有方法
        public Blackhole(string nsp, string name) : base(nsp, name)
        {
            absorbDetector = new BlackholeDetector()
            {
                mask = EntityCollisionHelper.MASK_VULNERABLE | EntityCollisionHelper.MASK_PROJECTILE,
                canDetectInvisible = true,
                factionTarget = FactionTarget.Any
            };
        }
        #endregion
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.Level.AddLoopSoundEntity(VanillaSoundID.gravitationSurge, entity.ID);
        }

        public override void Update(Entity entity)
        {
            base.Update(entity);

            entity.SetDisplayScale(Vector3.one * (entity.GetRange() / 80));

            bool active = entity.Timeout > 5;
            bool disappear = entity.Timeout == 5;
            entity.SetAnimationBool("Started", active);
            detectBuffer.Clear();
            absorbDetector.DetectMultiple(entity, detectBuffer);
            
            if (disappear)
                Teleport(entity);
            if (!active)
                return;

            // Absorb.
            foreach (var collider in detectBuffer)
            {
                var target = collider.Entity;
                bool hostile = target.IsHostile(entity);
                if (collider.IsMainCollider())
                {
                    if (target.Type == EntityTypes.BOSS)
                    {
                        if (hostile)
                        {
                            if (target.IsEntityOf(VanillaBossID.theGiantSnakeTail) || (target.IsEntityOf(VanillaBossID.theGiant) && TheGiant.CanAttractByBlackhole(target)))
                            {
                                snakeBuffer.Clear();
                                TheGiantSnakeTail.GetFullSnake(target, snakeBuffer);
                                foreach (var segment in snakeBuffer)
                                {
                                    segment.Velocity = Vector3.zero;
                                    var newCenter = segment.GetCenter() * 0.9f + entity.GetCenter() * 0.1f;
                                    segment.SetCenter(newCenter);
                                }
                            }
                        }
                    }
                    else if (target.Type == EntityTypes.ENEMY)
                    {
                        if (hostile)
                        {
                            target.Velocity = Vector3.zero;
                            var newCenter = target.GetCenter() * 0.7f + entity.GetCenter() * 0.3f;
                            target.SetCenter(newCenter);
                            target.StopChangingLane();
                        }
                    }
                    else if (target.Type == EntityTypes.PROJECTILE)
                    {
                        var newCenter = target.GetCenter() * 0.7f + entity.GetCenter() * 0.3f;
                        target.SetCenter(newCenter);
                    }
                }
                if (hostile && target.IsVulnerableEntity())
                {
                    collider.TakeDamage(entity.GetDamage(), new DamageEffectList(VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN), entity);
                }
            }
        }
        private void Teleport(Entity entity)
        {
            bool whitehole_left = false;
            bool whitehole_right = false;
            foreach (var collider in detectBuffer)
            {
                var target = collider.Entity;
                bool hostile = target.IsHostile(entity);
                if (target.Type == EntityTypes.ENEMY)
                {
                    if (hostile)
                    {
                        var pos = target.Position;
                        pos.x = VanillaLevelExt.RIGHT_BORDER;
                        pos.y = entity.Position.y;
                        pos.z = entity.Position.z;
                        target.Position = pos;
                        whitehole_right = true;
                    }
                }
                else if (target.Type == EntityTypes.PROJECTILE)
                {
                    var pos = target.Position;
                    pos.x = VanillaLevelExt.LEFT_BORDER;
                    pos.y = entity.Position.y;
                    pos.z = entity.Position.z;
                    target.Position = pos;
                    whitehole_left = true;
                }
            }
            if (whitehole_left)
                entity.Spawn(VanillaEffectID.whitehole, new Vector3(VanillaLevelExt.LEFT_BORDER, entity.Position.y, entity.Position.z));
            if (whitehole_right)
                entity.Spawn(VanillaEffectID.whitehole, new Vector3(VanillaLevelExt.RIGHT_BORDER, entity.Position.y, entity.Position.z));
        }
        private List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
        private List<Entity> snakeBuffer = new List<Entity>();
        private Detector absorbDetector;
    }
}