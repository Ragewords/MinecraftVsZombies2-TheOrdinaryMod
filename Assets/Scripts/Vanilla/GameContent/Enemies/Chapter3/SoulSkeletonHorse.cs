using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.soulSkeletonHorse)]
    public class SoulSkeletonHorse : SkeletonHorse
    {
        public SoulSkeletonHorse(string nsp, string name) : base(nsp, name)
        {
            blocksJumpDetector = new SkeletonHorseBlocksJumpDetector();
        }

        #region 回调
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetDoubleJumpTimer(entity, new FrameTimer(15));
        }
        public override void PostContactGround(Entity entity, Vector3 velocity)
        {
            base.PostContactGround(entity, velocity);
            var DJumpTimer = GetDoubleJumpTimer(entity);
            DJumpTimer.ResetTime(15);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (entity.State == STATE_JUMP)
            {
                if (!blocksJumpDetector.DetectExists(entity))
                {
                    var DJumpTimer = GetDoubleJumpTimer(entity);
                    DJumpTimer.Run();
                    if (DJumpTimer.Expired)
                    {
                        var vel = entity.Velocity;
                        vel.x = entity.GetFacingX() * 5;
                        vel.y = 12;
                        entity.Velocity = vel;
                        entity.PlaySound(VanillaSoundID.horseGallop);
                        DJumpTimer.ResetTime(45);
                    }
                }
            }
        }
        private Detector blocksJumpDetector;
        private static readonly NamespaceID ID = VanillaEnemyID.soulSkeletonHorse;
        public static FrameTimer GetDoubleJumpTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, FIELD_DOUBLE_JUMP_TIMER);
        public static void SetDoubleJumpTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(ID, FIELD_DOUBLE_JUMP_TIMER, value);
        public static readonly VanillaEntityPropertyMeta<FrameTimer> FIELD_DOUBLE_JUMP_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("DoubleJumpTimer");
        #endregion
    }
}
