using MVZ2.GameContent.Pickups;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.emeraldBall)]
    public class EmeraldBall : GoldenBall
    {
        public EmeraldBall(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var projectile = hitResult.Projectile;
            if (projectile.RNG.Next(100) < 25)
            {
                projectile.Produce(VanillaPickupID.emerald);
            }
        }
    }
}
