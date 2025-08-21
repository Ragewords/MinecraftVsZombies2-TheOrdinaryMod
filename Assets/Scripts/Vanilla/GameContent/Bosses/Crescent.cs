using System.Linq;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Difficulties;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Grids;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    [EntityBehaviourDefinition(VanillaBossNames.crescent)]
    public class Crescent : BossBehaviour
    {
        public Crescent(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetStateTimer(entity, new FrameTimer(240));
            SetActionTimer(entity, new FrameTimer(60));
            var flyBuff = entity.AddBuff<FlyBuff>();
            flyBuff.SetProperty(FlyBuff.PROP_FLY_SPEED, 0.2f);
            flyBuff.SetProperty(FlyBuff.PROP_FLY_SPEED_FACTOR, 0.5f);
            flyBuff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 120f);
            SetProjectileRNG(entity, new RandomGenerator(entity.RNG.Next()));
            entity.SetInvisible(true);
            SetSoundPlayed(entity, false);
            SetAttackState(entity, STATE_DARK_MATTER);
        }
        public override void PreTakeDamage(DamageInput input, CallbackResult result)
        {
            base.PreTakeDamage(input, result);
            if (input.Amount > 600)
            {
                input.SetAmount(600);
            }
        }
        public override void PostTakeDamage(DamageOutput result)
        {
            base.PostTakeDamage(result);
            if (result == null || result.BodyResult == null)
                return;
            var boss = result.Entity;
            var takenDamage = GetRecentTakenDamage(boss);
            takenDamage += result.BodyResult.SpendAmount;
            SetRecentTakenDamage(boss, takenDamage);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (entity.IsDead)
                return;
            AttackUpdate(entity);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            if (entity.IsDead)
            {
                entity.Timeout--;
                if (entity.Timeout == 60)
                {
                    entity.TriggerAnimation("FadeOut");
                }
                else if (entity.Timeout <= 0)
                {
                    entity.Remove();
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo deathInfo)
        {
            base.PostDeath(entity, deathInfo);

            entity.PlaySound(VanillaSoundID.theEyeDeath);
            entity.SetAnimationBool("IsDead", true);
            var level = entity.Level;
            var buff = entity.GetFirstBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 60f);
            Vector3 pos = new(level.GetEntityColumnX(4), 60, level.GetEntityLaneZ(2));
            entity.Position = pos;
            entity.Timeout = 100;
        }
        #region Move
        private void MoveLower(Entity entity)
        {
            entity.SetInvisible(false);
            var buff = entity.GetFirstBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 10f);
        }
        private void MoveHigher(Entity entity)
        {
            entity.SetInvisible(true);
            var buff = entity.GetFirstBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 120f);
        }
        #endregion

        #region Attack
        private void AttackUpdate(Entity entity)
        {
            int attackState = GetAttackState(entity);
            var timer = GetStateTimer(entity);
            timer.Run();
            if (timer.Expired)
            {
                switch (attackState)
                {
                    case STATE_DARK_MATTER:
                        {
                            if (!IsSoundPlayed(entity))
                            {
                                entity.PlaySound(VanillaSoundID.magnetic);
                                SetSoundPlayed(entity, true);
                                entity.SetAnimationInt("AttackState", 2);
                            }

                            var transTimer = GetActionTimer(entity);
                            transTimer.Run();

                            if (transTimer.PassedFrame(10))
                                entity.SetAnimationInt("AttackState", 1);

                            if (transTimer.Expired)
                            {
                                var level = entity.Level;
                                var contraption = level.FindEntities(e => e.Type == EntityTypes.PLANT && e.IsHostile(entity)).RandomTake(1, GetProjectileRNG(entity));
                                foreach (var target in contraption)
                                {
                                    var targetPos = target.Position;
                                    var effectPos = target.GetCenter();
                                    var projectileID = VanillaProjectileID.darkMatterBall;
                                    var projectileDefinition = entity.Level.Content.GetEntityDefinition(projectileID);
                                    var projectileGravity = projectileDefinition?.GetGravity() ?? 0;

                                    var shotParams = entity.GetShootParams();
                                    shotParams.position = entity.GetCenter();
                                    shotParams.soundID = VanillaSoundID.odd;
                                    shotParams.projectileID = projectileID;
                                    shotParams.velocity = VanillaProjectileExt.GetLobVelocityByTime(entity.Position, targetPos, 45, projectileGravity);

                                    var proj = entity.ShootProjectile(shotParams);
                                    proj.SetParent(entity);

                                    entity.Spawn(VanillaEffectID.darkMatterParticlesAbsorbing, effectPos);
                                }
                                SetAttackState(entity, STATE_EXECUTE);
                                SetSoundPlayed(entity, false);
                                entity.SetAnimationInt("AttackState", 0);
                                transTimer.ResetTime(90);
                                timer.Reset();
                            }
                        }
                        break;
                    case STATE_EXECUTE:
                        {
                            if (!IsSoundPlayed(entity))
                            {
                                entity.PlaySound(VanillaSoundID.fault);
                                SetSoundPlayed(entity, true);
                            }
                            entity.SetAnimationInt("AttackState", 2);

                            var transTimer = GetActionTimer(entity);
                            transTimer.Run();
                            if (transTimer.Expired)
                            {
                                var level = entity.Level;
                                entity.PlaySound(VanillaSoundID.odd);

                                var contraptions = level.FindEntities(e => e.Type == EntityTypes.PLANT
                                 && e.GetTakingGridLayers(e.GetGrid()).Contains(VanillaGridLayers.main))
                                .OrderByDescending(e => e.GetCost()).Take(1);

                                foreach (var contraption in contraptions)
                                {
                                    entity.SpawnWithParams(VanillaEffectID.executioner, contraption.Position);
                                }

                                entity.SetAnimationInt("AttackState", 0);
                                SetSoundPlayed(entity, false);
                                SetAttackState(entity, STATE_MIND_BLAST);
                                transTimer.ResetTime(210);
                                timer.Reset();
                            }
                        }
                        break;
                    case STATE_MIND_BLAST:
                        {
                            if (!IsSoundPlayed(entity))
                            {
                                entity.PlaySound(VanillaSoundID.theEyeStretch);
                                SetSoundPlayed(entity, true);
                                entity.SetAnimationInt("AttackState", 2);
                            }

                            var transTimer = GetActionTimer(entity);
                            transTimer.Run();
                            if (transTimer.PassedFrame(150))
                            {
                                entity.SetAnimationInt("AttackState", 3);
                                entity.PlaySound(VanillaSoundID.theEyeRoar);
                                entity.PlaySound(VanillaSoundID.theEyeScreamAttack);
                            }
                            if (transTimer.Frame <= 150)
                            {
                                var level = entity.Level;
                                var contraption = level.FindEntities(e => e.IsHostile(entity) && e.IsVulnerableEntity());
                                foreach (var target in contraption)
                                {
                                    target.TakeDamage(1, new DamageEffectList(VanillaDamageEffects.MUTE), entity);
                                }
                                level.ShakeScreen(10, 0, 10);
                            }

                            if (transTimer.Expired)
                            {
                                entity.SetAnimationInt("AttackState", 4);
                                SetSoundPlayed(entity, false);
                                SetAttackState(entity, STATE_REST);
                                transTimer.ResetTime(60);
                                timer.Reset();
                                MoveLower(entity);
                            }
                        }
                        break;
                    case STATE_REST:
                        {
                            var level = entity.Level;
                            var takenDamage = GetRecentTakenDamage(entity);
                            if (takenDamage >= level.GetCrescentDamageThresold() && !entity.IsDead)
                            {
                                entity.SetAnimationInt("AttackState", 0);
                                SetAttackState(entity, STATE_DARK_MATTER);
                                SetRecentTakenDamage(entity, 0);
                                timer.Reset();
                                MoveHigher(entity);
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        #region ����
        public static RandomGenerator GetProjectileRNG(Entity boss) => boss.GetBehaviourField<RandomGenerator>(PROP_PROJECTILE_RNG);
        public static void SetProjectileRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(PROP_PROJECTILE_RNG, value);
        public static FrameTimer GetActionTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(PROP_Action_TIMER);
        public static void SetActionTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(PROP_Action_TIMER, value);
        public static int GetAttackState(Entity boss) => boss.GetBehaviourField<int>(PROP_ATTACK_STATE);
        public static void SetAttackState(Entity boss, int value) => boss.SetBehaviourField(PROP_ATTACK_STATE, value);
        public static bool IsSoundPlayed(Entity boss) => boss.GetBehaviourField<bool>(PROP_SOUND);
        public static void SetSoundPlayed(Entity boss, bool value) => boss.SetBehaviourField(PROP_SOUND, value);
        public static FrameTimer GetStateTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(PROP_STATE_TIMER);
        public static void SetStateTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(PROP_STATE_TIMER, value);
        public static float GetRecentTakenDamage(Entity boss) => boss.GetBehaviourField<float>(PROP_RECENT_TAKEN_DAMAGE);
        public static void SetRecentTakenDamage(Entity boss, float value) => boss.SetBehaviourField(PROP_RECENT_TAKEN_DAMAGE, value);
        #endregion

        public const int STATE_DARK_MATTER = 0;
        public const int STATE_EXECUTE = 1;
        public const int STATE_MIND_BLAST = 2;
        public const int STATE_REST = 3;

        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_Action_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("ActionTimer");
        public static readonly VanillaEntityPropertyMeta<RandomGenerator> PROP_PROJECTILE_RNG = new VanillaEntityPropertyMeta<RandomGenerator>("ProjectileRNG");
        public static readonly VanillaEntityPropertyMeta<int> PROP_ATTACK_STATE = new VanillaEntityPropertyMeta<int>("AttackState");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_SOUND = new VanillaEntityPropertyMeta<bool>("Sound");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");
        private static readonly VanillaEntityPropertyMeta<float> PROP_RECENT_TAKEN_DAMAGE = new VanillaEntityPropertyMeta<float>("RecentTakenDamage");
    }
}
