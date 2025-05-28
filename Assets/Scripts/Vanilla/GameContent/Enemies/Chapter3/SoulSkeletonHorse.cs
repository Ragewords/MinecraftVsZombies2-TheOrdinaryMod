using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.soulSkeletonHorse)]
    public class SoulSkeletonHorse : MeleeEnemy
    {
        public SoulSkeletonHorse(string nsp, string name) : base(nsp, name)
        {
            detector = new SkeletonHorseJumpDetector();
            blocksJumpDetector = new SkeletonHorseBlocksJumpDetector();
            AddModifier(new FloatModifier(VanillaEnemyProps.SPEED, NumberOperator.Multiply, PROP_SPEED_MULTIPLIER));
        }

        #region 回调
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetLandTimer(entity, new FrameTimer(15));
            int jumpTimes = 1;
            SetGallopTime(entity, jumpTimes);
        }
        public override void PostContactGround(Entity entity, Vector3 velocity)
        {
            base.PostContactGround(entity, velocity);
            if (entity.State == STATE_JUMP)
            {
                var stateTimer = GetLandTimer(entity);
                stateTimer.Reset();
                SetJumpState(entity, JUMP_STATE_LAND);
                entity.PlaySound(VanillaSoundID.horseGallop);
                entity.Level.Spawn(VanillaEffectID.soulfireBlast, entity.Position, entity);
                for (int i = 0; i < 8; i++)
                {
                    var direction = Quaternion.Euler(0, i * 45, 0) * new Vector3(1, 0, 0);
                    var vel = direction * 12;
                    var projectile = entity.ShootProjectile(VanillaProjectileID.soulfireBall, vel);
                    projectile.Position = entity.Position;
                    projectile.SetDamage(60);
                    SoulfireBall.SetSplit(projectile, true);
                }
            }
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            entity.SetProperty(PROP_SPEED_MULTIPLIER, entity.State == STATE_GALLOP ? 2f : 1f);

            if (entity.State == STATE_GALLOP)
            {
                if (detector.DetectExists(entity))
                {
                    AddGallopTime(entity, -1);
                    var vel = entity.Velocity;
                    vel.x = entity.GetFacingX() * 5;
                    vel.y = 12;
                    entity.Velocity = vel;
                    SetJumpState(entity, JUMP_STATE_JUMP);
                    entity.PlaySound(VanillaSoundID.horseGallop);
                    entity.Level.Spawn(VanillaEffectID.soulfireBurn, entity.Position, entity);
                    for (int i = 0; i < 6; i++)
                    {
                        var direction = Quaternion.Euler(0, i * 60, 0) * new Vector3(1, 0, 0);
                        var velo = direction * 12;
                        var projectile = entity.ShootProjectile(VanillaProjectileID.soulfireBall, velo);
                        projectile.Position = entity.Position;
                        projectile.SetDamage(60);
                        SoulfireBall.SetSplit(projectile, true);
                    }
                }

                entity.UpdateWalkVelocity();

                var soundTime = GetGallopSoundTime(entity);
                soundTime--;
                if (soundTime <= 0)
                {
                    soundTime = GALLOP_SOUND_INTERVAL;
                    entity.PlaySound(VanillaSoundID.horseGallop);
                    entity.Level.Spawn(VanillaEffectID.soulfire, entity.Position, entity);
                }
                SetGallopSoundTime(entity, soundTime);
            }
            else if (entity.State == STATE_JUMP)
            {
                if (blocksJumpDetector.DetectExists(entity))
                {
                    var vel = entity.Velocity;
                    vel.x = 0;
                    vel.y = 0;
                    entity.Velocity = vel;
                    SetJumpState(entity, JUMP_STATE_NONE);
                    entity.Stun(30);
                    entity.PlaySound(VanillaSoundID.bonk);
                    entity.Level.Spawn(VanillaEffectID.soulfireBlast, entity.Position, entity);
                    for (int i = 0; i < 10; i++)
                    {
                        var direction = Quaternion.Euler(0, i * 36, 0) * new Vector3(1, 0, 0);
                        var velo = direction * 12;
                        var projectile = entity.ShootProjectile(VanillaProjectileID.soulfireBall, velo);
                        projectile.Position = entity.Position;
                        projectile.SetDamage(60);
                        SoulfireBall.SetSplit(projectile, true);
                        SoulfireBall.SetBlast(projectile, true);
                    }
                }
            }
            else if (entity.State == STATE_LAND)
            {
                var stateTimer = GetLandTimer(entity);
                stateTimer.Run();
                if (stateTimer.Expired)
                {
                    stateTimer.Reset();
                    SetJumpState(entity, JUMP_STATE_NONE);
                }
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            // 设置血量状态。
            entity.SetAnimationInt("HealthState", entity.GetHealthState(2));
        }
        #endregion

        #region 敌人回调
        protected override bool ValidateMeleeTarget(Entity enemy, Entity target)
        {
            if (!base.ValidateMeleeTarget(enemy, target))
                return false;
            if (!Detection.IsInFrontOf(enemy, target))
                return false;
            return true;
        }
        protected override int GetActionState(Entity enemy)
        {
            var state = base.GetActionState(enemy);
            if (state == VanillaEntityStates.WALK || state == VanillaEntityStates.ATTACK)
            {
                var jumpState = GetJumpState(enemy);
                if (jumpState == JUMP_STATE_LAND)
                {
                    return STATE_LAND;
                }
                else if (jumpState == JUMP_STATE_JUMP)
                {
                    return STATE_JUMP;
                }
                else if (GetGallopTime(enemy) > 0)
                {
                    return STATE_GALLOP;
                }
            }
            return state;
        }
        #endregion

        #region 字段
        public static int GetJumpState(Entity entity) => entity.GetBehaviourField<int>(ID, FIELD_JUMP_STATE);
        public static void SetJumpState(Entity entity, int value) => entity.SetBehaviourField(ID, FIELD_JUMP_STATE, value);

        public static FrameTimer GetLandTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, FIELD_LAND_TIMER);
        public static void SetLandTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(ID, FIELD_LAND_TIMER, value);

        public static int GetGallopTime(Entity entity) => entity.GetBehaviourField<int>(ID, FIELD_GALLOP_TIME);
        public static void SetGallopTime(Entity entity, int value) => entity.SetBehaviourField(ID, FIELD_GALLOP_TIME, value);
        public static void AddGallopTime(Entity entity, int value) => SetGallopTime(entity, GetGallopTime(entity) + value);

        public static int GetGallopSoundTime(Entity entity) => entity.GetBehaviourField<int>(ID, FIELD_GALLOP_SOUND_TIME);
        public static void SetGallopSoundTime(Entity entity, int value) => entity.SetBehaviourField(ID, FIELD_GALLOP_SOUND_TIME, value);
        #endregion

        public static readonly VanillaEntityPropertyMeta<int> FIELD_GALLOP_TIME = new VanillaEntityPropertyMeta<int>("GallopTime");
        public static readonly VanillaEntityPropertyMeta<int> FIELD_GALLOP_SOUND_TIME = new VanillaEntityPropertyMeta<int>("GallopSoundTime");
        public static readonly VanillaEntityPropertyMeta<int> FIELD_JUMP_STATE = new VanillaEntityPropertyMeta<int>("JumpState");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> FIELD_LAND_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("LandTimer");
        public static readonly VanillaEntityPropertyMeta<float> PROP_SPEED_MULTIPLIER = new VanillaEntityPropertyMeta<float>("SpeedMultiplier");
        public const int GALLOP_SOUND_INTERVAL = 15;
        public const int JUMP_STATE_NONE = 0;
        public const int JUMP_STATE_JUMP = 1;
        public const int JUMP_STATE_LAND = 2;
        public const int STATE_GALLOP = VanillaEntityStates.SKELETON_HORSE_GALLOP;
        public const int STATE_JUMP = VanillaEntityStates.SKELETON_HORSE_JUMP;
        public const int STATE_LAND = VanillaEntityStates.SKELETON_HORSE_LAND;
        private Detector detector;
        private Detector blocksJumpDetector;
        private static readonly NamespaceID ID = VanillaEnemyID.soulSkeletonHorse;
    }
}
