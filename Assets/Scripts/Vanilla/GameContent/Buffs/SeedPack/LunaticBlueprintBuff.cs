using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.SeedPacks
{
    [BuffDefinition(VanillaBuffNames.SeedPack.lunaticBlueprint)]
    public class LunaticBlueprintBuff : BuffDefinition
    {
        public LunaticBlueprintBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(EngineSeedProps.RECHARGE_SPEED, NumberOperator.Multiply, 0.8f));
        }
    }
}
