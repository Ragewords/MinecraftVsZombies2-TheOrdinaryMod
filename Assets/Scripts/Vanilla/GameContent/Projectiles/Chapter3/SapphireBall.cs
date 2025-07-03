using MVZ2.GameContent.Pickups;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.sapphireBall)]
    public class SapphireBall : GoldenBall
    {
        public SapphireBall(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var projectile = hitResult.Projectile;
            if (projectile.RNG.Next(100) < 50)
            {
                projectile.Produce(VanillaPickupID.sapphire);
            }
        }
    }
}
