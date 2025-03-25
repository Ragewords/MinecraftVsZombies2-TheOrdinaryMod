using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.lunaticContraption)]
    public class LunaticContraptionBuff : BuffDefinition
    {
        public LunaticContraptionBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(VanillaEntityProps.DAMAGE, NumberOperator.AddMultiplie, -0.2f));
        }
    }
}
