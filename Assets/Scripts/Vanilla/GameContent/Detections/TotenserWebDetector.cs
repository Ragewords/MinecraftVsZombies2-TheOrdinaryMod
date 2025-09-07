using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class TotenserWebDetector : Detector
    {
        public TotenserWebDetector(float range = 0)
        {
            mask = EntityCollisionHelper.MASK_ENEMY;
            this.range = range;
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var sizeX = range;
            var sizeY = 64;
            var sizeZ = 48;
            var source = self.Position;
            var centerX = source.x + sizeX * 0.5f * self.GetFacingX();
            var centerY = source.y + sizeY * 0.5f;
            var centerZ = source.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams self, IEntityCollider collider)
        {
            if (!base.ValidateCollider(self, collider))
                return false;
            var target = collider.Entity;
            if (!TargetInLawn(target))
                return false;
            if (target.HasBuff<TotenserWebBuff>())
                return false;
            return true;
        }
        private float range;
    }
}
