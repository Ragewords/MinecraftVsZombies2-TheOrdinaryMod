using System;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Projectiles
{
    [BuffDefinition(VanillaBuffNames.infinityDoor)]
    public class InfinityDoorBuff : BuffDefinition
    {
        public InfinityDoorBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new BooleanModifier(VanillaProjectileProps.NO_DESTROY_OUTSIDE_LAWN, true));
        }
        public override void PostUpdate(Buff buff)
        {
            var ent = buff.GetEntity();
            if (IsOutsideView(ent))
            {
                var bounds = ent.GetBounds();
                var position = ent.Position;
                var vel = ent.Velocity.normalized;
                var x_out = bounds.max.x < VanillaLevelExt.PROJECTILE_LEFT_BORDER || bounds.min.x > VanillaLevelExt.PROJECTILE_RIGHT_BORDER;
                var y_out = position.y > VanillaLevelExt.PROJECTILE_TOP_BORDER || position.y < VanillaLevelExt.PROJECTILE_BOTTOM_BORDER;
                var z_out = position.z > VanillaLevelExt.PROJECTILE_UP_BORDER || position.z < VanillaLevelExt.PROJECTILE_DOWN_BORDER;
                if (x_out)
                    position.x -= (VanillaLevelExt.PROJECTILE_RIGHT_BORDER - VanillaLevelExt.PROJECTILE_LEFT_BORDER) * MathF.Sign(vel.x);
                if (y_out)
                    position.y -= (VanillaLevelExt.PROJECTILE_TOP_BORDER - VanillaLevelExt.PROJECTILE_BOTTOM_BORDER) * MathF.Sign(vel.y);
                if (z_out)
                    position.z -= (VanillaLevelExt.PROJECTILE_UP_BORDER - VanillaLevelExt.PROJECTILE_DOWN_BORDER) * MathF.Sign(vel.z);
                ent.Position = position;
                var buff1 = ent.AddBuff<ProjectileWaitBuff>();
                buff1.SetProperty(ProjectileWaitBuff.PROP_TIMEOUT, 30);
            }
        }
        private bool IsOutsideView(Entity proj)
        {
            var bounds = proj.GetBounds();
            var position = proj.Position;
            return bounds.max.x < VanillaLevelExt.PROJECTILE_LEFT_BORDER ||
                bounds.min.x > VanillaLevelExt.PROJECTILE_RIGHT_BORDER ||
                position.z > VanillaLevelExt.PROJECTILE_UP_BORDER ||
                position.z < VanillaLevelExt.PROJECTILE_DOWN_BORDER ||
                position.y > VanillaLevelExt.PROJECTILE_TOP_BORDER ||
                position.y < VanillaLevelExt.PROJECTILE_BOTTOM_BORDER;
        }
    }
}
