using System.Collections.Generic;
using System.Linq;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.electricChain)]
    public class ElectricChainBuff : BuffDefinition
    {
        public ElectricChainBuff(string nsp, string name) : base(nsp, name)
        {
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMEOUT, 10);
            attackedEnemies.Clear();
            var entity = buff.GetEntity();
            attackedEnemies.Add(entity);
            FindNextTarget(entity, entity.Position, entity.GetFaction(), DMG);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            if (attackedEnemies.Count >= MAX_TARGETS)
                buff.Remove();
        }
        private void FindNextTarget(Entity entity, Vector3 origin, int faction, float nextDamage)
        {
            IEntityCollider[] hitColliders = entity.Level.OverlapSphere(origin, ZAP_RADIUS, faction, 0, EntityCollisionHelper.MASK_VULNERABLE);

            var validTargets = hitColliders
                .Where(c => !attackedEnemies.Contains(c.Entity))
                .OrderBy(c => Vector3.Distance(origin, c.Entity.Position))
                .ToArray();

            if (validTargets.Length > 0)
            {
                entity.PlaySound(VanillaSoundID.redLightning);
                AttackTarget(entity, validTargets[0].Entity, nextDamage);
            }
        }
        private void AttackTarget(Entity entity, Entity target, float currentDamage)
        {
            if (attackedEnemies.Count >= MAX_TARGETS || target == null)
                return;
            target.TakeDamage(currentDamage, new DamageEffectList(VanillaDamageEffects.LIGHTNING), entity);

            var arc = entity.Spawn(VanillaEffectID.electricArc, entity.Position);
            ElectricArc.Connect(arc, target.Position);
            ElectricArc.SetPointCount(arc, 10);
            ElectricArc.UpdateArc(arc);
            arc.Timeout = 15;
            
            attackedEnemies.Add(target);
            FindNextTarget(target, target.Position, target.GetFaction(), currentDamage * (1 - DMG_REDUCTION));
        }
        public const float ZAP_RADIUS = 120;
        public const float MAX_TARGETS = 5;
        public const float DMG = 10;
        public const float DMG_REDUCTION = 0.2f;
        public static readonly VanillaBuffPropertyMeta<int> PROP_ZAP_TIME = new VanillaBuffPropertyMeta<int>("zaptime");
        public static readonly VanillaBuffPropertyMeta<int> PROP_TIMEOUT = new VanillaBuffPropertyMeta<int>("Timeout");
        private List<Entity> attackedEnemies = new List<Entity>();
    }
}
