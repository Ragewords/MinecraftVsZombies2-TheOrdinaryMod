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
            return self.GetBounds();
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
