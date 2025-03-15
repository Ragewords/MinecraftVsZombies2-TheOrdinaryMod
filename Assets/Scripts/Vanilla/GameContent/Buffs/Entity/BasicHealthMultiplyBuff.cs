using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs
{
    [BuffDefinition(VanillaBuffNames.basicHealthMultiply)]
    public class BasicHealthMultiplyBuff : BuffDefinition
    {
        public BasicHealthMultiplyBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new Vector3Modifier(EngineEntityProps.MAX_HEALTH, NumberOperator.Multiply, PROP_MAX_HEALTH_MULTIPLIER));
        }

        public static readonly VanillaBuffPropertyMeta PROP_MAX_HEALTH_MULTIPLIER = new VanillaBuffPropertyMeta("MaxHealthMultiplier");
    }
}
