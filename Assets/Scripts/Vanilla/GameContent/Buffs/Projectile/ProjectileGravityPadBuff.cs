using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Projectiles
{
    [BuffDefinition(VanillaBuffNames.projectileGravityPad)]
    public class ProjectileGravityPadBuff : BuffDefinition
    {
        public ProjectileGravityPadBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(EngineEntityProps.GRAVITY, NumberOperator.Add, PULL_DOWN_SPEED));
            AddModifier(new BooleanModifier(VanillaProjectileProps.KILL_ON_GROUND, false));
            AddModifier(new BooleanModifier(VanillaProjectileProps.ROLLS, true));
        }
        public const float PULL_DOWN_SPEED = 0.133f;
    }
}
