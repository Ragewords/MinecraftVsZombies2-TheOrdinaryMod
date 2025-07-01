using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.stoneDropper)]
    public class StoneDropper : DispenserFamily
    {
        public StoneDropper(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            InitShootTimer(entity);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (!entity.IsEvoked())
            {
                ShootTick(entity);
                return;
            }
        }
        public override Entity Shoot(Entity entity)
        {
            if (entity.RNG.Next(4) == 0)
            {
                var param = entity.GetShootParams();
                param.projectileID = GetRandomProjectileID(entity.RNG);
                var yspeed = param.projectileID == VanillaProjectileID.bounceBoulder ? 10f : 0f;
                param.damage = param.projectileID == VanillaProjectileID.bounceBoulder ? entity.GetDamage() * 20 : entity.GetDamage() * 4;
                param.velocity.y = yspeed;
                return entity.ShootProjectile(param);
            }
            return base.Shoot(entity);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var rng = entity.RNG;
            for (int i = 0; i < 30; i++)
            {
                var xspeed = entity.GetFacingX() * rng.Next(10f, 18f);
                var yspeed = rng.Next(30f);
                var zspeed = rng.Next(-3f, 3f);
                var param = entity.GetShootParams();
                param.projectileID = VanillaProjectileID.boulder;
                param.damage = entity.GetDamage() * 4;
                param.velocity = new Vector3(xspeed, yspeed, zspeed);
                entity.ShootProjectile(param);
            }
            for (int i = 0; i < 3; i++)
            {
                var xspeed = entity.GetFacingX() * rng.Next(10f, 18f);
                var yspeed = 10f;
                var zspeed = -1f + i;
                var param = entity.GetShootParams();
                param.projectileID = VanillaProjectileID.bounceBoulder;
                param.damage = entity.GetDamage() * 20;
                param.velocity = new Vector3(xspeed, yspeed, zspeed);
                entity.ShootProjectile(param);
            }
            entity.PlaySound(VanillaSoundID.launch);
        }
        private NamespaceID GetRandomProjectileID(RandomGenerator rng)
        {
            var index = rng.WeightedRandom(projectilePoolWeights);
            return projectilePool[index];
        }
        private static NamespaceID[] projectilePool = new NamespaceID[]
        {
            VanillaProjectileID.boulder,
            VanillaProjectileID.bounceBoulder,
        };
        private static int[] projectilePoolWeights = new int[]
        {
            20,
            5
        };
    }
}
