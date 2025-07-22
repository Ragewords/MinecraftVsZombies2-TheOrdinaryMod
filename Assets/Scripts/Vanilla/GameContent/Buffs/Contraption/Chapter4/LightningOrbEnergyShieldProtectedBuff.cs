using MVZ2.Vanilla.Modifiers;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.lightningOrbEnergyShieldProtected)]
    public class LightningOrbEnergyShieldProtectedBuff : BuffDefinition
    {
        public LightningOrbEnergyShieldProtectedBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new IntModifier(EngineEntityProps.COLLISION_DETECTION, NumberOperator.Set, EntityCollisionHelper.DETECTION_IGNORE, VanillaModifierPriorities.FORCE));
        }
    }
}
