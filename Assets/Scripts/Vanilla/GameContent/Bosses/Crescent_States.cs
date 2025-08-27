using System.Collections;
using System.Linq;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Projectiles;
using MVZ2.GameContent.Shells;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    public partial class Crescent
    {
        #region 状态机
        private class CrescentStateMachine : EntityStateMachine
        {
            public CrescentStateMachine()
            {
                AddState(new AppearState());
                AddState(new IdleState());
                AddState(new LineDashState());
                AddState(new SpaceState());
                AddState(new DiveState());
                AddState(new DeadState());
            }
        }
        public static void UpdatePosition(Entity entity)
        {
            var posi = GetPositionBeforeDash(entity);
            var pos = entity.Position;
            pos.x = pos.x * 0.3f + posi.x * 0.7f;
            pos.y = pos.y * 0.3f + posi.y * 0.7f;
            pos.z = pos.z * 0.3f + posi.z * 0.7f;
            entity.Position = pos;
        }
        private class AppearState : EntityStateMachineState
        {
            public AppearState() : base(STATE_APPEAR) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(30);
                StopFly(entity);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var nextStateTimer = stateMachine.GetStateTimer(entity);
                nextStateTimer.Run(stateMachine.GetSpeed(entity));
                if (!nextStateTimer.Expired)
                    return;
                stateMachine.StartState(entity, STATE_IDLE);
                Fly(entity);
            }
        }
        private class IdleState : EntityStateMachineState
        {
            public IdleState() : base(STATE_IDLE) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var stateTimer = stateMachine.GetStateTimer(entity);
                stateTimer.ResetTime(60);
            }
            public override void OnExit(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnExit(stateMachine, entity);
                SetAlterJump(entity, !AlterJump(entity));
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var nextStateTimer = stateMachine.GetStateTimer(entity);
                nextStateTimer.Run(stateMachine.GetSpeed(entity));
                if (!nextStateTimer.Expired)
                    return;

                var lastState = stateMachine.GetPreviousState(entity);

                if (lastState == STATE_LINE_DASH)
                {
                    lastState = STATE_SPACE;
                }
                else if (lastState == STATE_SPACE)
                {
                    lastState = STATE_DIVE;
                }
                else
                {
                    lastState = STATE_LINE_DASH;
                }
                stateMachine.StartState(entity, lastState);
                stateMachine.SetPreviousState(entity, lastState);
            }
        }
        private class LineDashState : EntityStateMachineState
        {
            public LineDashState() : base(STATE_LINE_DASH) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var stateTimer = stateMachine.GetSubStateTimer(entity);
                stateTimer.ResetTime(15);
                entity.PlaySound(VanillaSoundID.crescentPreDash);
                SetPositionBeforeDash(entity, entity.Position + Vector3.right * 80 * (entity.Position.x < VanillaLevelExt.LAWN_CENTER_X ? -1 : 1));
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var grid = entity.Level.GetAllGrids().Where(g => g.Column == (entity.Position.x < VanillaLevelExt.LAWN_CENTER_X ? entity.Level.GetMaxColumnCount() - 1 : 0)).Random(entity.RNG);
                var endGrid = entity.Level.GetAllGrids().Where(g => g.Column >= 2 && g.Column <= 6).Random(entity.RNG);
                var dir = (grid.GetEntityPosition() - new Vector3(entity.Position.x, entity.GetGroundY(), entity.Position.z)).normalized;
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.Run(stateMachine.GetSpeed(entity));
                var substate = stateMachine.GetSubState(entity);
                if (subStateTimer.Expired)
                {
                    switch (substate)
                    {
                        case SUBSTATE_PREPARE:
                        case SUBSTATE_DASH_1:
                        case SUBSTATE_DASH_2:
                            stateMachine.SetSubState(entity, substate + 1);
                            entity.PlaySound(VanillaSoundID.crescentDash);
                            SetDashDir(entity, dir);
                            subStateTimer.ResetTime(60);
                            ResetPosition(entity);
                            break;
                        case SUBSTATE_DASH_3:
                            SetPositionBeforeDash(entity, endGrid.GetEntityPosition() + Vector3.up * HEIGHT);
                            stateMachine.SetSubState(entity, SUBSTATE_END);
                            subStateTimer.ResetTime(15);
                            break;
                        case SUBSTATE_END:
                            stateMachine.StartState(entity, STATE_IDLE);
                            break;
                    }
                }
                else
                {
                    switch (substate)
                    {
                        case SUBSTATE_DASH_1:
                        case SUBSTATE_DASH_2:
                        case SUBSTATE_DASH_3:
                            entity.Velocity = GetDashDir(entity) * 20;
                            if (subStateTimer.PassedInterval(3))
                            {
                                var dir_vertical1 = Quaternion.Euler(Vector3.up * 90) * GetDashDir(entity);
                                var dir_vertical2 = Quaternion.Euler(Vector3.up * -90) * GetDashDir(entity);
                                var projectile1 = entity.ShootProjectile(new ShootParams()
                                {
                                    projectileID = VanillaProjectileID.reflectionBullet,
                                    position = entity.Position,
                                    velocity = dir_vertical1 * 10,
                                    faction = entity.GetFaction(),
                                    damage = entity.GetDamage() / 5
                                });
                                var projectile2 = entity.ShootProjectile(new ShootParams()
                                {
                                    projectileID = VanillaProjectileID.reflectionBullet,
                                    position = entity.Position,
                                    velocity = dir_vertical2 * 10,
                                    faction = entity.GetFaction(),
                                    damage = entity.GetDamage() / 5
                                });
                            }
                            break;
                        case SUBSTATE_PREPARE:
                        case SUBSTATE_END:
                            entity.Velocity = Vector3.zero;
                            UpdatePosition(entity);
                            break;
                    }
                }
            }
            private void ResetPosition(Entity entity)
            {
                var pos1 = entity.Position;
                if (pos1.x < VanillaLevelExt.ENEMY_LEFT_BORDER)
                {
                    pos1.x = VanillaLevelExt.ENEMY_LEFT_BORDER;
                    entity.Position = pos1;
                }
                else if (pos1.x > VanillaLevelExt.ENEMY_RIGHT_BORDER)
                {
                    pos1.x = VanillaLevelExt.ENEMY_RIGHT_BORDER;
                    entity.Position = pos1;
                }
            }

            public const int SUBSTATE_PREPARE = 0;
            public const int SUBSTATE_DASH_1 = 1;
            public const int SUBSTATE_DASH_2 = 2;
            public const int SUBSTATE_DASH_3 = 3;
            public const int SUBSTATE_END = 4;
        }
        private class SpaceState : EntityStateMachineState
        {
            public SpaceState() : base(STATE_SPACE) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var stateTimer = stateMachine.GetSubStateTimer(entity);
                stateTimer.ResetTime(15);
                entity.PlaySound(VanillaSoundID.crescentPreDash);
                SetPositionBeforeDash(entity, entity.Position + Vector3.down * 30);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var targets = entity.Level.FindEntities(e => e.IsVulnerableEntity() && e.IsHostile(entity) && e.ExistsAndAlive());
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.Run(stateMachine.GetSpeed(entity));
                var substate = stateMachine.GetSubState(entity);
                switch (substate)
                {
                    case SUBSTATE_PREPARE:
                        entity.Velocity = Vector3.zero;
                        UpdatePosition(entity);
                        if (subStateTimer.Expired)
                        {
                            StopFly(entity);

                            if (targets.Length > 0)
                            {
                                var target = targets.Random(entity.RNG);
                                entity.PlaySound(VanillaSoundID.crescentDash);
                                var maxy = entity.GetCenter().y + 80f;
                                var pos = target.GetGrid().GetEntityPosition();
                                entity.Velocity = VanillaProjectileExt.GetLobVelocity(entity.Position, pos + Vector3.up * 32, maxy, entity.GetGravity());
                                SetPositionBeforeDash(entity, pos);
                                stateMachine.SetSubState(entity, SUBSTATE_DASH);
                            }
                            else
                                stateMachine.SetSubState(entity, SUBSTATE_END);
                        }
                        break;
                    case SUBSTATE_DASH:
                        if (entity.GetRelativeY() <= 0)
                        {
                            entity.Velocity = Vector3.zero;
                            entity.PlaySound(VanillaSoundID.smallExplosion);
                            entity.PlaySound(VanillaSoundID.crescentShock);
                            foreach (var collider in entity.Level.OverlapSphere(entity.GetCenter(), 60, entity.GetFaction(), EntityCollisionHelper.MASK_PLANT, 0))
                            {
                                collider.Entity?.AddBuff<LevitationBuff>();
                            }
                            stateMachine.SetSubState(entity, SUBSTATE_END);
                            subStateTimer.ResetTime(15);
                        }
                        break;
                    case SUBSTATE_END:
                        UpdatePosition(entity);
                        if (subStateTimer.Expired)
                        {
                            Fly(entity);
                            stateMachine.StartState(entity, STATE_IDLE);
                        }
                        break;
                }
            }

            public const int SUBSTATE_PREPARE = 0;
            public const int SUBSTATE_DASH = 1;
            public const int SUBSTATE_END = 2;
        }
        private class DiveState : EntityStateMachineState
        {
            public DiveState() : base(STATE_DIVE) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                var stateTimer = stateMachine.GetSubStateTimer(entity);
                stateTimer.ResetTime(5);
                entity.PlaySound(VanillaSoundID.crescentPreDash);
                var grid = entity.Level.GetAllGrids().Random(entity.RNG);
                SetPositionBeforeDash(entity, grid.GetEntityPosition() + Vector3.up * 400);
                StopFly(entity);
            }
            public override void OnUpdateAI(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateAI(stateMachine, entity);
                var grid = entity.Level.GetAllGrids().Random(entity.RNG);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.Run(stateMachine.GetSpeed(entity));
                var substate = stateMachine.GetSubState(entity);
                switch (substate)
                {
                    case SUBSTATE_PREPARE:
                        UpdatePosition(entity);
                        if (subStateTimer.Expired)
                        {
                            entity.PlaySound(VanillaSoundID.crescentDash);
                            stateMachine.SetSubState(entity, SUBSTATE_DASH);
                            entity.Velocity = Vector3.down * 40;
                        }
                        break;
                    case SUBSTATE_DASH:
                        if (entity.GetRelativeY() <= 0)
                        {
                            entity.Velocity = Vector3.down * 40;
                            ShootDanmaku(entity, 0);
                            SetPositionBeforeDash(entity, grid.GetEntityPosition() + Vector3.up * 400);
                            stateMachine.SetSubState(entity, SUBSTATE_DANMAKU_1);
                            subStateTimer.ResetTime(5);
                        }
                        break;
                    case SUBSTATE_DANMAKU_1:
                    case SUBSTATE_DANMAKU_2:
                        if (!subStateTimer.Expired)
                            UpdatePosition(entity);
                        if (entity.GetRelativeY() <= 0)
                        {
                            entity.Velocity = Vector3.down * 40;
                            ShootDanmaku(entity, substate - 1);
                            subStateTimer.ResetTime(5);
                            stateMachine.SetSubState(entity, substate + 1);
                            SetPositionBeforeDash(entity, grid.GetEntityPosition() + Vector3.up * 400);
                        }
                        break;
                    case SUBSTATE_DANMAKU_3:
                        if (!subStateTimer.Expired)
                            UpdatePosition(entity);
                        if (entity.GetRelativeY() <= 0)
                        {
                            entity.Velocity = Vector3.zero;
                            ShootDanmaku(entity, 3);
                            Fly(entity);
                            SetPositionBeforeDash(entity, grid.GetEntityPosition());
                            subStateTimer.ResetTime(15);
                            stateMachine.SetSubState(entity, SUBSTATE_END);
                        }
                        break;
                    case SUBSTATE_END:
                        UpdatePosition(entity);
                        if (subStateTimer.Expired)
                        {
                            entity.Velocity = Vector3.zero;
                            stateMachine.StartState(entity, STATE_IDLE);
                        }
                        break;
                }
            }
            private void ShootDanmaku(Entity entity, int stage)
            {
                entity.PlaySound(VanillaSoundID.smallExplosion);
                entity.PlaySound(VanillaSoundID.crescentShock);
                entity.PlaySound(VanillaSoundID.danmaku, volume: 0.5f);
                foreach (var collider in entity.Level.OverlapSphere(entity.GetCenter(), 40, entity.GetFaction(), EntityCollisionHelper.MASK_PLANT, 0))
                {
                    collider.Entity?.TakeDamage(entity.GetDamage() / 2, new DamageEffectList(), entity);
                }
                switch (stage)
                {
                    case 0:
                        for (int i = 0; i < 4; i++)
                        {
                            var direction = Quaternion.Euler(0, i * 90, 0) * Vector3.right * 10;
                            var velocity = direction;
                            entity.ShootProjectile(new ShootParams()
                            {
                                projectileID = VanillaProjectileID.arrowBullet,
                                position = entity.GetCenter(),
                                velocity = velocity,
                                faction = entity.GetFaction(),
                                damage = entity.GetDamage() / 4
                            });
                        }
                        break;
                    case 1:
                        for (int i = 0; i < 8; i++)
                        {
                            var direction = Quaternion.Euler(0, i * 45, 0) * Vector3.right * 10;
                            var velocity = direction;
                            entity.ShootProjectile(new ShootParams()
                            {
                                projectileID = VanillaProjectileID.arrowBullet,
                                position = entity.GetCenter(),
                                velocity = velocity,
                                faction = entity.GetFaction(),
                                damage = entity.GetDamage() / 4
                            });
                        }
                        break;
                    case 2:
                        for (int i = 0; i < 10; i++)
                        {
                            var direction = Quaternion.Euler(0, i * 36, 0) * Vector3.right * 10;
                            var velocity = direction;
                            entity.ShootProjectile(new ShootParams()
                            {
                                projectileID = VanillaProjectileID.arrowBullet,
                                position = entity.GetCenter(),
                                velocity = velocity,
                                faction = entity.GetFaction(),
                                damage = entity.GetDamage() / 4
                            });
                        }
                        break;
                    case 3:
                        for (int i = 0; i < 12; i++)
                        {
                            var direction = Quaternion.Euler(0, i * 30, 0) * Vector3.right * 10;
                            var velocity = direction;
                            entity.ShootProjectile(new ShootParams()
                            {
                                projectileID = VanillaProjectileID.arrowBullet,
                                position = entity.GetCenter(),
                                velocity = velocity,
                                faction = entity.GetFaction(),
                                damage = entity.GetDamage() / 4
                            });
                        }
                        break;
                }
            }

            public const int SUBSTATE_PREPARE = 0;
            public const int SUBSTATE_DASH = 1;
            public const int SUBSTATE_DANMAKU_1 = 2;
            public const int SUBSTATE_DANMAKU_2 = 3;
            public const int SUBSTATE_DANMAKU_3 = 4;
            public const int SUBSTATE_END = 5;
        }
        private class DeadState : EntityStateMachineState
        {
            public DeadState() : base(STATE_DEAD) { }
            public override void OnEnter(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnEnter(stateMachine, entity);
                entity.Velocity = Vector3.zero;
                entity.TriggerAnimation("Shock");
                SetPositionBeforeDash(entity, entity.Level.GetGrid(4, 2).GetEntityPosition() + Vector3.up * HEIGHT);
                var stateTimer = stateMachine.GetSubStateTimer(entity);
                stateTimer.ResetTime(60);
            }
            public override void OnUpdateLogic(EntityStateMachine stateMachine, Entity entity)
            {
                base.OnUpdateLogic(stateMachine, entity);
                var subStateTimer = stateMachine.GetSubStateTimer(entity);
                subStateTimer.Run();
                var substate = stateMachine.GetSubState(entity);
                switch (substate)
                {
                    case SUBSTATE_ENTER:
                    case SUBSTATE_SHOCK_0:
                    case SUBSTATE_SHOCK_1:
                    case SUBSTATE_SHOCK_2:
                        entity.Velocity = Vector3.zero;
                        var posi = GetPositionBeforeDash(entity);
                        var pos = entity.Position;
                        pos.x = pos.x * 0.3f + posi.x * 0.7f;
                        pos.y = pos.y * 0.3f + posi.y * 0.7f;
                        pos.z = pos.z * 0.3f + posi.z * 0.7f;
                        entity.Position = pos;

                        if (subStateTimer.Expired)
                        {
                            entity.TriggerAnimation("Shock");
                            stateMachine.SetSubState(entity, substate + 1);
                            entity.PlaySound(VanillaSoundID.crescentShock);
                            subStateTimer.ResetTime(60 - substate * 18);
                        }
                        break;
                    case SUBSTATE_SHOCK_3:
                        if (subStateTimer.Expired)
                        {
                            Explosion.Spawn(entity, entity.GetCenter(), 120);
                            entity.PlaySound(VanillaSoundID.explosion);
                            stateMachine.SetSubState(entity, SUBSTATE_END);
                            subStateTimer.ResetTime(45);
                        }
                        break;
                    case SUBSTATE_END:
                        StopFly(entity);
                        entity.AddBuff<NightmareaperFallBuff>();
                        entity.Velocity = Vector3.up * 20;
                        if (subStateTimer.Expired)
                        {
                            entity.Spawn(VanillaEffectID.darkMatterParticles, entity.Position);
                            Explosion.Spawn(entity, entity.GetCenter(), 120);
                            entity.PlaySound(VanillaSoundID.explosion);
                            entity.Remove();
                        }
                        break;
                }
            }
            public const int SUBSTATE_ENTER = 0;
            public const int SUBSTATE_SHOCK_0 = 1;
            public const int SUBSTATE_SHOCK_1 = 2;
            public const int SUBSTATE_SHOCK_2 = 3;
            public const int SUBSTATE_SHOCK_3 = 4;
            public const int SUBSTATE_END = 5;
        }
        #endregion
    }
}