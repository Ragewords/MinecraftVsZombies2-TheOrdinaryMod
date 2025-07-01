using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.dispenser)]
    public class Dispenser : DispenserFamily
    {
        public Dispenser(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            InitShootTimer(entity);
            SetRepeatTimer(entity, new FrameTimer(15));
            var evocationTimer = new FrameTimer(120);
            SetEvocationTimer(entity, evocationTimer);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (!entity.IsEvoked())
            {
                ShootTick(entity);
                int repeatCount = GetRepeatCount(entity);
                if (repeatCount > 0)
                {
                    var repeatTimer = GetRepeatTimer(entity);
                    repeatTimer.Run(entity.GetAttackSpeed());
                    if (repeatTimer.Expired)
                    {
                        Shoot(entity);
                        SetRepeatCount(entity, repeatCount - 1);
                        repeatTimer.Reset();
                    }
                }
                return;
            }

            EvokedUpdate(entity);
        }
        public override void OnShootTick(Entity entity)
        {
            int count = 0;
            if (entity.RNG.Next(9) > 3)
                count = 2;
            else
                count = 1;
            SetRepeatCount(entity, count);
            var repeatTimer = GetRepeatTimer(entity);
            repeatTimer.ResetTime(5);
            repeatTimer.Frame = 0;
        }


        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var evocationTimer = GetEvocationTimer(entity);
            evocationTimer.Reset();
            entity.SetEvoked(true);
        }
        public static FrameTimer GetEvocationTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_EVOCATION_TIMER);
        public static void SetEvocationTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_EVOCATION_TIMER, timer);
        private void EvokedUpdate(Entity entity)
        {
            var evocationTimer = GetEvocationTimer(entity);
            evocationTimer.Run();
            if (evocationTimer.PassedInterval(2))
            {
                var projectile = Shoot(entity);
                projectile.Velocity *= 2;
            }
            if (evocationTimer.Expired)
            {
                entity.SetEvoked(false);
                var shootTimer = GetShootTimer(entity);
                shootTimer.Reset();
            }
        }
        public static FrameTimer GetRepeatTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_REPEAT_TIMER);
        public static void SetRepeatTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_REPEAT_TIMER, timer);
        public static int GetRepeatCount(Entity entity) => entity.GetBehaviourField<int>(ID, PROP_REPEAT_COUNT);
        public static void SetRepeatCount(Entity entity, int count) => entity.SetBehaviourField(ID, PROP_REPEAT_COUNT, count);
        private static readonly NamespaceID ID = VanillaContraptionID.dispenser;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_EVOCATION_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("EvocationTimer");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_REPEAT_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("RepeatTimer");
        public static readonly VanillaEntityPropertyMeta<int> PROP_REPEAT_COUNT = new VanillaEntityPropertyMeta<int>("RepeatCount");
    }
}
