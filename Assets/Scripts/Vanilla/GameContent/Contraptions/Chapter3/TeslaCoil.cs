using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.teslaCoil)]
    public class TeslaCoil : ContraptionBehaviour
    {
        public TeslaCoil(string nsp, string name) : base(nsp, name)
        {
            detector = new TeslaCoilDetector(ATTACK_HEIGHT);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetAttackTimer(entity, new FrameTimer(ATTACK_COOLDOWN));
            SetIsConnectedX(entity, false);
            SetIsConnectedY(entity, false);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.PlaySound(VanillaSoundID.lightningAttack);
            var pos = entity.Position;
            pos.y += 240;
            var cloud = entity.SpawnWithParams(VanillaEffectID.thunderCloud, pos);

            CreateArc(entity, entity.Position + ARC_OFFSET, cloud.GetCenter());
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            var connectx = GetConnectCoilX(entity);
            var connecty = GetConnectCoilY(entity);
            FindCoil(entity);
            if (entity.State == STATE_IDLE)
            {
                var timer = GetAttackTimer(entity);
                timer.Run(entity.GetAttackSpeed());
                if (timer.Expired)
                {
                    if (detector.DetectExists(entity))
                    {
                        entity.State = STATE_ATTACK;
                        timer.ResetTime(ATTACK_CHARGE);
                        entity.PlaySound(VanillaSoundID.teslaPower);
                    }
                    else
                    {
                        timer.Frame = 7;
                    }
                }
            }
            else if (entity.State == STATE_ATTACK)
            {
                var timer = GetAttackTimer(entity);
                timer.Run(entity.GetAttackSpeed());
                if (timer.Expired)
                {
                    var target = detector.DetectEntityWithTheMost(entity, t => GetTargetPriority(entity, t));
                    if (target != null)
                    {
                        var faction = entity.GetFaction();
                        var damage = entity.GetDamage();
                        var sourcePosition = entity.Position + ARC_OFFSET;
                        var targetPosition = target.Position;
                        var groundY = entity.Level.GetGroundY(targetPosition.x, targetPosition.z);
                        if (targetPosition.y <= groundY)
                        {
                            targetPosition.y = groundY;
                        }
                        Shock(entity, damage, faction, SHOCK_RADIUS, targetPosition);
                        entity.Level.Explode(entity.Position, 80, entity.GetFaction(), damage / 4, new DamageEffectList(VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.LIGHTNING), entity);
                        CreateArc(entity, sourcePosition, targetPosition);
                        entity.PlaySound(VanillaSoundID.teslaAttack);
                        entity.Spawn(VanillaEffectID.waterLightningParticles, entity.Position);

                        if (connectx.ExistsAndAlive() && connectx.IsFriendly(entity) && !connectx.IsAIFrozen())
                            CreateArc(entity, sourcePosition, connectx.Position + ARC_OFFSET);

                        if (connecty.ExistsAndAlive() && connecty.IsFriendly(entity) && !connecty.IsAIFrozen())
                            CreateArc(entity, sourcePosition, connecty.Position + ARC_OFFSET);
                    }
                    timer.ResetTime(ATTACK_COOLDOWN);
                    entity.State = STATE_IDLE;
                }
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationBool("Attacking", entity.State == STATE_ATTACK);
            entity.SetAnimationBool("ShowArc", entity.State != STATE_ATTACK && !entity.IsAIFrozen());
            entity.SetAnimationFloat("AttackSpeed", entity.GetAttackSpeed());
        }
        public override void PostRemove(Entity entity)
        {
            base.PostRemove(entity);
            var connectx = GetConnectCoilX(entity);
            var connecty = GetConnectCoilY(entity);
            if (connectx.ExistsAndAlive())
                SetIsConnectedX(connectx, false);
            if (connecty.ExistsAndAlive())
                SetIsConnectedY(connecty, false);
        }
        public override void PostDeath(Entity entity, DeathInfo damageInfo)
        {
            base.PostDeath(entity, damageInfo);
            entity.Level.Explode(entity.Position, 80, entity.GetFaction(), entity.GetDamage() * 4, new DamageEffectList(VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.EXPLOSION), entity);
            var explosion = entity.Level.Spawn(VanillaEffectID.explosion, entity.GetCenter(), entity);
            explosion.SetSize(Vector3.one * 160);
            entity.PlaySound(VanillaSoundID.explosion);
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
        private void FindCoil(Entity entity)
        {
            //寻找同一行的可链接目标。
            foreach (var coilx in entity.Level.GetEntities(EntityTypes.PLANT))
            {
                if (coilx.GetLane() != entity.GetLane())
                    continue;
                else if (coilx.IsHostile(entity))
                    continue;
                else if (coilx.IsAIFrozen())
                    continue;
                else if (coilx.GetDefinitionID() != VanillaContraptionID.teslaCoil)
                    continue;
                else if (coilx.ID == entity.ID)
                    continue;
                else if (GetIsConnectedX(entity))
                    continue;
                else if (GetIsConnectedX(coilx))
                    continue;
                if (coilx != null)
                {
                    SetConnectCoilX(entity, coilx);
                    SetConnectCoilX(coilx, entity);
                    SetIsConnectedX(coilx, true);
                    SetIsConnectedX(entity, true);
                }
            }
            //寻找同一列的可链接目标。
            foreach (var coily in entity.Level.GetEntities(EntityTypes.PLANT))
            {
                if (coily.GetColumn() != entity.GetColumn())
                    continue;
                else if (coily.IsHostile(entity))
                    continue;
                else if (coily.IsAIFrozen())
                    continue;
                else if (coily.GetDefinitionID() != VanillaContraptionID.teslaCoil)
                    continue;
                else if (coily.ID == entity.ID)
                    continue;
                else if (GetIsConnectedY(entity))
                    continue;
                else if (GetIsConnectedY(coily))
                    continue;
                if (coily != null)
                {
                    SetConnectCoilY(entity, coily);
                    SetConnectCoilY(coily, entity);
                    SetIsConnectedY(coily, true);
                    SetIsConnectedY(entity, true);
                }
            }
        }
        public static void Shock(Entity source, float damage, int faction, float shockRadius, Vector3 targetPosition, DamageEffectList damageEffects = null)
        {
            damageEffects = damageEffects ?? new DamageEffectList(VanillaDamageEffects.LIGHTNING, VanillaDamageEffects.MUTE);
            var level = source.Level;
            detectBuffer.Clear();
            detectBuffer_alt.Clear();
            gridDetectBuffer.Clear();
            level.OverlapSphereNonAlloc(targetPosition, shockRadius, faction, EntityCollisionHelper.MASK_VULNERABLE, 0, detectBuffer);
            if (targetPosition.y <= level.GetGroundY(targetPosition.x, targetPosition.z) && level.IsWaterAt(targetPosition.x, targetPosition.z))
            {
                level.GetConnectedWaterGrids(targetPosition, 1, 1, gridDetectBuffer);
                foreach (var grid in gridDetectBuffer)
                {
                    var column = grid.Column;
                    var lane = grid.Lane;
                    Detection.OverlapGridGroundNonAlloc(level, column, lane, faction, EntityCollisionHelper.MASK_VULNERABLE, 0, detectBuffer);
                    var x = level.GetColumnX(column) + level.GetGridWidth() * 0.5f;
                    var z = level.GetLaneZ(lane) + level.GetGridHeight() * 0.5f;
                    var y = level.GetGroundY(x, z);
                    source.Spawn(VanillaEffectID.waterLightningParticles, new Vector3(x, y, z));
                }
            }
            var connectx = GetConnectCoilX(source);
            if (connectx.ExistsAndAlive())
            {
                if (connectx.IsHostile(source) || connectx.IsAIFrozen())
                    return;
                level.GetConnectedLaneGrids(source.Position, connectx.Position, gridDetectBuffer);
                foreach (var grid in gridDetectBuffer)
                {
                    var column = grid.Column;
                    var lane = grid.Lane;
                    Detection.OverlapGridGroundNonAlloc(level, column, lane, faction, EntityCollisionHelper.MASK_VULNERABLE, 0, detectBuffer_alt);
                }
            }
            var connecty = GetConnectCoilY(source);
            if (connecty.ExistsAndAlive())
            {
                if (connecty.IsHostile(source) || connecty.IsAIFrozen())
                    return;
                level.GetConnectedColumnGrids(source.Position, connecty.Position, gridDetectBuffer);
                foreach (var grid in gridDetectBuffer)
                {
                    var column = grid.Column;
                    var lane = grid.Lane;
                    Detection.OverlapGridGroundNonAlloc(level, column, lane, faction, EntityCollisionHelper.MASK_VULNERABLE, 0, detectBuffer_alt);
                }
            }
            foreach (var collider in detectBuffer)
            {
                collider.TakeDamage(damage, damageEffects, source);
                var targetBuff = collider.Entity.GetFirstBuff<ElectricArcBuff>();
                if (targetBuff == null)
                {
                    targetBuff = collider.Entity.AddBuff<ElectricArcBuff>();
                }
                targetBuff.SetProperty(ElectricArcBuff.PROP_ZAP_TIME, 5);
            }
            foreach (var collider in detectBuffer_alt)
            {
                collider.TakeDamage(damage / 16, damageEffects, source);
            }
        }
        public static void CreateArc(Entity source, Vector3 sourcePosition, Vector3 targetPosition)
        {
            var arc = source.Spawn(VanillaEffectID.electricArc, sourcePosition);
            ElectricArc.Connect(arc, targetPosition);
            ElectricArc.SetPointCount(arc, 20);
            ElectricArc.UpdateArc(arc);
            arc.Timeout = 30;
        }
        public static FrameTimer GetAttackTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_ATTACK_TIMER);
        public static void SetAttackTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_ATTACK_TIMER, timer);
        public static Entity GetConnectCoilX(Entity entity)
        {
            var entityID = entity.GetBehaviourField<EntityID>(ID, CONNECT_COIL_X);
            return entityID?.GetEntity(entity.Level);
        }
        public static void SetConnectCoilX(Entity entity, Entity value)
        {
            entity.SetBehaviourField(ID, CONNECT_COIL_X, new EntityID(value));
        }
        public static Entity GetConnectCoilY(Entity entity)
        {
            var entityID = entity.GetBehaviourField<EntityID>(ID, CONNECT_COIL_Y);
            return entityID?.GetEntity(entity.Level);
        }
        public static void SetConnectCoilY(Entity entity, Entity value)
        {
            entity.SetBehaviourField(ID, CONNECT_COIL_Y, new EntityID(value));
        }
        public static bool GetIsConnectedX(Entity entity)
        {
            return entity.GetBehaviourField<bool>(ID, CONNECTED_X);
        }
        public static void SetIsConnectedX(Entity entity, bool value)
        {
            entity.SetBehaviourField(ID, CONNECTED_X, value);
        }
        public static bool GetIsConnectedY(Entity entity)
        {
            return entity.GetBehaviourField<bool>(ID, CONNECTED_Y);
        }
        public static void SetIsConnectedY(Entity entity, bool value)
        {
            entity.SetBehaviourField(ID, CONNECTED_Y, value);
        }

        public const int ATTACK_COOLDOWN = 65;
        public const int ATTACK_CHARGE = 25;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_ATTACK_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("AttackTimer");
        public static readonly VanillaEntityPropertyMeta<EntityID> CONNECT_COIL_X = new VanillaEntityPropertyMeta<EntityID>("ConnecteCoilX");
        public static readonly VanillaEntityPropertyMeta<EntityID> CONNECT_COIL_Y = new VanillaEntityPropertyMeta<EntityID>("ConnecteCoilY");
        public static readonly VanillaEntityPropertyMeta<bool> CONNECTED_X = new VanillaEntityPropertyMeta<bool>("ConnectedX");
        public static readonly VanillaEntityPropertyMeta<bool> CONNECTED_Y = new VanillaEntityPropertyMeta<bool>("ConnectedY");
        public const float ATTACK_HEIGHT = 160;
        public static readonly Vector3 ARC_OFFSET = new Vector3(0, 96, 0);
        public const float SHOCK_RADIUS = 20;

        public const int STATE_IDLE = VanillaEntityStates.TESLA_COIL_IDLE;
        public const int STATE_ATTACK = VanillaEntityStates.TESLA_COIL_ATTACK;

        private Detector detector;
        private static List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
        private static List<IEntityCollider> detectBuffer_alt = new List<IEntityCollider>();
        private static HashSet<LawnGrid> gridDetectBuffer = new HashSet<LawnGrid>();
        private static readonly NamespaceID ID = VanillaContraptionID.teslaCoil;
    }
}
