using System.Linq;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.fireChargeDispenser)]
    public class FireChargeDispenser : ContraptionBehaviour
    {
        public FireChargeDispenser(string nsp, string name) : base(nsp, name)
        {
            detector = new TeslaCoilDetector(ATTACK_HEIGHT);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetAttackTimer(entity, new FrameTimer(ATTACK_COOLDOWN));
            SetEvocationTimer(entity, new FrameTimer(EVOKATION_TIMER));
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var evokeTimer = GetEvocationTimer(entity);
            evokeTimer.Reset();
            entity.SetEvoked(true);
            entity.PlaySound(VanillaSoundID.fuse);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            var shootTimer = GetAttackTimer(entity);
            if (!entity.IsEvoked())
            {
                shootTimer.Run(entity.GetAttackSpeed());
                if (shootTimer.Expired)
                {
                    var target = detector.DetectEntityWithTheMost(entity, t => GetTargetPriority(entity, t));
                    if (target != null)
                    {
                        var targetPos = target.Position;
                        var velocity = VanillaProjectileExt.GetLobVelocityByTime(entity.GetShootPoint(), targetPos, 30, GRAVITY);
                        Shoot(entity, entity.GetProjectileID(), entity.GetDamage(), velocity);
                    }
                    shootTimer.ResetTime(ATTACK_COOLDOWN);
                }
                return;
            }

            EvokedUpdate(entity);
        }
        private float GetTargetPriority(Entity self, Entity target)
        {
            var target2Self = target.Position - self.Position;
            target2Self.y = 0;
            var distance = target2Self.magnitude;
            var priority = -distance;
            if (target.Position.y > self.Position.y + 40)
            {
                priority += 300;
            }
            return priority;
        }
        public void Shoot(Entity entity, NamespaceID id, float damage, Vector3 velocity)
        {
            entity.TriggerAnimation("Shoot");
            var projectile = entity.ShootProjectile(new ShootParams()
            {
                projectileID = id,
                position = entity.GetShootPoint(),
                faction = entity.GetFaction(),
                damage = damage,
                soundID = entity.GetShootSound(),
                velocity = velocity,
            });
            projectile.SetGravity(GRAVITY);
        }
        private void EvokedUpdate(Entity entity)
        {
            var evokeTimer = GetEvocationTimer(entity);
            evokeTimer.Run();
            var level = entity.Level;
            var grid = level.GetAllGrids().Random(entity.RNG);
            var targets = entity.Level.FindEntities(e => IsEvocationTarget(entity, e)).RandomTake(1, entity.RNG);
            var targetPos = grid.GetEntityPosition();
            foreach (var target in targets)
            {
                var targetGrid = target.GetGrid();
                if (targetGrid != null)
                    targetPos = targetGrid.GetEntityPosition();
            }
            var velocity = VanillaProjectileExt.GetLobVelocityByTime(entity.GetShootPoint(), targetPos, 60, GRAVITY);

            if (evokeTimer.PassedInterval(10))
            {
                Shoot(entity, VanillaProjectileID.missile, entity.GetDamage() * 3, velocity);
            }

            if (evokeTimer.PassedInterval(3))
            {
                Shoot(entity, entity.GetProjectileID(), entity.GetDamage(), velocity);
            }

            if (evokeTimer.PassedInterval(24))
            {
                Shoot(entity, VanillaProjectileID.flyingTNT, entity.GetDamage() * 5, velocity);
            }

            if (evokeTimer.Expired)
            {
                entity.SetEvoked(false);
            }
        }
        private static bool IsEvocationTarget(Entity self, Entity target)
        {
            if (target == null)
                return false;
            if (target.IsDead)
                return false;
            if (!target.IsVulnerableEntity())
                return false;
            if (!self.IsHostile(target))
                return false;
            if (!Detection.CanDetect(target))
                return false;
            return true;
        }
        public static FrameTimer GetAttackTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_ATTACK_TIMER);
        public static void SetAttackTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_ATTACK_TIMER, timer);
        public static FrameTimer GetEvocationTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_MISSLE_TIMEOUT);
        public static void SetEvocationTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_MISSLE_TIMEOUT, timer);

        public const int ATTACK_COOLDOWN = 60;
        public const int EVOKATION_TIMER = 120;
        public const int GRAVITY = 1;
        public const float ATTACK_HEIGHT = 160;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_ATTACK_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("AttackTimer");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_MISSLE_TIMEOUT = new VanillaEntityPropertyMeta<FrameTimer>("MissleTimeout");

        private Detector detector;
        private static readonly NamespaceID ID = VanillaContraptionID.fireChargeDispenser;
    }
}


