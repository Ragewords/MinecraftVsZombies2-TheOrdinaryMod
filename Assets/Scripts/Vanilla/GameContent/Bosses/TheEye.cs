using System.Linq;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Grids;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using Tools.Mathematics;
using UnityEngine;

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
            SetMoveTimer(entity, new FrameTimer(300));
            SetStateTimer(entity, new FrameTimer(300));
            SetTransformTimer(entity, new FrameTimer(40));
            var flyBuff = entity.AddBuff<FlyBuff>();
            flyBuff.SetProperty(FlyBuff.PROP_FLY_SPEED, 0.2f);
            flyBuff.SetProperty(FlyBuff.PROP_FLY_SPEED_FACTOR, 0.5f);
            flyBuff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 60f);
            SetProjectileRNG(entity, new RandomGenerator(entity.RNG.Next()));
            SetMoveRNG(entity, new RandomGenerator(entity.RNG.Next()));
            SetSoundPlayed(entity, false);
            SetAttackState(entity, STATE_ENEMY_MOVE);
            var level = entity.Level;
        }
        public override void PreTakeDamage(DamageInput input, CallbackResult result)
        {
            base.PreTakeDamage(input, result);
            if (input.Amount > 300)
            {
                input.SetAmount(300);
            }
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (entity.IsDead)
                return;
            MoveUpdate(entity);
            AttackUpdate(entity);
            entity.Level.Explode(entity.Position, 200, entity.GetFaction(), 0.1f, new DamageEffectList(VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN), entity);

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
                    var expPart = entity.Level.Spawn(VanillaEffectID.explosion, entity.GetCenter(), entity);
                    expPart.SetSize(Vector3.one * 240);
                    expPart.SetTint(Color.black);
                    entity.Level.ShakeScreen(60, 0, 30);
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
        }
        #region Move
        private void MoveUpdate(Entity entity)
        {
            var timer = GetMoveTimer(entity);
            timer.Run();
            if (timer.Expired)
            {
                timer.Reset();
                var buff = entity.GetFirstBuff<FlyBuff>();
                buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, entity.RNG.Next(2, 8) * 10f);
                StartMove(entity);
            }
            var motionTime = GetMoveTimeout(entity);
            if (motionTime <= 0)
                return;
            float lastPercent = Mathf.Clamp01(1 - motionTime / (float)MAX_MOVE_TIMEOUT);
            float lastMovePercent = MathTool.EaseInAndOut(lastPercent);

            motionTime--;

            SetMoveTimeout(entity, motionTime);

            float percent = Mathf.Clamp01(1 - motionTime / (float)MAX_MOVE_TIMEOUT);
            float movePercent = MathTool.EaseInAndOut(percent);
            var displacement = GetMoveDisplacement(entity);

            float addedPercent = movePercent - lastMovePercent;
            entity.Position += addedPercent * displacement;

            // End Moving.
            if (motionTime <= 0)
            {
                motionTime = 0;
                SetMoveDisplacement(entity, Vector3.zero);
            }
        }
        private void StartMove(Entity entity)
        {
            SetMoveTimeout(entity, MAX_MOVE_TIMEOUT);
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
            SetMoveDisplacement(entity, new Vector3(endX - entity.Position.x, 0, endZ - entity.Position.z));
        }
        #endregion

        #region Attack
        private void AttackUpdate(Entity entity)
        {
            var timer = GetStateTimer(entity);
            timer.Run();
            int attackState = GetAttackState(entity);
            if (timer.Expired)
            {
                switch (attackState)
                {
                    case STATE_ENEMY_MOVE:
                        {
                            foreach (var target in entity.Level.GetEntities())
                            {
                                if (target.Type != EntityTypes.ENEMY)
                                    continue;
                                target.RandomChangeAdjacentLane(entity.RNG);
                            }
                            entity.PlaySound(VanillaSoundID.cave);
                            entity.PlaySound(VanillaSoundID.reverseVampire);
                            SetAttackState(entity, STATE_DARK_MATTER);
                            timer.Reset();
                        }
                        break;
                    case STATE_DARK_MATTER:
                        {
                            if (!IsSoundPlayed(entity))
                            {
                                entity.PlaySound(VanillaSoundID.magnetic);
                                SetSoundPlayed(entity, true);
                            }

                            entity.SetAnimationInt("AttackState", 1);
                            var transTimer = GetTransformTimer(entity);
                            transTimer.Run();
                            if (transTimer.Expired)
                            {
                                var level = entity.Level;
                                var contraption = level.FindEntities(e => e.Type == EntityTypes.PLANT && e.IsHostile(entity)).RandomTake(1, GetProjectileRNG(entity));
                                foreach (var target in contraption)
                                {
                                    var targetPos = target.Position;
                                    var projectileID = VanillaProjectileID.darkMatterBall;
                                    var projectileDefinition = entity.Level.Content.GetEntityDefinition(projectileID);
                                    var projectileGravity = projectileDefinition?.GetGravity() ?? 0;

                                    var shotParams = entity.GetShootParams();
                                    shotParams.position = entity.Position;
                                    shotParams.soundID = VanillaSoundID.odd;
                                    shotParams.projectileID = projectileID;
                                    shotParams.velocity = VanillaProjectileExt.GetLobVelocityByTime(entity.Position, targetPos, 45, projectileGravity);
                                    entity.ShootProjectile(shotParams);
                                    var expPart = entity.Level.Spawn(VanillaEffectID.explosion, entity.GetCenter(), entity);
                                    expPart.SetSize(Vector3.one * 100);
                                    expPart.SetTint(Color.black);
                                }
                                SetAttackState(entity, STATE_SUMMON);
                                SetSoundPlayed(entity, false);
                                entity.SetAnimationInt("AttackState", 0);
                                transTimer.ResetTime(150);
                                timer.Reset();
                            }
                        }
                        break;
                    case STATE_SUMMON:
                        {
                            if (!IsSoundPlayed(entity))
                            {
                                entity.PlaySound(VanillaSoundID.nyaightmareScream);
                                SetSoundPlayed(entity, true);
                            }
                            entity.SetAnimationInt("AttackState", 2);

                            var transTimer = GetTransformTimer(entity);
                            transTimer.Run();
                            if (transTimer.Expired)
                            {
                                var level = entity.Level;
                                var grids = level.GetAllGrids();
                                var targetGrids = grids.Where(g => g.IsWater());
                                entity.PlaySound(VanillaSoundID.odd);
                                foreach (var grid in targetGrids)
                                {
                                    var pool = summonPool.Random(GetProjectileRNG(entity));
                                    Vector3 pos = grid.GetEntityPosition();
                                    level.Spawn(pool, pos, entity);
                                    var expPart = entity.Level.Spawn(VanillaEffectID.smokeCluster, pos, entity);
                                    expPart.SetSize(Vector3.one * 800);
                                    expPart.SetTint(Color.black);
                                }
                                entity.SetAnimationInt("AttackState", 0);
                                SetSoundPlayed(entity, false);
                                SetAttackState(entity, STATE_ENEMY_MOVE);
                                transTimer.ResetTime(40);
                                timer.Reset();
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        #region ÊôÐÔ
        public static FrameTimer GetMoveTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(ID, PROP_MOVE_TIMER);
        public static void SetMoveTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(ID, PROP_MOVE_TIMER, value);
        public static int GetMoveTimeout(Entity boss) => boss.GetBehaviourField<int>(ID, PROP_MOVE_TIMEOUT);
        public static void SetMoveTimeout(Entity boss, int value) => boss.SetBehaviourField(ID, PROP_MOVE_TIMEOUT, value);
        public static Vector3 GetMoveDisplacement(Entity boss) => boss.GetBehaviourField<Vector3>(ID, PROP_MOVE_DISPLACEMENT);
        public static void SetMoveDisplacement(Entity boss, Vector3 value) => boss.SetBehaviourField(ID, PROP_MOVE_DISPLACEMENT, value);
        public static RandomGenerator GetMoveRNG(Entity boss) => boss.GetBehaviourField<RandomGenerator>(ID, PROP_MOVE_RNG);
        public static void SetMoveRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(ID, PROP_MOVE_RNG, value);
        public static RandomGenerator GetProjectileRNG(Entity boss) => boss.GetBehaviourField<RandomGenerator>(ID, PROP_PROJECTILE_RNG);
        public static void SetProjectileRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(ID, PROP_PROJECTILE_RNG, value);
        public static FrameTimer GetTransformTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(ID, PROP_TRANSFORM_TIMER);
        public static void SetTransformTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(ID, PROP_TRANSFORM_TIMER, value);
        public static int GetAttackState(Entity boss) => boss.GetBehaviourField<int>(ID, PROP_ATTACK_STATE);
        public static void SetAttackState(Entity boss, int value) => boss.SetBehaviourField(ID, PROP_ATTACK_STATE, value);
        public static bool IsSoundPlayed(Entity boss) => boss.GetBehaviourField<bool>(ID, PROP_SOUND);
        public static void SetSoundPlayed(Entity boss, bool value) => boss.SetBehaviourField(ID, PROP_SOUND, value);
        public static FrameTimer GetStateTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);
        public static void SetStateTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(ID, PROP_STATE_TIMER, value);
        #endregion

        public const int STATE_ENEMY_MOVE = 0;
        public const int STATE_DARK_MATTER = 1;
        public const int STATE_SUMMON = 2;

        public const int MAX_MOVE_TIMEOUT = 60;

        public static readonly NamespaceID ID = VanillaBossID.theEye;

        public static readonly VanillaEntityPropertyMeta<RandomGenerator> PROP_MOVE_RNG = new VanillaEntityPropertyMeta<RandomGenerator>("MoveRNG");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_MOVE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("MoveTimer");
        public static readonly VanillaEntityPropertyMeta<int> PROP_MOVE_TIMEOUT = new VanillaEntityPropertyMeta<int>("MoveTimeout");
        public static readonly VanillaEntityPropertyMeta<Vector3> PROP_MOVE_DISPLACEMENT = new VanillaEntityPropertyMeta<Vector3>("MoveDisplacement");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_TRANSFORM_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("TransformTimer");
        public static readonly VanillaEntityPropertyMeta<RandomGenerator> PROP_PROJECTILE_RNG = new VanillaEntityPropertyMeta<RandomGenerator>("ProjectileRNG");
        public static readonly VanillaEntityPropertyMeta<int> PROP_ATTACK_STATE = new VanillaEntityPropertyMeta<int>("AttackState");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_SOUND = new VanillaEntityPropertyMeta<bool>("Sound");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");

        private static NamespaceID[] summonPool = new NamespaceID[]
        {
            VanillaEnemyID.zombie,
            VanillaEnemyID.leatherCappedZombie,
            VanillaEnemyID.ironHelmettedZombie,
            VanillaEnemyID.spider,
            VanillaEnemyID.caveSpider,
            VanillaEnemyID.motherTerror,
        };
    }
}
