using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.jackOLantern)]
    public class JackOLanternBuff : BuffDefinition
    {
        public JackOLanternBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new BooleanModifier(VanillaEntityProps.IS_LIGHT_SOURCE, true));
            AddModifier(new ColorModifier(VanillaEntityProps.LIGHT_COLOR, Color.yellow));
            AddModifier(new Vector3Modifier(VanillaEntityProps.LIGHT_RANGE, NumberOperator.Add, new Vector3(100, 100, 100)));
        }
    }
}


