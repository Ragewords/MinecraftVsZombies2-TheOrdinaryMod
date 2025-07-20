using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Modifiers;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.skelebombPunched)]
    public class SkelebombPunchedBuff : BuffDefinition
    {
        public SkelebombPunchedBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(VanillaEntityProps.DAMAGE, NumberOperator.Multiply, 2));
            AddModifier(new IntModifier(EngineEntityProps.FACTION, NumberOperator.Set, VanillaFactions.NEUTRAL));
            AddModifier(new IntModifier(EngineEntityProps.COLLISION_DETECTION, NumberOperator.Set, EntityCollisionHelper.DETECTION_IGNORE, VanillaModifierPriorities.FORCE));
        }
    }
}
