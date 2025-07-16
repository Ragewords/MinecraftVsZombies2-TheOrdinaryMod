using System.Collections.Generic;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.confusingPlanet)]
    public class ConfusingPlanet : EffectBehaviour
    {
        #region 公有方法
        public ConfusingPlanet(string nsp, string name) : base(nsp, name)
        {
            absorbDetector = new SphereDetector(120)
            {
                mask = EntityCollisionHelper.MASK_PLANT | EntityCollisionHelper.MASK_ENEMY,
                canDetectInvisible = true,
                factionTarget = FactionTarget.Any
            };
        }
        #endregion
        public override void Update(Entity entity)
        {
            base.Update(entity);
            bool inactive = entity.Timeout <= 90;
            entity.SetAnimationBool("Disappear", inactive);
            if (inactive)
                return;
                
            detectBuffer.Clear();
            absorbDetector.DetectMultiple(entity, detectBuffer);
            foreach (var target in detectBuffer)
            {
                target.Entity.Slow(90);
            }
        }
        private List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
        private Detector absorbDetector;
    }
}