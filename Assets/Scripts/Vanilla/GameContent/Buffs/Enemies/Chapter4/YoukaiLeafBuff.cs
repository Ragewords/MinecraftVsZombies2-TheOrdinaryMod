using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.youkaiLeaf)]
    public class YoukaiLeafBuff : BuffDefinition
    {
        public YoukaiLeafBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new MaxHealthModifier(NumberOperator.Set, 300));
            AddModifier(new BooleanModifier(VanillaEntityProps.GRAYSCALE, true));
        }
    }
}
