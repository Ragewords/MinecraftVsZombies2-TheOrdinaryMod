using System.Collections.Generic;
using MVZ2.GameContent.Areas;
using MVZ2.GameContent.Artifacts;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Obstacles;
using MVZ2.GameContent.Pickups;
using MVZ2.Vanilla;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2Logic;
using MVZ2Logic.Level;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.annihilationField)]
    public class AnnihilationField : EffectBehaviour
    {

        #region 公有方法
        public AnnihilationField(string nsp, string name) : base(nsp, name)
        {
            absorbDetector = new BlackholeDetector()
            {
                mask = EntityCollisionHelper.MASK_VULNERABLE | EntityCollisionHelper.MASK_PROJECTILE,
                canDetectInvisible = true,
                factionTarget = FactionTarget.Hostile
            };
        }
        #endregion
        public override void Update(Entity entity)
        {
            base.Update(entity);

            var range = entity.GetRange();
            entity.SetDisplayScale(Vector3.one * (range / 65));
            entity.Level.ShakeScreen(entity.Timeout <= 35 ? 5 : 0, 0, 15);

            bool active = entity.Timeout > 5 && entity.Timeout <= 35;
            entity.SetAnimationBool("Started", active);

            if (entity.Timeout == 35)
            {
                entity.PlaySound(VanillaSoundID.annihilationField);
            }
            if (!active)
                return;

            // Absorb.
            detectBuffer.Clear();
            absorbDetector.DetectEntities(entity, detectBuffer);
            var sqrRange = range * range;
            entity.Level.FindEntitiesNonAlloc(e => e.IsEntityOf(VanillaObstacleID.monsterSpawner) && e.IsHostile(entity) && (e.GetCenter() - entity.GetCenter()).sqrMagnitude <= sqrRange, detectBuffer);
            foreach (var target in detectBuffer)
            {
                if (target.Type == EntityTypes.ENEMY)
                {
                    target.Neutralize();
                    target.Remove();
                }
                else if (target.Type == EntityTypes.PLANT || target.Type == EntityTypes.OBSTACLE || target.Type == EntityTypes.PROJECTILE)
                {
                    target.Remove();
                }
            }
        }
        public override void PostRemove(Entity entity)
        {
            base.PostRemove(entity);
            var level = entity.Level;
            if (level.AreaID == VanillaAreaID.dream && !Global.Game.IsUnlocked(VanillaUnlockID.bottledBlackhole))
            {
                if (!level.EntityExists(e => e.IsEntityOf(VanillaPickupID.artifactPickup) && ArtifactPickup.GetArtifactID(e) == VanillaArtifactID.bottledBlackhole))
                {
                    var lantern = level.Spawn(VanillaPickupID.artifactPickup, entity.Position, entity);
                    ArtifactPickup.SetArtifactID(lantern, VanillaArtifactID.bottledBlackhole);
                }
            }
        }
        private List<Entity> detectBuffer = new List<Entity>();
        private Detector absorbDetector;
    }
}