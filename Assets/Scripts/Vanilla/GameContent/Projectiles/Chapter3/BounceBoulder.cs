using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.bounceBoulder)]
    public class BounceBoulder : ProjectileBehaviour
    {
        public BounceBoulder(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Update(Entity projectile)
        {
            base.Update(projectile);
            float angleSpeed = -projectile.Velocity.x * 2.5f;
            projectile.RenderRotation += Vector3.forward * angleSpeed;
        }
        public override void PostContactGround(Entity projectile, Vector3 velocity)
        {
            base.PostContactGround(projectile, velocity);
            projectile.Velocity = new Vector3(projectile.Velocity.x, 10f, projectile.Velocity.z);
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var projectile = hitResult.Projectile;
            var other = hitResult.Other;
            projectile.Velocity = new Vector3(projectile.Velocity.x, 10f, projectile.Velocity.z);
            if (other.Type == EntityTypes.ENEMY)
            {
                var vel = other.Velocity;
                vel.x += 6 * Mathf.Sign(projectile.Velocity.x) * other.GetWeakKnockbackMultiplier();
                other.Velocity = vel;
                projectile.PlaySound(VanillaSoundID.bash);
            }
        }
    }
}
