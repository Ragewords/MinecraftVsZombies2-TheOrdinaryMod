using System.Linq;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Grids;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using Tools.Mathematics;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    [EntityBehaviourDefinition(VanillaBossNames.theEye)]
    public class TheEye : BossBehaviour
    {
        public TheEye(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetStunTimer(entity, new FrameTimer(120));
            SetStateTimer(entity, new FrameTimer(240));
            SetActionTimer(entity, new FrameTimer(60));
            var flyBuff = entity.AddBuff<FlyBuff>();
            flyBuff.SetProperty(FlyBuff.PROP_FLY_SPEED, 0.2f);
            flyBuff.SetProperty(FlyBuff.PROP_FLY_SPEED_FACTOR, 0.5f);
            flyBuff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 120f);
            SetProjectileRNG(entity, new RandomGenerator(entity.RNG.Next()));
            SetMoveRNG(entity, new RandomGenerator(entity.RNG.Next()));
            SetSoundPlayed(entity, false);
            SetAttackState(entity, STATE_DARK_MATTER);
            entity.SetIsInvisible(true);
        }
        public override void PreTakeDamage(DamageInput input, CallbackResult result)
        {
            base.PreTakeDamage(input, result);
            if (input.Amount > 600)
            {
                input.SetAmount(600);
            }
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
                if (entity.Timeout == 50)
                {
                    entity.SetAnimationBool("FadeOut", true);
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
        public static void FakeStun(Entity entity)
        {
            entity.TriggerAnimation("Stun");
        }
        #region Move
        private void MoveLower(Entity entity)
        {
            entity.SetIsInvisible(false);
            var buff = entity.GetFirstBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 10f);
        }
        private void MoveBack(Entity entity)
        {
            entity.SetIsInvisible(true);
            var buff = entity.GetFirstBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 120f);
        }
        private void MoveHigher(Entity entity)
        {
            entity.SetIsInvisible(true);
            var buff = entity.GetFirstBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 480f);
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

                                    var expPart = entity.Level.Spawn(VanillaEffectID.explosion, entity.GetCenter(), entity);
                                    expPart.SetSize(Vector3.one * 100);
                                    expPart.SetTint(Color.black);
                                }
                                SetAttackState(entity, STATE_MESS);
                                SetSoundPlayed(entity, false);
                                entity.SetAnimationInt("AttackState", 0);
                                transTimer.ResetTime(90);
                                timer.Reset();
                            }
                        }
                        break;
                    case STATE_MESS:
                        {
                            if (!IsSoundPlayed(entity))
                            {
                                entity.PlaySound(VanillaSoundID.magnetic);
                                SetSoundPlayed(entity, true);
                                MoveHigher(entity);
                            }
                            entity.SetAnimationInt("AttackState", 2);

                            var transTimer = GetActionTimer(entity);
                            transTimer.Run();
                            if (transTimer.PassedFrame(60))
                            {
                                var level = entity.Level;
                                entity.PlaySound(VanillaSoundID.odd);
                                var contraption = level.FindEntities(e => e.Type == EntityTypes.PLANT && e.IsHostile(entity));
                                foreach (var target in contraption)
                                {
                                    target.AddBuff<RUAWizardBuff>();
                                }
                            }
                            if (transTimer.PassedFrame(45))
                            {
                                var level = entity.Level;
                                entity.PlaySound(VanillaSoundID.reverseVampire);
                                var enemy = level.FindEntities(e => e.Type == EntityTypes.ENEMY && e.IsFriendly(entity));
                                foreach (var target in enemy)
                                {
                                    target.RandomChangeAdjacentLane(target.RNG);
                                }
                            }
                            if (transTimer.PassedFrame(30))
                            {
                                var level = entity.Level;
                                entity.PlaySound(VanillaSoundID.fault);
                                var contraptions = level.FindEntities(e => e.Type == EntityTypes.PLANT && e.IsHostile(entity)).Where(c => c.IsDispenser()).RandomTake(3, entity.RNG);
                                var grids = level.GetAllGrids().Where(g => g.IsEmpty()).RandomTake(6, entity.RNG);
                                foreach (var contraption in contraptions)
                                {
                                    var ID = contraption.GetDefinitionID();
                                    var placementID = contraption.Definition.GetPlacementID();
                                    var placementDef = level.Content.GetPlacementDefinition(placementID);
                                    if (placementDef == null)
                                        continue;
                                    var targetGrids = grids.Where(g => g.CanSpawnEntity(contraption.GetDefinitionID()));
                                    if (targetGrids.Count() <= 0)
                                        continue;
                                    var grid = targetGrids.Random(GetProjectileRNG(entity));
                                    entity.SpawnWithParams(contraption.Definition.GetID(), grid.GetEntityPosition());
                                }
                            }
                            if (transTimer.Expired)
                            {
                                entity.SetAnimationInt("AttackState", 0);
                                SetSoundPlayed(entity, false);
                                SetAttackState(entity, STATE_BLAST);
                                transTimer.ResetTime(210);
                                timer.Reset();
                                MoveBack(entity);
                            }
                        }
                        break;
                    case STATE_BLAST:
                        {
                            if (!IsSoundPlayed(entity))
                            {
                                entity.PlaySound(VanillaSoundID.theEyeStretch);
                                SetSoundPlayed(entity, true);
                                entity.SetAnimationInt("AttackState", 2);
                                MoveLower(entity);
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
                                    target.TakeDamage(1, new DamageEffectList(VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN), entity);
                                }
                            }

                            if (transTimer.Expired)
                            {
                                entity.SetAnimationInt("AttackState", 0);
                                SetSoundPlayed(entity, false);
                                SetAttackState(entity, STATE_DARK_MATTER);
                                transTimer.ResetTime(60);
                                timer.Reset();
                                MoveBack(entity);
                            }
                        }
                        break;
                }
            }
        }
        #endregion

        #region ����
        public static FrameTimer GetStunTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(ID, PROP_MOVE_TIMER);
        public static void SetStunTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(ID, PROP_MOVE_TIMER, value);
        public static int GetMoveTimeout(Entity boss) => boss.GetBehaviourField<int>(ID, PROP_MOVE_TIMEOUT);
        public static void SetMoveTimeout(Entity boss, int value) => boss.SetBehaviourField(ID, PROP_MOVE_TIMEOUT, value);
        public static Vector3 GetMoveDisplacement(Entity boss) => boss.GetBehaviourField<Vector3>(ID, PROP_MOVE_DISPLACEMENT);
        public static void SetMoveDisplacement(Entity boss, Vector3 value) => boss.SetBehaviourField(ID, PROP_MOVE_DISPLACEMENT, value);
        public static RandomGenerator GetMoveRNG(Entity boss) => boss.GetBehaviourField<RandomGenerator>(ID, PROP_MOVE_RNG);
        public static void SetMoveRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(ID, PROP_MOVE_RNG, value);
        public static RandomGenerator GetProjectileRNG(Entity boss) => boss.GetBehaviourField<RandomGenerator>(ID, PROP_PROJECTILE_RNG);
        public static void SetProjectileRNG(Entity boss, RandomGenerator value) => boss.SetBehaviourField(ID, PROP_PROJECTILE_RNG, value);
        public static FrameTimer GetActionTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(ID, PROP_Action_TIMER);
        public static void SetActionTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(ID, PROP_Action_TIMER, value);
        public static int GetAttackState(Entity boss) => boss.GetBehaviourField<int>(ID, PROP_ATTACK_STATE);
        public static void SetAttackState(Entity boss, int value) => boss.SetBehaviourField(ID, PROP_ATTACK_STATE, value);
        public static bool IsSoundPlayed(Entity boss) => boss.GetBehaviourField<bool>(ID, PROP_SOUND);
        public static void SetSoundPlayed(Entity boss, bool value) => boss.SetBehaviourField(ID, PROP_SOUND, value);
        public static FrameTimer GetStateTimer(Entity boss) => boss.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);
        public static void SetStateTimer(Entity boss, FrameTimer value) => boss.SetBehaviourField(ID, PROP_STATE_TIMER, value);
        #endregion

        public const int STATE_DARK_MATTER = 0;
        public const int STATE_MESS = 1;
        public const int STATE_BLAST = 2;

        public const int MAX_MOVE_TIMEOUT = 60;

        public static readonly NamespaceID ID = VanillaBossID.theEye;

        public static readonly VanillaEntityPropertyMeta<RandomGenerator> PROP_MOVE_RNG = new VanillaEntityPropertyMeta<RandomGenerator>("MoveRNG");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_MOVE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("MoveTimer");
        public static readonly VanillaEntityPropertyMeta<int> PROP_MOVE_TIMEOUT = new VanillaEntityPropertyMeta<int>("MoveTimeout");
        public static readonly VanillaEntityPropertyMeta<Vector3> PROP_MOVE_DISPLACEMENT = new VanillaEntityPropertyMeta<Vector3>("MoveDisplacement");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_Action_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("ActionTimer");
        public static readonly VanillaEntityPropertyMeta<RandomGenerator> PROP_PROJECTILE_RNG = new VanillaEntityPropertyMeta<RandomGenerator>("ProjectileRNG");
        public static readonly VanillaEntityPropertyMeta<int> PROP_ATTACK_STATE = new VanillaEntityPropertyMeta<int>("AttackState");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_SOUND = new VanillaEntityPropertyMeta<bool>("Sound");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");
    }
}
