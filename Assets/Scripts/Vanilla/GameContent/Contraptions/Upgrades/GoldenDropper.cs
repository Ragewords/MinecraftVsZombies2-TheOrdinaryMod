using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.goldenDropper)]
    public class GoldenDropper : DispenserFamily
    {
        public GoldenDropper(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetRNG(entity, new RandomGenerator(entity.RNG.Next()));
            InitShootTimer(entity);
        }
        public override void PostTakeDamage(DamageOutput result)
        {
            base.PostTakeDamage(result);
            if (result.HasAnyFatal())
                result.Entity.Spawn(VanillaContraptionID.woodenDropper, result.Entity.Position);
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
            if (entity.RNG.Next(5) == 0)
            {
                var param = entity.GetShootParams();
                param.projectileID = GetRandomProjectileID(GetRNG(entity));
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
                var zspeed = rng.Next(-1.5f, 1.5f);
                var param = entity.GetShootParams();
                param.velocity = new Vector3(xspeed, yspeed, zspeed);
                if (entity.RNG.Next(5) == 0)
                {
                    param.projectileID = GetRandomProjectileID(GetRNG(entity));
                }
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
            VanillaProjectileID.emeraldBall,
            VanillaProjectileID.rubyBall,
            VanillaProjectileID.sapphireBall,
            VanillaProjectileID.diamondBall,
        };
        private static int[] projectilePoolWeights = new int[]
        {
            70,
            15,
            10,
            5,
        };
        public static RandomGenerator GetRNG(Entity boss) => boss.GetBehaviourField<RandomGenerator>(ID, PROP_RNG);
        public static void SetRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(ID, PROP_RNG, value);
        public static readonly VanillaEntityPropertyMeta<RandomGenerator> PROP_RNG = new VanillaEntityPropertyMeta<RandomGenerator>("RNG");
        public static readonly NamespaceID ID = VanillaContraptionID.goldenDropper;
    }
}
