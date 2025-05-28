using MVZ2.GameContent.Buffs;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Auras;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using System.Collections.Generic;
using UnityEngine;
using static MVZ2.GameContent.Contraptions.GravityPad;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.soulsand)]
    public class Soulsand : EnemyBehaviour
    {
        public Soulsand(string nsp, string name) : base(nsp, name)
        {
            AddAura(new SoulsandAura());
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.Timeout = entity.GetMaxTimeout();
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);

            if (entity.Velocity.y != 0)
            {
                entity.AddFragmentTickDamage(Mathf.Abs(entity.Velocity.y));
            }
            entity.SetAnimationInt("HealthState", entity.GetHealthState(3));
            if (entity.Timeout >= 0)
            {
                entity.Timeout--;
                if (entity.Timeout <= 0)
                {
                    entity.Die(entity);
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            entity.Remove();
        }
        public class SoulsandAura : AuraEffectDefinition
        {
            public SoulsandAura()
            {
                BuffID = VanillaBuffID.soulsand;
                UpdateInterval = 5;
                enemyDetector = new SoulsandDetector();
            }

            public override void GetAuraTargets(AuraEffect auraEffect, List<IBuffTarget> results)
            {
                var source = auraEffect.Source;
                var entity = source.GetEntity();
                if (entity == null)
                    return;
                detectBuffer.Clear();
                enemyDetector.DetectEntities(entity, detectBuffer);
                results.AddRange(detectBuffer);
            }
            private Detector enemyDetector;
            private List<Entity> detectBuffer = new List<Entity>();
        }
    }
}
