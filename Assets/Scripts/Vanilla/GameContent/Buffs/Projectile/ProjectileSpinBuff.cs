using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Projectiles
{
    [BuffDefinition(VanillaBuffNames.projectileSpin)]
    public class ProjectileSpinBuff : BuffDefinition
    {
        public ProjectileSpinBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new BooleanModifier(VanillaProjectileProps.POINT_TO_DIRECTION, false));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var dir = buff.GetProperty<int>(PROP_DIRECTION);
            var angle = buff.GetProperty<Vector3>(PROP_ANGLE).magnitude;
            var rotation = buff.GetEntity().RenderRotation.magnitude;
            buff.GetEntity().RenderRotation += Vector3.forward * (angle - rotation) * 0.2f;
        }
        public static readonly VanillaBuffPropertyMeta<Vector3> PROP_ANGLE = new VanillaBuffPropertyMeta<Vector3>("Angle");
        public static readonly VanillaBuffPropertyMeta<int> PROP_DIRECTION = new VanillaBuffPropertyMeta<int>("Direction");
    }
}
