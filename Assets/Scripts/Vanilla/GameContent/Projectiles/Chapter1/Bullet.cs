using System.Linq;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Shells;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.bullet)]
    public class Bullet : ProjectileBehaviour
    {
        public Bullet(string nsp, string name) : base(nsp, name)
        {
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
