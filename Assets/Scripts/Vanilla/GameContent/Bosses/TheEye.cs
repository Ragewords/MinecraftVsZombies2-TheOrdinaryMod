using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;
using static MVZ2.GameContent.Buffs.VanillaBuffID;
using static MVZ2.GameContent.Buffs.VanillaBuffNames;

namespace MVZ2.GameContent.Bosses
{
    [EntityBehaviourDefinition(VanillaBossNames.theEye)]
    public class TheEye : BossBehaviour
    {
        public TheEye(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetTimeout(entity, new FrameTimer(6300));
            SetMoveTimer(entity, new FrameTimer(150));
            SetTransformTimer(entity, new FrameTimer(300));
            var flyBuff = entity.AddBuff<FlyBuff>();
            flyBuff.SetProperty(FlyBuff.PROP_FLY_SPEED, 0.2f);
            flyBuff.SetProperty(FlyBuff.PROP_FLY_SPEED_FACTOR, 0.5f);
            flyBuff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 20);
            SetProjectileRNG(entity, new RandomGenerator(entity.RNG.Next()));
            SetMoveRNG(entity, new RandomGenerator(entity.RNG.Next()));
            var level = entity.Level;
            SetMoveDisplacement(entity, new Vector3(level.GetEntityColumnX(4), 0, level.GetEntityLaneZ(2)));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (entity.IsDead)
                return;
            StartMove(entity);
            var deathTimer = GetTimeout(entity);
            deathTimer.Run();
            if (deathTimer.Expired)
            {
                foreach (var wall in entity.Level.FindEntities(VanillaEffectID.crushingWalls))
                {
                    CrushingWalls.CloseSecondTime(wall);
                }
                entity.PlaySound(VanillaSoundID.fling);
            }
            var moveTimer = GetMoveTimer(entity);
            moveTimer.Run();
            if (moveTimer.Expired)
            {
                int endLane;
                int endColumn;
                var level = entity.Level;
                var moveRNG = GetMoveRNG(entity);
                do
                {
                    endLane = moveRNG.Next(0, level.GetMaxLaneCount());
                    endColumn = moveRNG.Next(0, level.GetMaxColumnCount());
                }
                while (endLane == entity.GetLane() && endColumn == entity.GetColumn());

                float endX = level.GetEntityColumnX(endColumn);
                float endZ = level.GetEntityLaneZ(endLane);
                SetMoveDisplacement(entity, new Vector3(endX, entity.Position.y, endZ));

                var rng = GetProjectileRNG(entity);
                foreach (var target in entity.Level.GetEntities())
                {
                    if (target.Type != EntityTypes.ENEMY)
                        continue;
                    target.RandomChangeAdjacentLane(entity.RNG);
                }
                entity.PlaySound(VanillaSoundID.cave);
                entity.PlaySound(VanillaSoundID.reverseVampire);
                moveTimer.ResetTime(300);
                var buff = entity.GetFirstBuff<FlyBuff>();
                buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, entity.RNG.Next(0, 80));
            }
            var transTimer = GetTransformTimer(entity);
            transTimer.Run();
            if (transTimer.Expired)
            {
                var rng = GetProjectileRNG(entity);
                NamespaceID[] pool = projectilePool;
                NamespaceID[] pool1 = enemyPool;
                foreach (var target in entity.Level.GetEntities())
                {
                    if (target.Type != EntityTypes.ENEMY && target.Type != EntityTypes.PLANT)
                        continue;
                    if (target.Type == EntityTypes.ENEMY)
                    {
                        if (target.IsEntityOf(pool1.Random(rng)))
                            continue;
                        var random = entity.Level.Spawn(pool1.Random(rng), target.Position, entity);
                        random.SetFactionAndDirection(target.GetFaction());
                        target.Level.Spawn(VanillaEffectID.smokeCluster, target.Position, entity);
                        target.Remove();
                    }
                    else if (target.Type == EntityTypes.PLANT)
                    {
                        if (target.IsDispenser())
                        {
                            if (target.GetShootParams().projectileID == pool.Random(rng))
                                continue;
                            target.SetProjectileID(pool.Random(rng));
                            target.Level.Spawn(VanillaEffectID.smokeCluster, target.Position, entity);
                        }
                    }
                }
                entity.PlaySound(VanillaSoundID.odd);
                transTimer.Reset();
            }
        }
        private void StartMove(Entity entity)
        {
            var destination = GetMoveDisplacement(entity);
            var pos = entity.Position;
            if (destination.x != entity.Position.x && destination.z != entity.Position.z)
            {
                pos.y += 20;
                entity.Position = pos;
                if (pos.y >= 400)
                {
                    entity.Position = GetMoveDisplacement(entity) + Vector3.up * 400;
                }
            }  
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            if (entity.IsDead)
            {
                entity.Timeout--;
                if (entity.Timeout == 30)
                {
                    entity.PlaySound(VanillaSoundID.theEyeDeath2);
                }
                else if (entity.Timeout <= 0)
                {
                    entity.Spawn(VanillaEffectID.boneParticles, entity.GetCenter());
                    var blood = entity.Spawn(VanillaEffectID.bloodParticles, entity.GetCenter());
                    blood.SetTint(Color.black);
                    var cluster = entity.Spawn(VanillaEffectID.smokeCluster, entity.GetCenter());
                    cluster.SetTint(Color.black);
                    cluster.SetSize(Vector3.one * 180);
                    var expPart = entity.Level.Spawn(VanillaEffectID.explosion, entity.Position, entity);
                    expPart.SetSize(Vector3.one * 240);
                    expPart.SetTint(Color.black);
                    entity.Level.ShakeScreen(60, 0, 45);
                    entity.Remove();
                }
            }
            entity.SetAnimationBool("IsDead", entity.IsDead);
        }
        public override void PostDeath(Entity entity, DeathInfo deathInfo)
        {
            base.PostDeath(entity, deathInfo);

            entity.Spawn(VanillaEffectID.darkMatterParticles, entity.GetCenter());
            entity.PlaySound(VanillaSoundID.theEyeDeath1);
            entity.SetAnimationBool("IsDead", true);
            entity.Timeout = 200;
            CheckTimerAndWallsDestruction(entity);
        }
        private static void CheckTimerAndWallsDestruction(Entity entity)
        {
            var level = entity.Level;
            var hasAliveReaper = level.EntityExists(e => e != entity && e.IsEntityOf(VanillaBossID.nightmareaper) && !e.IsDead);
            if (!hasAliveReaper)
            {
                foreach (var timer in level.FindEntities(VanillaEffectID.nightmareaperTimer))
                {
                    timer.Remove();
                }
                foreach (var walls in level.FindEntities(VanillaEffectID.crushingWalls))
                {
                    walls.Remove();
                }
            }
        }
        private static NamespaceID[] enemyPool = new NamespaceID[]
        {
            VanillaEnemyID.zombie,
            VanillaEnemyID.leatherCappedZombie,
            VanillaEnemyID.ironHelmettedZombie,
            VanillaEnemyID.spider,
            VanillaEnemyID.caveSpider,
            VanillaEnemyID.ghast,
            VanillaEnemyID.motherTerror,
            VanillaEnemyID.parasiteTerror,
        };
        private static NamespaceID[] projectilePool = new NamespaceID[]
        {
            VanillaProjectileID.arrow,
            VanillaProjectileID.spike,
            VanillaProjectileID.dart,
        };
        public static RandomGenerator GetMoveRNG(Entity boss) => boss.GetBehaviourField<RandomGenerator>(ID, PROP_MOVE_RNG);
        public static void SetMoveRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(ID, PROP_MOVE_RNG, value);
        public static RandomGenerator GetProjectileRNG(Entity boss) => boss.GetBehaviourField<RandomGenerator>(ID, PROP_PROJECTILE_RNG);
        public static void SetProjectileRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(ID, PROP_PROJECTILE_RNG, value);
        public static FrameTimer GetTimeout(Entity boss) => boss.GetBehaviourField<FrameTimer>(ID, PROP_DEATH_TIMEOUT);
        public static void SetTimeout(Entity boss, FrameTimer value) => boss.SetBehaviourField(ID, PROP_DEATH_TIMEOUT, value);
        public static FrameTimer GetMoveTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(ID, PROP_MOVE_TIMER);
        public static void SetMoveTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(ID, PROP_MOVE_TIMER, value);
        public static FrameTimer GetTransformTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(ID, PROP_TRANSFORM_TIMER);
        public static void SetTransformTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(ID, PROP_TRANSFORM_TIMER, value);
        public static Vector3 GetMoveDisplacement(Entity boss) => boss.GetBehaviourField<Vector3>(ID, PROP_MOVE_DISPLACEMENT);
        public static void SetMoveDisplacement(Entity boss, Vector3 value) => boss.SetBehaviourField(ID, PROP_MOVE_DISPLACEMENT, value);
        public static readonly NamespaceID ID = VanillaBossID.theEye;
        public static readonly VanillaEntityPropertyMeta PROP_MOVE_RNG = new VanillaEntityPropertyMeta("MoveRNG");
        public static readonly VanillaEntityPropertyMeta PROP_DEATH_TIMEOUT = new VanillaEntityPropertyMeta("DeathTimeout");
        public static readonly VanillaEntityPropertyMeta PROP_MOVE_TIMER = new VanillaEntityPropertyMeta("MoveTimer");
        public static readonly VanillaEntityPropertyMeta PROP_TRANSFORM_TIMER = new VanillaEntityPropertyMeta("TransformTimer");
        public static readonly VanillaEntityPropertyMeta PROP_PROJECTILE_RNG = new VanillaEntityPropertyMeta("ProjectileRNG");
        public static readonly VanillaEntityPropertyMeta PROP_MOVE_DISPLACEMENT = new VanillaEntityPropertyMeta("MoveDisplacement");
    }
}
