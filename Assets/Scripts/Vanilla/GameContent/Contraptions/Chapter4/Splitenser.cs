using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Projectiles;
using MVZ2.GameContent.Stages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.splitenser)]
    public class Splitenser : DispenserFamily
    {
        public Splitenser(string nsp, string name) : base(nsp, name)
        {
            detectorBack = new DispenserDetector()
            {
                ignoreHighEnemy = true,
                reversed = true,
            };
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            InitShootTimer(entity);
            SetEvocationTimer(entity, new FrameTimer(120));
            SetFrontRepeatTimer(entity, new FrameTimer(REPEAT_INTERVAL));
            SetRepeatTimer(entity, new FrameTimer(REPEAT_INTERVAL));
            SetBackRepeatRNG(entity, new RandomGenerator(entity.RNG.Next()));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            FeverUpdate(entity);
            if (!entity.IsEvoked())
            {
                ShootTickSplit(entity);
                int repeatCount = GetRepeatCount(entity);
                if (repeatCount > 0)
                {
                    var repeatTimer = GetRepeatTimer(entity);
                    repeatTimer.Run(entity.GetAttackSpeed());
                    if (repeatTimer.Expired)
                    {
                        ShootBack(entity);
                        SetRepeatCount(entity, repeatCount - 1);
                        repeatTimer.Reset();
                    }
                }

                int FrepeatCount = GetFrontRepeatCount(entity);
                if (FrepeatCount > 0)
                {
                    var repeatTimer = GetFrontRepeatTimer(entity);
                    repeatTimer.Run(entity.GetAttackSpeed());
                    if (repeatTimer.Expired)
                    {
                        ShootFront(entity);
                        SetFrontRepeatCount(entity, FrepeatCount - 1);
                        repeatTimer.Reset();
                    }
                }
                return;
            }

            EvokedUpdate(entity);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationBool("Fever", IsFever(entity));
        }
        public void ShootTickSplit(Entity entity)
        {
            var shootTimer = GetShootTimer(entity);
            shootTimer.Run(entity.GetAttackSpeed());
            if (shootTimer.Expired)
            {
                var frontTarget = detector.Detect(entity);
                if (frontTarget != null)
                {
                    RepeatShootFront(entity);
                }
                var backTarget = detectorBack.Detect(entity);
                if (backTarget != null)
                {
                    RepeatShootBack(entity);
                }
                shootTimer.ResetTime(GetTimerTime(entity));
            }
        }
        public Entity ShootFront(Entity entity)
        {
            entity.TriggerAnimation("ShootFront");
            return entity.ShootProjectile();
        }
        public Entity ShootBack(Entity entity)
        {
            entity.TriggerAnimation("ShootBack");

            var param = entity.GetShootParams();

            var offset = entity.GetShotOffset();
            offset.x *= -1;
            offset = entity.ModifyShotOffset(offset);
            param.position = entity.Position + offset;

            var vel = param.velocity;
            vel.x *= -1;
            param.velocity = vel;

            return entity.ShootProjectile(param);
        }
        public Entity ShootLargeArrowBack(Entity entity)
        {
            entity.TriggerAnimation("ShootBack");

            var param = entity.GetShootParams();

            var offset = entity.GetShotOffset();
            offset.x *= -1;
            offset = entity.ModifyShotOffset(offset);
            param.position = entity.Position + offset;

            var vel = param.velocity;
            vel.x *= -1;
            param.velocity = vel.normalized;

            param.projectileID = VanillaProjectileID.largeArrow;
            param.damage = entity.GetDamage() * 30;
            param.soundID = VanillaSoundID.spellCard;

            return entity.ShootProjectile(param);
        }
        public Entity BurstShootBack(Entity entity, float multipiler)
        {
            entity.TriggerAnimation("ShootBack");

            var param = entity.GetShootParams();

            var offset = entity.GetShotOffset();
            offset.x *= -1;
            offset = entity.ModifyShotOffset(offset);
            param.position = entity.Position + offset;

            var vel = param.velocity;
            vel.x *= -1;
            param.velocity = vel.normalized * multipiler;

            param.soundID = null;

            return entity.ShootProjectile(param);
        }
        public void RepeatShootFront(Entity entity)
        {
            bool repeat4 = entity.RNG.Next(10) == 0 || IsFever(entity);
            int count = 1 + (repeat4 ? 3 : 0);
            SetFrontRepeatCount(entity, count);
            var repeatTimer = GetFrontRepeatTimer(entity);
            repeatTimer.ResetTime(Mathf.FloorToInt(10f / count));
            repeatTimer.Frame = 0;
        }
        public void RepeatShootBack(Entity entity)
        {
            bool repeat6 = GetBackRepeatRNG(entity).Next(2) == 0 || IsFever(entity);
            int count = 2 + (repeat6 ? 4 : 0);
            SetRepeatCount(entity, count);
            var repeatTimer = GetRepeatTimer(entity);
            repeatTimer.ResetTime(Mathf.FloorToInt(10f / count));
            repeatTimer.Frame = 0;
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var evocationTimer = GetEvocationTimer(entity);
            evocationTimer.Reset();
            entity.SetEvoked(true);
        }
        public void FeverUpdate(Entity entity)
        {
            if (!entity.Level.HasBehaviour<WaveStageBehaviour>())
                return;
            SetFever(entity, WaveStageBehaviour.IsHighWave(entity.Level));
            if (IsFever(entity))
            {
                if (PlaySound(entity))
                {
                    entity.PlaySound(VanillaSoundID.pearlBoost);
                    entity.AddBuff<DesirePotHighlightBuff>();
                    SetPlaySound(entity, false);
                }
            }
            else
            {
                SetPlaySound(entity, true);
            }
        }
        public static FrameTimer GetEvocationTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_EVOCATION_TIMER);
        public static void SetEvocationTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_EVOCATION_TIMER, timer);
        public static FrameTimer GetFrontRepeatTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_F_REPEAT_TIMER);
        public static void SetFrontRepeatTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_F_REPEAT_TIMER, timer);
        public static int GetFrontRepeatCount(Entity entity) => entity.GetBehaviourField<int>(PROP_F_REPEAT_COUNT);
        public static void SetFrontRepeatCount(Entity entity, int timer) => entity.SetBehaviourField(PROP_F_REPEAT_COUNT, timer);
        public static FrameTimer GetRepeatTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_REPEAT_TIMER);
        public static void SetRepeatTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_REPEAT_TIMER, timer);
        public static int GetRepeatCount(Entity entity) => entity.GetBehaviourField<int>(PROP_REPEAT_COUNT);
        public static void SetRepeatCount(Entity entity, int timer) => entity.SetBehaviourField(PROP_REPEAT_COUNT, timer);
        public static void SetBackRepeatRNG(Entity entity, RandomGenerator rng) => entity.SetBehaviourField(PROP_B_REPEAT_RNG, rng);
        public static RandomGenerator GetBackRepeatRNG(Entity entity) => entity.GetBehaviourField<RandomGenerator>(PROP_B_REPEAT_RNG);
        public static void SetFever(Entity entity, bool value) => entity.SetBehaviourField(PROP_FEVER, value);
        public static bool IsFever(Entity entity) => entity.GetBehaviourField<bool>(PROP_FEVER);
        public static void SetPlaySound(Entity entity, bool value) => entity.SetBehaviourField(PROP_PLAY_SOUND, value);
        public static bool PlaySound(Entity entity) => entity.GetBehaviourField<bool>(PROP_PLAY_SOUND);
        private void EvokedUpdate(Entity entity)
        {
            var evocationTimer = GetEvocationTimer(entity);
            evocationTimer.Run();
            if (evocationTimer.PassedInterval(2))
            {
                var frontProjectile = ShootFront(entity);
                frontProjectile.Velocity *= 2;

                var backProjectile = ShootBack(entity);
                backProjectile.Velocity *= 2;
            }
            if (evocationTimer.Expired)
            {
                ShootLargeArrowBack(entity);
                for (var i = 0; i < 10; i++)
                {
                    BurstShootBack(entity, i + 1);
                }
                entity.SetEvoked(false);
                var shootTimer = GetShootTimer(entity);
                shootTimer.Reset();
            }
        }
        private Detector detectorBack;
        public const int REPEAT_INTERVAL = 5;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_EVOCATION_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("EvocationTimer");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_F_REPEAT_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("F_RepeatTimer");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_REPEAT_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("RepeatTimer");
        public static readonly VanillaEntityPropertyMeta<int> PROP_F_REPEAT_COUNT = new VanillaEntityPropertyMeta<int>("FRepeatCount");
        public static readonly VanillaEntityPropertyMeta<int> PROP_REPEAT_COUNT = new VanillaEntityPropertyMeta<int>("RepeatCount");
        public static readonly VanillaEntityPropertyMeta<RandomGenerator> PROP_B_REPEAT_RNG = new VanillaEntityPropertyMeta<RandomGenerator>("BRepeatRNG");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_FEVER = new VanillaEntityPropertyMeta<bool>("Fever");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_PLAY_SOUND = new VanillaEntityPropertyMeta<bool>("PlaySound", true);
    }
}
