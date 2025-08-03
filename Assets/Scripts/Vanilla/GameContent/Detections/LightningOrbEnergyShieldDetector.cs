using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.Vanilla.Detections;
using PVZEngine.Entities;
using UnityEngine;

namespace MVZ2.GameContent.Detections
{
    public class LightningOrbEnergyShieldDetector : Detector
    {
        public LightningOrbEnergyShieldDetector()
        {
        }
        protected override Bounds GetDetectionBounds(Entity self)
        {
            var sizeX = 240;
            var sizeY = 80;
            var sizeZ = 240;
            var centerX = self.Position.x;
            var centerY = self.GetGroundY() + 40;
            var centerZ = self.Position.z;
            return new Bounds(new Vector3(centerX, centerY, centerZ), new Vector3(sizeX, sizeY, sizeZ));
        }
        protected override bool ValidateCollider(DetectionParams self, IEntityCollider collider)
        {
            if (!base.ValidateCollider(self, collider))
                return false;
            var target = collider.Entity;
            if (target.HasBuff<LightningOrbEnergyShieldBuff>())
                return false;
            return true;
        }
    }
}
