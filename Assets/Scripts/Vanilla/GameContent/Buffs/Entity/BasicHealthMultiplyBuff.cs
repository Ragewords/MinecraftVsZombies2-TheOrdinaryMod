using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs
{
    [BuffDefinition(VanillaBuffNames.basicHealthMultiply)]
    public class BasicHealthMultiplyBuff : BuffDefinition
    {
        public BasicHealthMultiplyBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new MaxHealthModifier(NumberOperator.Multiply, PROP_MAX_HEALTH_MULTIPLIER));
        }

        public static readonly VanillaBuffPropertyMeta<float> PROP_MAX_HEALTH_MULTIPLIER = new VanillaBuffPropertyMeta<float>("MaxHealthMultiplier");
    }
}
