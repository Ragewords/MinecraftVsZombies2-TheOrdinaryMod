using MVZ2.Vanilla.Detections;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class SoulsandDetector : Detector
    {
        public SoulsandDetector()
        {
            mask = EntityCollisionHelper.MASK_ENEMY;
            factionTarget = FactionTarget.Hostile | FactionTarget.Friendly;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var Size = self.GetScaledSize();
            var sizeX = Size.x;
            var sizeY = Size.y;
            var sizeZ = Size.z;
            var source = self.Position;
            var centerX = source.x;
            var centerY = source.y + sizeY * 0.5f;
            var centerZ = source.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams param, EntityCollider collider)
        {
            var target = collider.Entity;
            if (target == null)
                return false;
            if (target.IsDead)
                return false;
            if (!target.IsFactionTarget(param.faction, factionTarget))
                return false;
            return true;
        }
    }
}
