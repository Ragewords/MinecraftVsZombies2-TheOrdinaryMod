using MVZ2.Vanilla.Modifiers;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.Boss.seijaLantern)]
    public class SeijaLanternBuff : BuffDefinition
    {
        public SeijaLanternBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new IntModifier(EngineEntityProps.COLLISION_DETECTION, NumberOperator.Set, EntityCollisionHelper.DETECTION_IGNORE, VanillaModifierPriorities.FORCE));
            AddModifier(ColorModifier.Multiply(EngineEntityProps.TINT, PROP_TINT_MULTIPLIER));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TINT_MULTIPLIER, new Color(1, 1, 0, 0.5f));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timeout = buff.GetProperty<int>(PROP_TIMEOUT);
            timeout--;
            buff.SetProperty(PROP_TIMEOUT, timeout);
            if (timeout <= 0)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<int> PROP_TIMEOUT = new VanillaBuffPropertyMeta<int>("Timeout");
        public static readonly VanillaBuffPropertyMeta<Color> PROP_TINT_MULTIPLIER = new VanillaBuffPropertyMeta<Color>("TintMultiplier");
    }
}
