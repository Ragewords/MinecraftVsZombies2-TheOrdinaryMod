using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.Vanilla.Detections;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class SoulsandDetector : Detector
    {
        public SoulsandDetector()
        {
            mask = EntityCollisionHelper.MASK_ENEMY;
            factionTarget = FactionTarget.Any;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            return self.GetBounds();
        }
        protected override bool ValidateCollider(DetectionParams param, IEntityCollider collider)
        {
            var target = collider.Entity;
            if (target == null)
                return false;
            if (target.IsDead)
                return false;
            if (!target.IsFactionTarget(param.faction, factionTarget))
                return false;
            if (target.HasBuff<SoulsandBuff>())
                return false;
            return true;
        }
    }
}
