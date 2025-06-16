using System.Collections.Generic;
using MVZ2.GameContent.Buffs;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    public partial class Wither
    {
        #region 状态机
        private class WitherStateMachine : EntityStateMachine
        {
            public WitherStateMachine()
            {
                AddState(new AppearState());
                AddState(new IdleState());
                AddState(new ChargeState());
                AddState(new EatState());
                AddState(new MagicState());
                AddState(new SwitchState());
                AddState(new SummonState());
                AddState(new StunState());
                AddState(new DeathState());
            }
        }
        #endregion

        #region 状态
        private class AppearState : EntityStateMachineState
        {
            public AppearState() : base(STATE_APPEAR) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                entity.SetAnimationBool("Shaking", true);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.ResetTime(30);
                SetHeadOpen(entity, 1);
            }
            public override void OnExit(EntityStateMachine machine, Entity entity)
            {
                base.OnExit(machine, entity);
                entity.SetAnimationBool("Shaking", false);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                SetHeadOpen(entity, 1);

                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.Run(stateMachine.GetSpeed(entity));
                if (!substateTimer.Expired)
                    return;
                stateMachine.StartState(entity, STATE_IDLE);
            }
        }
        private class IdleState : EntityStateMachineState
        {
            public List<Entity> searchTargetBuffer = new List<Entity>();
            public Detector searchTargetDetector = new WitherDetector(WitherDetector.MODE_SKULL);
            public const int MAX_SKULL_CHARGE_MAIN = 90;
            public const int MAX_SKULL_CHARGE = 60;
            public IdleState() : base(STATE_IDLE) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(300);
            }
            public override void OnExit(EntityStateMachine machine, Entity entity)
            {
                base.OnExit(machine, entity);
                EntityID[] headTargets = GetHeadTargets(entity);
                if (headTargets == null)
                    return;
                for (int i = 0; i < headTargets.Length; i++)
                {
                    headTargets[i] = null;
                }
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                if (GetPhase(entity) == PHASE_1 && entity.Health <= entity.GetMaxHealth() / 2)
                {
                    SetPhase(entity, PHASE_2);
                    stateMachine.StartState(entity, STATE_SWITCH);
                    return;
                }
                else if (GetPhase(entity) == PHASE_2 && entity.Health > entity.GetMaxHealth() / 2)
                {
                    SetPhase(entity, PHASE_1);
                    return;
                }
                UpdateAction(stateMachine, entity);
                UpdateStateSwitch(stateMachine, entity);
            }
            private void UpdateAction(EntityStateMachine stateMachine, Entity entity)
            {
                var headOpen = GetHeadOpen(entity);
                headOpen *= 0.5f;
                SetHeadOpen(entity, headOpen);

                UpdateArmored(entity);
                UpdateHeads(entity);
            }
            private void UpdateArmored(Entity entity)
            {
                entity.SetRelativeY(Mathf.Max(0, entity.GetRelativeY() - 1));

                Vector2 targetPos = new Vector2(entity.Position.x, entity.Position.z);
                if (!HasArmor(entity))
                {
                    if (entity.GetRelativeY() < FLY_HEIGHT)
                    {
                        var pos = entity.Position;
                        pos.y += 7;
                        entity.Position = pos;

                        var vel = entity.Velocity;
                        vel.y = Mathf.Max(0, vel.y);
                        entity.Velocity = vel;
                    }
                    //主头负责移动
                    EntityID[] headTargets = GetHeadTargets(entity);
                    if (headTargets != null && headTargets.Length > 0)
                    {
                        var target = headTargets[0]?.GetEntity(entity.Level);
                        if (target.ExistsAndAlive())
                        {
                            var level = entity.Level;
                            var targetColumn = level.GetMaxColumnCount() - 2;
                            targetPos = new Vector2(level.GetEntityColumnX(targetColumn), target.Position.z);
                        }
                    }
                }
                else
                {
                    var level = entity.Level;
                    var targetColumn = level.GetMaxColumnCount() - 2;
                    var targetLane = level.GetMaxLaneCount() / 2;
                    targetPos = new Vector2(level.GetEntityColumnX(targetColumn), level.GetEntityLaneZ(targetLane));
                }
                var thisPos = new Vector2(entity.Position.x, entity.Position.z);
                var dir = targetPos - thisPos;
                if (dir.magnitude > 20)
                {
                    var moveVel = dir.normalized * 20;
                    var pos = entity.Position;
                    pos.x += moveVel.x;
                    pos.z += moveVel.y;
                    entity.Position = pos;
                }
            }
            private void UpdateHeads(Entity entity)
            {
                EntityID[] headTargets = GetHeadTargets(entity);
                if (headTargets == null)
                {
                    headTargets = new EntityID[HEAD_COUNT];
                    SetHeadTargets(entity, headTargets);
                }
                for (var head = 0; head < headTargets.Length; head++)
                {
                    //锁定目标
                    var target = headTargets[head]?.GetEntity(entity.Level);
                    if (!target.ExistsAndAlive())
                    {
                        //搜寻物体，看是否有合格的敌人
                        searchTargetBuffer.Clear();
                        searchTargetDetector.DetectEntities(entity, searchTargetBuffer);
                        //若拥有合格的敌人，则随机选取一个
                        if (searchTargetBuffer.Count > 0)
                        {
                            target = searchTargetBuffer.Random(entity.RNG);
                            headTargets[head] = new EntityID(target);
                        }
                    }
                    else
                    {
                        //判断目标是否合格，否则取消锁定
                        if (!searchTargetDetector.ValidateTarget(entity, target))
                        {
                            target = null;
                            headTargets[head] = null;
                        }
                    }
                    //锁定完毕，开始执行
                    if (target.ExistsAndAlive())
                    {
                        //发射凋灵之首
                        FireSkullUpdate(entity, head, target);
                    }
                }
            }
            private void FireSkullUpdate(Entity entity, int head, Entity target)
            {
                float[] skullCharges = GetSkullCharges(entity);
                if (skullCharges == null)
                {
                    skullCharges = new float[HEAD_COUNT];
                    SetSkullCharges(entity, skullCharges);
                }
                var maxCharges = head == 0 ? MAX_SKULL_CHARGE_MAIN : MAX_SKULL_CHARGE;
                skullCharges[head] += entity.GetAttackSpeed();

                while (skullCharges[head] >= maxCharges)
                {
                    var headPosition = GetHeadPosition(entity, head);
                    var param = entity.GetShootParams();
                    param.position = headPosition;
                    param.projectileID = VanillaProjectileID.witherSkull;
                    param.damage = entity.GetDamage() * 0.75f;
                    param.soundID = VanillaSoundID.witherShoot;
                    param.velocity = 9 * (target.GetCenter() - headPosition).normalized;
                    entity.ShootProjectile(param);
                    skullCharges[head] -= maxCharges;
                }
            }
            private void UpdateStateSwitch(EntityStateMachine stateMachine, Entity entity)
            {
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.Run(stateMachine.GetSpeed(entity));
                if (!stateTimer.Expired)
                    return;
                var nextState = GetNextState(stateMachine, entity);
                stateMachine.StartState(entity, nextState);
                stateMachine.SetPreviousState(entity, nextState);
            }
            private int GetNextState(EntityStateMachine stateMachine, Entity entity)
            {
                var lastState = stateMachine.GetPreviousState(entity);
                if (lastState == STATE_IDLE || lastState == STATE_MAGIC)
                {
                    lastState = STATE_SUMMON;
                    if (GetPhase(entity) == PHASE_2)
                    {
                        return lastState;
                    }
                }
                if (lastState == STATE_SUMMON)
                {
                    lastState = STATE_CHARGE;
                    if (CanCharge(entity))
                    {
                        return lastState;
                    }
                }
                if (lastState == STATE_CHARGE)
                {
                    lastState = STATE_EAT;
                    if (CanEat(entity))
                    {
                        return lastState;
                    }
                }
                if (lastState == STATE_EAT)
                {
                    lastState = STATE_MAGIC;
                    return lastState;
                }

                return STATE_IDLE;
            }
        }
        private class ChargeState : EntityStateMachineState
        {
            public ChargeState() : base(STATE_CHARGE)
            {
            }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.ResetTime(15);

                var lane = FindChargeLane(entity);
                if (lane < 0)
                {
                    lane = entity.GetLane();
                }
                SetTargetLane(entity, lane);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var substate = stateMachine.GetSubState(entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.Run(stateMachine.GetSpeed(entity));

                var level = entity.Level;
                switch (substate)
                {
                    case SUBSTATE_MOVE:
                        {
                            //移动
                            var column = entity.IsFacingLeft() ? level.GetMaxColumnCount() - 1 : 0;
                            var targetX = level.GetEntityColumnX(column);
                            var targetZ = level.GetEntityLaneZ(GetTargetLane(entity));
                            var targetY = level.GetGroundY(targetX, targetZ);

                            var pos = entity.Position;
                            pos.x = pos.x * 0.85f + targetX * 0.15f;
                            pos.y = pos.y * 0.85f + targetY * 0.15f;
                            pos.z = pos.z * 0.85f + targetZ * 0.15f;
                            entity.Position = pos;

                            if (substateTimer.Expired)
                            {
                                substateTimer.ResetTime(90);
                                stateMachine.SetSubState(entity, SUBSTATE_CHARGING);
                            }
                        }
                        break;

                    case SUBSTATE_CHARGING:
                        //聚能
                        //张嘴
                        var headOpen = GetHeadOpen(entity);
                        headOpen = headOpen * 0.7f + 1 * 0.3f;
                        SetHeadOpen(entity, headOpen);

                        if (substateTimer.Expired)
                        {
                            var vel = entity.Velocity;
                            vel.x = entity.GetFacingX() * -20;
                            entity.Velocity = vel;
                            stateMachine.SetSubState(entity, SUBSTATE_DASH);
                            substateTimer.ResetTime(20);
                        }
                        break;
                    case SUBSTATE_DASH:
                        {
                            var pos = entity.Position;
                            var vel = entity.Velocity;
                            vel.x += entity.GetFacingX() * (substateTimer.MaxFrame - substateTimer.Frame) * 0.5f;

                            bool reachEnd = false;
                            float endX = 0;
                            if (entity.IsFacingLeft())
                            {
                                var column = 0;
                                endX = level.GetEntityColumnX(column);
                                reachEnd = pos.x + vel.x <= endX;
                            }
                            else
                            {
                                var column = level.GetMaxColumnCount() - 1;
                                endX = level.GetEntityColumnX(column);
                                reachEnd = pos.x + vel.x >= endX;
                            }
                            if (reachEnd)
                            {
                                pos.x = endX;

                                vel.x = 0;
                                stateMachine.SetSubState(entity, SUBSTATE_DASH_END);
                                substateTimer.ResetTime(30);
                                var column = level.GetMaxColumnCount();
                                var spawnX = level.GetEntityColumnX(column);
                                var lane = entity.GetLane();
                                var spawnZ = level.GetEntityLaneZ(lane);
                                var chariot = entity.Level.Spawn(VanillaEnemyID.hellChariot, new Vector3(spawnX, 0, spawnZ), entity);
                                var exp = chariot.Spawn(VanillaEffectID.explosion, chariot.GetCenter());
                                exp.SetSize(Vector3.one * 180);
                            }
                            entity.Position = pos;
                            entity.Velocity = vel;
                        }
                        break;
                    case SUBSTATE_DASH_END:
                        if (substateTimer.Expired)
                        {
                            stateMachine.StartState(entity, STATE_IDLE);
                        }
                        break;
                }
            }
            public const int SUBSTATE_MOVE = 0;
            public const int SUBSTATE_CHARGING = 1;
            public const int SUBSTATE_DASH = 2;
            public const int SUBSTATE_DASH_END = 3;
        }
        private class EatState : EntityStateMachineState
        {
            public EatState() : base(STATE_EAT) { }

            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.ResetTime(15);

                var lane = FindEatLane(entity);
                if (lane < 0)
                {
                    lane = entity.GetLane();
                }
                SetTargetLane(entity, lane);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var substate = stateMachine.GetSubState(entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.Run(stateMachine.GetSpeed(entity));

                var level = entity.Level;
                switch (substate)
                {
                    case SUBSTATE_MOVE:
                        {
                            //移动
                            var column = entity.IsFacingLeft() ? level.GetMaxColumnCount() - 1 : 0;
                            var targetX = level.GetEntityColumnX(column);
                            var targetZ = level.GetEntityLaneZ(GetTargetLane(entity));
                            var targetY = level.GetGroundY(targetX, targetZ);

                            var pos = entity.Position;
                            pos.x = pos.x * 0.85f + targetX * 0.15f;
                            pos.y = pos.y * 0.85f + targetY * 0.15f;
                            pos.z = pos.z * 0.85f + targetZ * 0.15f;
                            entity.Position = pos;

                            if (substateTimer.Expired)
                            {
                                substateTimer.ResetTime(30);
                                stateMachine.SetSubState(entity, SUBSTATE_READY);
                            }
                        }
                        break;

                    case SUBSTATE_READY:
                        //聚能
                        //张嘴
                        var progress = substateTimer.MaxFrame - substateTimer.Frame;
                        if (progress < 16)
                        {
                            var headOpen = GetHeadOpen(entity);
                            if (progress < 4)
                            {
                                headOpen = Mathf.Lerp(0, 1, progress / 4f);
                            }
                            else if (progress < 8)
                            {
                                headOpen = Mathf.Lerp(1, 0, (progress - 4) / 4f);
                            }
                            else if (progress < 12)
                            {
                                headOpen = Mathf.Lerp(0, 1, (progress - 8) / 4f);
                            }
                            else
                            {
                                headOpen = Mathf.Lerp(1, 0, (progress - 12) / 4f);
                            }
                            SetHeadOpen(entity, headOpen);
                        }
                        if (substateTimer.PassedFrame(substateTimer.MaxFrame - 8) || substateTimer.PassedFrame(substateTimer.MaxFrame - 16))
                        {
                            entity.PlaySound(VanillaSoundID.shieldHit, 0.5f, 2);
                        }

                        if (substateTimer.Expired)
                        {
                            stateMachine.SetSubState(entity, SUBSTATE_DASH);
                            substateTimer.ResetTime(20);
                        }
                        break;
                    case SUBSTATE_DASH:
                        {
                            var headOpen = GetHeadOpen(entity);
                            headOpen = headOpen * 0.7f + 1 * 0.3f;
                            SetHeadOpen(entity, headOpen);

                            var pos = entity.Position;
                            var vel = entity.Velocity;
                            vel.x += entity.GetFacingX() * (substateTimer.MaxFrame - substateTimer.Frame) * 0.5f;
                            entity.Velocity = vel;
                            if (substateTimer.Expired)
                            {
                                FinishEat(entity);
                            }
                        }
                        break;
                    case SUBSTATE_EATEN:
                        {
                            var headOpen = GetHeadOpen(entity);
                            headOpen = headOpen * 0.7f;
                            SetHeadOpen(entity, headOpen);

                            var pos = entity.Position;
                            var vel = entity.Velocity;
                            vel.x -= entity.GetFacingX() * (substateTimer.MaxFrame - substateTimer.Frame) * 0.5f;

                            bool reachEnd = false;
                            float endX = 0;
                            if (entity.IsFacingLeft())
                            {
                                var column = level.GetMaxColumnCount() - 1;
                                endX = level.GetEntityColumnX(column);
                                reachEnd = pos.x + vel.x >= endX;
                            }
                            else
                            {
                                var column = 0;
                                endX = level.GetEntityColumnX(column);
                                reachEnd = pos.x + vel.x <= endX;
                            }
                            if (reachEnd)
                            {
                                pos.x = endX;
                                vel.x = 0;
                                stateMachine.StartState(entity, STATE_IDLE);
                            }
                            entity.Position = pos;
                            entity.Velocity = vel;
                        }
                        break;
                }
            }

            public const int SUBSTATE_MOVE = 0;
            public const int SUBSTATE_READY = 1;
            public const int SUBSTATE_DASH = 2;
            public const int SUBSTATE_EATEN = 3;
        }
        private class MagicState : EntityStateMachineState
        {
            public MagicState() : base(STATE_MAGIC) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.ResetTime(15);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                int magic = GetMagicType(entity);
                var substate = stateMachine.GetSubState(entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.Run(stateMachine.GetSpeed(entity));

                var level = entity.Level;
                switch (substate)
                {
                    case SUBSTATE_MOVE:
                        {
                            //移动
                            var column = entity.IsFacingLeft() ? level.GetMaxColumnCount() - 3 : 2;
                            var lane = level.GetMaxLaneCount() / 2;
                            var targetX = level.GetEntityColumnX(column);
                            var targetZ = level.GetEntityLaneZ(lane);
                            var targetY = level.GetGroundY(targetX, targetZ);

                            var pos = entity.Position;
                            pos.x = pos.x * 0.85f + targetX * 0.15f;
                            pos.y = pos.y * 0.85f + targetY * 0.15f;
                            pos.z = pos.z * 0.85f + targetZ * 0.15f;
                            entity.Position = pos;

                            if (substateTimer.Expired)
                            {
                                substateTimer.ResetTime(45);
                                stateMachine.SetSubState(entity, SUBSTATE_CAST);
                                entity.PlaySound(VanillaSoundID.witherMagicCast);
                                entity.SetAnimationBool("Shaking", true);
                                entity.SetAnimationInt("LightColor", magic);
                            }
                        }
                        break;

                    case SUBSTATE_CAST:
                        {
                            //张嘴
                            var headOpen = GetHeadOpen(entity);
                            headOpen = headOpen * 0.7f + 1 * 0.3f;
                            SetHeadOpen(entity, headOpen);

                            if (substateTimer.Expired)
                            {
                                entity.PlaySound(VanillaSoundID.witherMagicEnd);
                                switch (magic)
                                {
                                    case MAGIC_MESMERIZER:
                                        {
                                            var targets = entity.Level.FindEntities(e => e.IsHostile(entity) && e.Type == EntityTypes.PLANT && e.GetDefinitionID() != VanillaContraptionID.glowstone && !e.IsCharmed()).RandomTake(10, entity.RNG);
                                            foreach (var target in targets)
                                            {
                                                target.Charm(entity.GetFaction());
                                            }
                                            SetMagicType(entity, MAGIC_BERSERKER);
                                        }
                                        break;
                                    case MAGIC_BERSERKER:
                                        {
                                            var targets = entity.Level.FindEntities(e => e.IsHostile(entity) && e.Type == EntityTypes.PLANT && e.GetDefinitionID() != VanillaContraptionID.glowstone && !e.IsCharmed()).RandomTake(2, entity.RNG);
                                            foreach (var target in targets)
                                            {
                                                var buff = target.GetFirstBuff<AboutToExplodeBuff>();
                                                if (buff == null)
                                                {
                                                    buff = target.AddBuff<AboutToExplodeBuff>();
                                                }
                                                buff.SetProperty(AboutToExplodeBuff.PROP_FACTION, entity.GetFaction());
                                            }
                                            SetMagicType(entity, MAGIC_DULLAHAN);
                                        }
                                        break;
                                    case MAGIC_DULLAHAN:
                                        {
                                            for (int lane = 0; lane < entity.Level.GetMaxLaneCount(); lane++)
                                            {
                                                var x = entity.IsFacingLeft() ? VanillaLevelExt.ENEMY_RIGHT_BORDER : VanillaLevelExt.ENEMY_LEFT_BORDER;
                                                var z = entity.Level.GetEntityLaneZ(lane);
                                                var y = entity.Level.GetGroundY(x, z);
                                                var param = entity.GetSpawnParams();
                                                entity.Spawn(VanillaEnemyID.dullahan, new Vector3(x, y, z), param);
                                                for (int i = 0; i < 2; i++)
                                                {
                                                    entity.Spawn(VanillaEnemyID.dullahanHead, new Vector3(x, y, z), param);
                                                }
                                            }
                                            SetMagicType(entity, MAGIC_MESMERIZER);
                                        }
                                        break;
                                }
                                entity.SetAnimationBool("Shaking", false);
                                entity.SetAnimationInt("LightColor", -1);
                                stateMachine.SetSubState(entity, SUBSTATE_END);
                                substateTimer.ResetTime(30);
                            }
                        }
                        break;
                    case SUBSTATE_END:
                        {
                            var headOpen = GetHeadOpen(entity);
                            headOpen = headOpen * 0.7f;
                            SetHeadOpen(entity, headOpen);

                            if (substateTimer.Expired)
                            {
                                stateMachine.StartState(entity, STATE_IDLE);
                            }
                        }
                        break;
                }
            }
            public const int SUBSTATE_MOVE = 0;
            public const int SUBSTATE_CAST = 1;
            public const int SUBSTATE_END = 2;
        }
        private class SwitchState : EntityStateMachineState
        {
            public SwitchState() : base(STATE_SWITCH) { }
            public override void OnEnter(EntityStateMachine machine, Entity entity)
            {
                base.OnEnter(machine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(60);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var headOpen = GetHeadOpen(entity);
                headOpen *= 0.5f;
                SetHeadOpen(entity, headOpen);

                var substate = stateMachine.GetSubState(entity);
                UpdateMotion(stateMachine, entity, substate);
                if (substate == SUBSTATE_FLYING)
                {
                    var distance2D = new Vector2(GetTargetX(entity) - entity.Position.x, GetTargetZ(entity) - entity.Position.z);
                    if (distance2D.sqrMagnitude < 100)
                    {
                        stateMachine.SetSubState(entity, SUBSTATE_FALLING);
                    }
                }
                else if (substate == SUBSTATE_FALLING)
                {
                    if (entity.GetRelativeY() <= 1)
                    {
                        stateMachine.SetSubState(entity, SUBSTATE_ON_GROUND);
                        entity.PlaySound(VanillaSoundID.witherSpawn);
                        entity.PlaySound(VanillaSoundID.explosion);
                        entity.Explode(entity.GetCenter(), 120, entity.GetFaction(), entity.GetDamage() * 18, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN));

                        var param = entity.GetSpawnParams();
                        var exp = entity.Spawn(VanillaEffectID.explosion, entity.GetCenter(), param);
                        exp.SetSize(Vector3.one * 240);
                        for (int i = 0; i < entity.Level.GetMaxLaneCount(); i++)
                        {
                            if (i == entity.GetLane())
                                continue;
                            var x = entity.Position.x;
                            var z = entity.Level.GetEntityLaneZ(i);
                            var y = entity.Level.GetGroundY(x, z);
                            entity.SpawnWithParams(VanillaEnemyID.dullahan, new Vector3(x, y, z));
                            entity.Spawn(VanillaEnemyID.anubisand, new Vector3(x, y, z), param);
                        }
                    }
                }

                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.Run(stateMachine.GetSpeed(entity));
                if (stateTimer.Expired)
                {
                    stateMachine.StartState(entity, STATE_IDLE);
                }
            }
            private void UpdateMotion(EntityStateMachine stateMachine, Entity entity, int substate)
            {
                var level = entity.Level;
                var x = GetTargetX(entity);
                var z = GetTargetZ(entity);
                var y = level.GetGroundY(x, z);
                if (substate == SUBSTATE_FLYING)
                {
                    y += 80;
                }
                var targetPos = new Vector3(x, y, z);
                var thisPos = entity.Position;
                var dir = targetPos - thisPos;

                var pos = entity.Position;
                pos = pos * 0.5f + targetPos * 0.5f;
                entity.Position = pos;
            }
            private float GetTargetX(Entity entity)
            {
                var level = entity.Level;
                var targetColumn = level.GetMaxColumnCount() - 2;
                return level.GetEntityColumnX(targetColumn);
            }
            private float GetTargetZ(Entity entity)
            {
                var level = entity.Level;
                var targetLane = level.GetMaxLaneCount() / 2;
                return level.GetEntityLaneZ(targetLane);
            }
            public const int SUBSTATE_FLYING = 0;
            public const int SUBSTATE_FALLING = 1;
            public const int SUBSTATE_ON_GROUND = 2;
        }
        private class SummonState : EntityStateMachineState
        {
            public SummonState() : base(STATE_SUMMON) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.ResetTime(15);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var substate = stateMachine.GetSubState(entity);
                var substateTimer = stateMachine.GetSubStateTimer(entity);
                substateTimer.Run(stateMachine.GetSpeed(entity));

                var level = entity.Level;
                switch (substate)
                {
                    case SUBSTATE_MOVE:
                        {
                            //移动
                            var column = entity.IsFacingLeft() ? level.GetMaxColumnCount() - 1 : 0;
                            var lane = level.GetMaxLaneCount() / 2;
                            var targetX = level.GetEntityColumnX(column);
                            var targetZ = level.GetEntityLaneZ(lane);
                            var targetY = level.GetGroundY(targetX, targetZ);

                            var pos = entity.Position;
                            pos.x = pos.x * 0.85f + targetX * 0.15f;
                            pos.y = pos.y * 0.85f + targetY * 0.15f;
                            pos.z = pos.z * 0.85f + targetZ * 0.15f;
                            entity.Position = pos;

                            if (substateTimer.Expired)
                            {
                                substateTimer.ResetTime(30);
                                stateMachine.SetSubState(entity, SUBSTATE_ROAR);
                                entity.PlaySound(VanillaSoundID.witherCry);
                                entity.SetAnimationBool("Shaking", true);
                                entity.SetAnimationInt("LightColor", 3);
                            }
                        }
                        break;

                    case SUBSTATE_ROAR:
                        {
                            //张嘴
                            var headOpen = GetHeadOpen(entity);
                            headOpen = headOpen * 0.7f + 1 * 0.3f;
                            SetHeadOpen(entity, headOpen);

                            if (substateTimer.Expired)
                            {
                                entity.SetAnimationBool("Shaking", false);
                                entity.SetAnimationInt("LightColor", -1);
                                stateMachine.SetSubState(entity, SUBSTATE_SUMMONED);
                                substateTimer.ResetTime(30);

                                entity.PlaySound(VanillaSoundID.witherSpawn);
                                var bedserker = entity.SpawnWithParams(VanillaEnemyID.bedserker, entity.Position + entity.GetFacingDirection() * 80);
                                var brainwasher = entity.SpawnWithParams(VanillaEnemyID.brainwasher, entity.Position + entity.GetFacingDirection() * 80 + new Vector3(0, 0, 80));
                                var jackdullahan = entity.SpawnWithParams(VanillaEnemyID.jackDullahan, entity.Position + entity.GetFacingDirection() * 80 + new Vector3(0, 0, -80));

                                var param = entity.GetSpawnParams();
                                param.SetProperty(EngineEntityProps.SIZE, Vector3.one * 120);
                                entity.Spawn(VanillaEffectID.explosion, bedserker.GetCenter(), param);

                                var cluster = entity.Spawn(VanillaEffectID.smokeCluster, brainwasher.GetCenter());
                                cluster.SetSize(Vector3.one * 120);
                                cluster.SetTint(Color.magenta);
                                var blast = entity.Spawn(VanillaEffectID.soulfireBurn, jackdullahan.GetCenter());
                                blast.SetSize(Vector3.one * 120);
                                entity.PlaySound(VanillaSoundID.explosion);
                            }
                        }
                        break;
                    case SUBSTATE_SUMMONED:
                        {
                            var headOpen = GetHeadOpen(entity);
                            headOpen = headOpen * 0.7f;
                            SetHeadOpen(entity, headOpen);

                            if (substateTimer.Expired)
                            {
                                stateMachine.StartState(entity, STATE_IDLE);
                            }
                        }
                        break;
                }
            }
            public const int SUBSTATE_MOVE = 0;
            public const int SUBSTATE_ROAR = 1;
            public const int SUBSTATE_SUMMONED = 2;
        }
        private class StunState : EntityStateMachineState
        {
            public StunState() : base(STATE_STUNNED) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(30);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.Run(stateMachine.GetSpeed(entity));
                var headOpen = GetHeadOpen(entity);
                headOpen = headOpen * 0.7f + 1 * 0.3f;
                SetHeadOpen(entity, headOpen);

                if (stateTimer.Expired)
                {
                    entity.SetAnimationBool("Shaking", false);
                    stateMachine.StartState(entity, STATE_IDLE);
                }
            }
        }
        private class DeathState : EntityStateMachineState
        {
            public DeathState() : base(STATE_DEATH) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                entity.SetAnimationBool("Shaking", true);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(150);
            }
            public override void OnUpdateLogic(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateLogic(stateMachine, entity);
                var headOpen = GetHeadOpen(entity);
                headOpen = headOpen * 0.7f + 1 * 0.3f;
                SetHeadOpen(entity, headOpen);

                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.Run(stateMachine.GetSpeed(entity));

                if (stateTimer.Expired)
                {
                    entity.PlaySound(VanillaSoundID.witherDeath);
                    entity.PlaySound(VanillaSoundID.explosion);
                    entity.Explode(entity.GetCenter(), 120, entity.GetFaction(), entity.GetDamage() * 18, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN));

                    var param = entity.GetSpawnParams();
                    var exp = entity.Spawn(VanillaEffectID.explosion, entity.GetCenter(), param);
                    exp.SetSize(Vector3.one * 240);
                    entity.Level.ShakeScreen(20, 0, 30);
                    entity.Remove();
                }
            }
        }
        #endregion

    }
}
