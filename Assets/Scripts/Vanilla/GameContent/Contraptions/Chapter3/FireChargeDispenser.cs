using MVZ2.GameContent.Detections;
using MVZ2.GameContent.HeldItems;
using MVZ2.HeldItems;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.HeldItems;
using MVZ2Logic;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;
using MVZ2.GameContent.Models;
using PVZEngine.SeedPacks;
using MVZ2.Vanilla.Level;
using static UnityEngine.EventSystems.EventTrigger;
using MVZ2.GameContent.Projectiles;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.fireChargeDispenser)]
    public class FireChargeDispenser : ContraptionBehaviour, IHeldEntityBehaviour
    {
        public FireChargeDispenser(string nsp, string name) : base(nsp, name)
        {
            detector = new TeslaCoilDetector(ATTACK_HEIGHT);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetAttackTimer(entity, new FrameTimer(ATTACK_COOLDOWN));
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var shootTimer = GetAttackTimer(entity);
            shootTimer.ResetTime(ATTACK_COOLDOWN * 2);
            entity.SetEvoked(true);
            SetMissleTargetLocked(entity, false);
            entity.PlaySound(VanillaSoundID.fuse);
            entity.Level.SetHeldItem(VanillaHeldTypes.entity, entity.ID, 100, true);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            var shootTimer = GetAttackTimer(entity);
            shootTimer.Run(entity.GetAttackSpeed());
            if (shootTimer.Expired)
            {
                var target = detector.DetectEntityWithTheMost(entity, t => GetTargetPriority(entity, t));
                if (target != null)
                {
                    var targetPos = target.Position;
                    var velocity = VanillaProjectileExt.GetLobVelocityByTime(entity.GetShootPoint(), targetPos, 30, GRAVITY);
                    Shoot(entity, entity.GetProjectileID(), entity.GetDamage(), entity.GetShootSound(), velocity);
                }
                shootTimer.ResetTime(ATTACK_COOLDOWN);
            }
            if (entity.IsEvoked())
            {
                EvokedUpdate(entity);
            }
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
        public void Shoot(Entity entity, NamespaceID id, float damage, NamespaceID sound, Vector3 velocity)
        {
            entity.TriggerAnimation("Shoot");
            var projectile = entity.ShootProjectile(new ShootParams()
            {
                projectileID = id,
                position = entity.GetShootPoint(),
                faction = entity.GetFaction(),
                damage = damage,
                soundID = sound,
                velocity = velocity,
            });
            projectile.SetGravity(GRAVITY);
        }
        private void EvokedUpdate(Entity pad)
        {
            // 发射导弹。
            bool locked = IsMissleTargetLocked(pad);
            Vector3 targetPosition = GetMissleTarget(pad);

            // 发射倒计时。
            var shootTimer = GetAttackTimer(pad);
            shootTimer.Run();

            // 倒计时结束，或者没有在手持该器械
            // 结束大招。
            bool holdingThis = pad.Level.IsHoldingEntity(pad);
            if (shootTimer.Expired || (!holdingThis && !locked))
            {
                if (locked)
                {
                    var velocity = VanillaProjectileExt.GetLobVelocityByTime(pad.GetShootPoint(), targetPosition, 30, GRAVITY);
                    Shoot(pad, VanillaProjectileID.missile, pad.GetDamage() * 10, VanillaSoundID.missile, velocity);
                }
                if (holdingThis)
                {
                    pad.Level.ResetHeldItem();
                }
                pad.SetEvoked(false);

                shootTimer.ResetTime(ATTACK_COOLDOWN / 3);
                SetMissleTimeout(pad, 0);
                SetMissleTarget(pad, Vector3.zero);
            }
        }
        public static FrameTimer GetAttackTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_ATTACK_TIMER);
        public static void SetAttackTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_ATTACK_TIMER, timer);
        public static Vector3 GetMissleTarget(Entity pad) => pad.GetBehaviourField<Vector3>(ID, PROP_MISSLE_TARGET);
        public static void SetMissleTarget(Entity pad, Vector3 position) => pad.SetBehaviourField(ID, PROP_MISSLE_TARGET, position);
        public static int GetMissleTimeout(Entity pad) => pad.GetBehaviourField<int>(ID, PROP_MISSLE_TIMEOUT);
        public static void SetMissleTimeout(Entity pad, int value) => pad.SetBehaviourField(ID, PROP_MISSLE_TIMEOUT, value);
        public static bool IsMissleTargetLocked(Entity pad) => pad.GetBehaviourField<bool>(ID, PROP_MISSLE_TARGET_LOCKED);
        public static void SetMissleTargetLocked(Entity pad, bool value) => pad.SetBehaviourField(ID, PROP_MISSLE_TARGET_LOCKED, value);

        public const int ATTACK_COOLDOWN = 90;
        public const int GRAVITY = 1;
        public const float ATTACK_HEIGHT = 160;
        public static readonly VanillaEntityPropertyMeta PROP_ATTACK_TIMER = new VanillaEntityPropertyMeta("AttackTimer");
        public static readonly VanillaEntityPropertyMeta PROP_MISSLE_TARGET = new VanillaEntityPropertyMeta("MissleTarget");
        public static readonly VanillaEntityPropertyMeta PROP_MISSLE_TIMEOUT = new VanillaEntityPropertyMeta("MissleTimeout");
        public static readonly VanillaEntityPropertyMeta PROP_MISSLE_TARGET_LOCKED = new VanillaEntityPropertyMeta("MissleTargetLocked");

        private Detector detector;
        private static readonly NamespaceID ID = VanillaContraptionID.fireChargeDispenser;

        bool IHeldEntityBehaviour.CheckRaycast(Entity entity, HeldItemTarget target, IHeldItemData data)
        {
            return target is HeldItemTargetGrid targetGrid;
        }

        HeldHighlight IHeldEntityBehaviour.GetHighlight(Entity entity, HeldItemTarget target, IHeldItemData data)
        {
            return HeldHighlight.Green;
        }

        void IHeldEntityBehaviour.Use(Entity entity, HeldItemTarget target, IHeldItemData data, PointerInteraction interaction)
        {
            var targetPhase = Global.IsMobile() ? PointerInteraction.Release : PointerInteraction.Press;
            if (interaction != targetPhase)
                return;
            if (target is not HeldItemTargetGrid targetGrid)
                return;
            if (targetGrid.Target == null)
                return;
            var level = target.GetLevel();
            SetMissleTarget(entity, targetGrid.Target.GetEntityPosition());
            SetMissleTargetLocked(entity, true);
            var shootTimer = GetAttackTimer(entity);
            shootTimer.ResetTime(30);
            level.ResetHeldItem();
            entity.PlaySound(VanillaSoundID.parabotTick);
        }
        SeedPack IHeldEntityBehaviour.GetSeedPack(Entity entity, LevelEngine level, IHeldItemData data)
        {
            return null;
        }
        NamespaceID IHeldEntityBehaviour.GetModelID(Entity entity, LevelEngine level, IHeldItemData data)
        {
            return VanillaModelID.targetHeldItem;
        }

        float IHeldEntityBehaviour.GetRadius(Entity entity, LevelEngine level, IHeldItemData data)
        {
            return 0;
        }

        void IHeldEntityBehaviour.Update(Entity entity, LevelEngine level, IHeldItemData data)
        {
            if (entity == null || !entity.Exists() || entity.IsAIFrozen())
            {
                level.ResetHeldItem();
                return;
            }
        }
    }
}


