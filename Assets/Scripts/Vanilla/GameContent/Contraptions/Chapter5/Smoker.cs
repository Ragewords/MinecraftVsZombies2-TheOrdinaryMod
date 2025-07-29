using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.smoker)]
    public class Smoker : ContraptionBehaviour
    {
        public Smoker(string nsp, string name) : base(nsp, name)
        {
            smokeDetector = new FireBreathDetector()
            {
                fireBreathID = VanillaEffectID.smokerSmoke
            };
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetSmokeTimer(entity, new FrameTimer(SMOKE_INTERVAL));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            var target = smokeDetector.Detect(entity);
            var timer = GetSmokeTimer(entity);
            timer.Run(entity.GetAttackSpeed());
            if (timer.Expired)
            {
                if (target != null)
                {
                    var param = entity.GetSpawnParams();
                    param.SetProperty(VanillaEntityProps.DAMAGE, entity.GetDamage() * 2);
                    param.SetProperty(EngineEntityProps.FLIP_X, entity.IsFlipX());
                    entity.Spawn(VanillaEffectID.smokerSmoke, entity.GetCenter(), param);
                    timer.ResetTime(SMOKE_INTERVAL);
                }
                else
                {
                    timer.ResetTime(SMOKE_INTERVAL_SHORT);
                }
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            bool frozen = entity.IsAIFrozen();
            entity.SetAnimationBool("Frozen", frozen);
            entity.SetLightSource(!frozen);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.TriggerAnimation("Burst");
            entity.PlaySound(VanillaSoundID.flame);
            for (var i = -1; i <= 1; i++)
            {
                var level = entity.Level;
                for (var j = (entity.GetLane() == 0 ? 0 : -1); j <= (entity.GetLane() == level.GetMaxLaneCount() - 1 ? 0 : 1); j++)
                {
                    var param = entity.GetSpawnParams();
                    param.SetProperty(VanillaEntityProps.DAMAGE, entity.GetDamage() / 3);
                    var gridPos = level.GetEntityGridPosition(entity.GetColumn() + i + 2 * entity.GetFacingX(), entity.GetLane() + j);
                    var fireBlock = entity.Spawn(VanillaEffectID.smokerFire, gridPos, param);
                    fireBlock.Timeout = 360;
                }
            }
        }
        public const int SMOKE_INTERVAL = 60;
        public const int SMOKE_INTERVAL_SHORT = 5;
        public static FrameTimer GetSmokeTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_SMOKE_TIMER);
        public static void SetSmokeTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_SMOKE_TIMER, timer);
        private static readonly NamespaceID ID = VanillaContraptionID.smoker;
        private static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_SMOKE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("SmokeTimer");
        private Detector smokeDetector;
    }
}
