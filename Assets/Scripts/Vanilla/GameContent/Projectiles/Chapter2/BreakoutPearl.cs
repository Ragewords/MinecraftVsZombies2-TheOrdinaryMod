using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.breakoutPearl)]
    public class BreakoutPearl : ProjectileBehaviour
    {
        public BreakoutPearl(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.Timeout = 30;
        }

        protected override void PreHitEntity(ProjectileHitInput hit, DamageInput damage, CallbackResult result)
        {
            base.PreHitEntity(hit, damage, result);
            var bullet = hit.Projectile;
            if (bullet.Parent != null && bullet.Parent.Exists())
            {
                result.SetFinalValue(false);
                return;
            }
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);

            var bullet = hitResult.Projectile;
            var target = hitResult.Other;
            var bullet2Target = bullet.Position - target.Position;
            var newDirection = bullet2Target;
            newDirection.y = 0;
            var velocity = bullet.Velocity;
            var newVelocity = newDirection.normalized * velocity.magnitude;
            velocity.x = newVelocity.x;
            velocity.y = 0;
            velocity.z = newVelocity.z;
            bullet.Velocity = velocity;
            for (int i = 0; i < 8; i++)
            {
                var direction = Quaternion.Euler(0, 45 - i * 45, 0) * bullet.Velocity.normalized;
                var vel = direction * 15;
                var projectile = target.ShootProjectile(new ShootParams()
                {
                    projectileID = GetRandomProductionEnemyID(bullet.RNG),
                    position = bullet.Position,
                    velocity = vel,
                    faction = bullet.GetFaction(),
                    damage = bullet.GetDamage() / 8
                });
                projectile.SetHSVToColor(Color.magenta);
            }
        }
        public override void Update(Entity projectile)
        {
            var bounds = projectile.GetBounds();
            if (bounds.max.x < VanillaLevelExt.PROJECTILE_LEFT_BORDER)
            {
                projectile.Remove();
                return;
            }

            projectile.Timeout = 30;
            var level = projectile.Level;
            var position = projectile.Position;
            var velocity = projectile.Velocity;
            if (position.x < MAX_X)
            {
                position.x = MAX_X;
                velocity.x *= -1;
                projectile.PlaySound(VanillaSoundID.pearlTouch);
            }
            if (position.z > level.GetGridTopZ())
            {
                position.z = level.GetGridTopZ();
                velocity.z *= -1;
                projectile.PlaySound(VanillaSoundID.pearlTouch);
            }
            else if (position.z < level.GetGridBottomZ())
            {
                position.z = level.GetGridBottomZ();
                velocity.z *= -1;
                projectile.PlaySound(VanillaSoundID.pearlTouch);
            }
            projectile.Position = position;
            projectile.Velocity = velocity;
            base.Update(projectile);
        }
        private NamespaceID GetRandomProductionEnemyID(RandomGenerator rng)
        {
            var index = rng.WeightedRandom(productionPoolWeights);
            return productionPool[index];
        }
        private static NamespaceID[] productionPool = new NamespaceID[]
        {
            VanillaProjectileID.reflectionBullet,
            VanillaProjectileID.arrowBullet,
            VanillaProjectileID.reflectionSonicwave
        };
        private static int[] productionPoolWeights = new int[]
        {
            7,
            2,
            1
        };

        public const float MAX_X = VanillaLevelExt.LEFT_BORDER + 40;
    }
}
