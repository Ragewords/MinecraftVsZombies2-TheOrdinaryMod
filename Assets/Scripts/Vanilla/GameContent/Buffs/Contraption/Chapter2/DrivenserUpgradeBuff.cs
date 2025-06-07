using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.drivenserUpgrade)]
    public class DrivenserUpgradeBuff : BuffDefinition
    {
        public DrivenserUpgradeBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(VanillaEntityProps.DAMAGE, NumberOperator.Add, 2));
            AddModifier(new Vector3Modifier(VanillaEntityProps.SHOT_VELOCITY, NumberOperator.Multiply, Vector3.one * 1.1f));
            AddModifier(new FloatModifier(VanillaEntityProps.ATTACK_SPEED, NumberOperator.Add, 1));
        }
    }
}
