using System.Linq;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Shells;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.shuriken)]
    public class Shuriken : ProjectileBehaviour
    {
        public Shuriken(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            entity.RenderRotation += Vector3.back * 30f;
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damageOutput)
        {
            base.PostHitEntity(hitResult, damageOutput);
            if (damageOutput == null)
                return;
            var reflectSlice = damageOutput.GetAllResults().Any(e => e?.ShellDefinition?.ReflectSlice() ?? false);
            if (reflectSlice)
            {
                hitResult.Pierce = true;
                var projectile = hitResult.Projectile;
                var entity = hitResult.Other;
                Knife.Deflect(entity, projectile);
            }
        }
    }
}
