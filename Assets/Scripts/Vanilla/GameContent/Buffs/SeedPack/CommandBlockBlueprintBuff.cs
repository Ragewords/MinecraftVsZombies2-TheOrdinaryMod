using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.SeedPacks
{
    [BuffDefinition(VanillaBuffNames.SeedPack.commandBlockBlueprint)]
    public class CommandBlockBlueprintBuff : BuffDefinition
    {
        public CommandBlockBlueprintBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(EngineSeedProps.COST, NumberOperator.Set, 100));
            SetProperty(EngineRechargeProps.START_MAX_RECHARGE, 1050);
            SetProperty(EngineRechargeProps.MAX_RECHARGE, 1500);
        }
    }
}
