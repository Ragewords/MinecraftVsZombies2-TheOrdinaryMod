using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.littleContraption)]
    public class LittleContraptionBuff : BuffDefinition
    {
        public LittleContraptionBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new Vector3Modifier(EngineEntityProps.SCALE, NumberOperator.Multiply, new Vector3(0.5f, 0.5f, 0.5f)));
            AddModifier(new Vector3Modifier(EngineEntityProps.DISPLAY_SCALE, NumberOperator.Multiply, new Vector3(0.5f, 0.5f, 0.5f)));
            AddModifier(new Vector3Modifier(VanillaEntityProps.SHADOW_SCALE, NumberOperator.Multiply, new Vector3(0.5f, 0.5f, 0.5f)));
            AddModifier(new FloatModifier(EngineEntityProps.MAX_HEALTH, NumberOperator.Multiply, 0.25f));
            AddModifier(new FloatModifier(VanillaEntityProps.DAMAGE, NumberOperator.Multiply, 0.25f));
        }
    }
}
