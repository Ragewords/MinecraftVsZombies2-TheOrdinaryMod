using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.vortexHopper)]
    public class VortexHopper : ContraptionBehaviour
    {
        public VortexHopper(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskHostile |= EntityCollisionHelper.MASK_ENEMY;
            SetRepeatTimer(entity, new FrameTimer(0));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (entity.State == VanillaEntityStates.VORTEX_HOPPER_SPIN)
            {
                var repeatTimer = GetRepeatTimer(entity);
                var dir = GetDir(entity);
                SetDir(entity, dir + 20);
                repeatTimer.Run();
                if (repeatTimer.Expired)
                {
                    for (int i = 0; i < 30; i++)
                    {
                       var direction = Quaternion.Euler(0, i * 12 + dir, 0) * new Vector3(1f, 0f, 0f) * 12;
                        var projectile = entity.ShootProjectile(VanillaProjectileID.arrow, direction);
                        projectile.SetDamage(entity.GetDamage());
                        projectile.Position = new Vector3(entity.Position.x, 20f, entity.Position.z);
                    }
                    SetRepeatTimer(entity, new FrameTimer(5));
                }
                DragEnemiesNearby(entity);
                var relativeY = entity.GetRelativeY();
                relativeY -= 1;
                entity.SetRelativeY(relativeY);
                if (relativeY <= -48)
                {
                    entity.Remove();
                }
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationBool("Spinning", entity.State == VanillaEntityStates.VORTEX_HOPPER_SPIN);
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            if (!collision.Collider.IsForMain())
                return;
            var hopper = collision.Entity;
            if (hopper.State == VanillaEntityStates.VORTEX_HOPPER_SPIN)
                return;
            var other = collision.Other;
            if (other.Type != EntityTypes.ENEMY)
                return;
            if (hopper.IsAIFrozen())
                return;
            if (!IsValidEnemy(hopper, other))
                return;
            StartSpin(hopper);
            DragEnemy(hopper, other);
        }
        public override bool CanEvoke(Entity entity)
        {
            if (entity.State == VanillaEntityStates.VORTEX_HOPPER_SPIN)
            {
                return false;
            }
            return base.CanEvoke(entity);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.SetEvoked(true);
            entity.AddBuff<VortexHopperEvokedBuff>();
            StartSpin(entity);
        }

        public static bool IsValidEnemy(Entity hopper, Entity target)
        {
            if (target.IsDead || !hopper.IsHostile(target) || !Detection.CanDetect(target))
                return false;
            if (!target.IsInWater())
                return false;
            return true;
        }

        private static void StartSpin(Entity hopper)
        {
            hopper.State = VanillaEntityStates.VORTEX_HOPPER_SPIN;
            hopper.AddBuff<VortexHopperSpinBuff>();
            hopper.PlaySound(VanillaSoundID.vortex);
            DragEnemiesNearby(hopper);

            var pos = hopper.Position;
            pos.y = hopper.GetGroundY();

            Entity vortex = hopper.Level.Spawn(VanillaEffectID.vortex, pos, hopper);
            var vortexScale = hopper.GetRange() / 120;
            vortex.SetScale(Vector3.one * vortexScale);
            vortex.SetDisplayScale(Vector3.one * vortexScale);
        }

        private static void DragEnemiesNearby(Entity hopper)
        {
            foreach (var target in hopper.Level.FindEntities(e => e.Type == EntityTypes.ENEMY && IsValidEnemy(hopper, e) && IsInRange(hopper, e)))
            {
                DragEnemy(hopper, target);
            }
        }
        private static void DragEnemy(Entity hopper, Entity enemy)
        {
            if (enemy.IsDead || enemy.HasBuff<VortexHopperDragBuff>())
                return;
            if (enemy.ImmuneVortex())
                return;
            enemy.Die(new DamageEffectList(VanillaDamageEffects.DROWN), hopper);
            var hopperPos = hopper.Position;
            hopperPos.y = hopper.GetGroundY();
            var hopperPos2D = new Vector2(hopper.Position.x, hopper.Position.z);
            var targetPos2D = new Vector2(enemy.Position.x, enemy.Position.z);
            var buff = enemy.AddBuff<VortexHopperDragBuff>();
            buff.SetProperty(VortexHopperDragBuff.PROP_CENTER, hopperPos);
            buff.SetProperty(VortexHopperDragBuff.PROP_RADIUS, Vector2.Distance(targetPos2D, hopperPos2D));
            buff.SetProperty(VortexHopperDragBuff.PROP_ANGLE, Vector2.SignedAngle(Vector2.right, targetPos2D - hopperPos2D));
        }
        private static bool IsInRange(Entity hopper, Entity target)
        {
            var hopperPos = new Vector2(hopper.Position.x, hopper.Position.z);
            var targetPos = new Vector2(target.Position.x, target.Position.z);
            var distance = Vector2.Distance(hopperPos, targetPos);
            return distance < hopper.GetRange();
        }
        public static FrameTimer GetRepeatTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_REPEAT_TIMER);
        public static void SetRepeatTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_REPEAT_TIMER, timer);
        public static int GetDir(Entity entity) => entity.GetBehaviourField<int>(ID, PROP_DIR);
        public static void SetDir(Entity entity, int timer) => entity.SetBehaviourField(ID, PROP_DIR, timer);

        private static readonly NamespaceID ID = VanillaContraptionID.vortexHopper;
        public const float EVOKED_SPIN_RADIUS = 120;
        public const float SPIN_RADIUS = 40;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_REPEAT_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("RepeatTimer");
        public static readonly VanillaEntityPropertyMeta<int> PROP_DIR = new VanillaEntityPropertyMeta<int>("Dir");
    }
}
