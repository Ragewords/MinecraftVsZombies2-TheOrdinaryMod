using System.Collections.Generic;
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
            zapDetector = new SphereDetector(ZAP_RADIUS)
            {
                factionTarget = FactionTarget.Friendly
            };
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMEOUT, 10);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();
            var zap_time = buff.GetProperty<int>(PROP_ZAP_TIME);

            var timeout = buff.GetProperty<int>(PROP_TIMEOUT);
            timeout--;
            buff.SetProperty(PROP_TIMEOUT, timeout);
            if (timeout <= 0)
            {
                buff.Remove();
            }

            if (zap_time <= 0)
            {
                return;
            }

            detectBuffer.Clear();
            var target = zapDetector.DetectEntityWithTheLeast(entity, e => Mathf.Abs(e.Position.magnitude - entity.Position.magnitude));
            if (target == null)
                return;
            if (target.HasBuff<ElectricChainBuff>())
                return;

            entity.PlaySound(VanillaSoundID.redLightning);
            target.TakeDamage(10 * zap_time, new DamageEffectList(VanillaDamageEffects.LIGHTNING), entity);

            var arc = entity.Spawn(VanillaEffectID.electricArc, entity.Position);
            ElectricArc.Connect(arc, target.Position);
            ElectricArc.SetPointCount(arc, 20);
            ElectricArc.UpdateArc(arc);
            arc.Timeout = 15;

            var targetBuff = target.GetFirstBuff<ElectricChainBuff>();
            if (targetBuff == null)
            {
                targetBuff = target.AddBuff<ElectricChainBuff>();
            }
            targetBuff.SetProperty(PROP_ZAP_TIME, zap_time - 1);
        }
        public const float ZAP_RADIUS = 120;
        public static readonly VanillaBuffPropertyMeta<int> PROP_ZAP_TIME = new VanillaBuffPropertyMeta<int>("zaptime");
        public static readonly VanillaBuffPropertyMeta<int> PROP_TIMEOUT = new VanillaBuffPropertyMeta<int>("Timeout");
        private List<Entity> detectBuffer = new List<Entity>();
        private Detector zapDetector;
    }
}
