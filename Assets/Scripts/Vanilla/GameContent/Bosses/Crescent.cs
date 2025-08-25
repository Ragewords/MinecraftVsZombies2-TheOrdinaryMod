using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Bosses
{
    [EntityBehaviourDefinition(VanillaBossNames.crescent)]
    public partial class Crescent : BossBehaviour
    {
        public Crescent(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity boss)
        {
            base.Init(boss);
            stateMachine.Init(boss);
            stateMachine.StartState(boss, STATE_IDLE);
            SetPositionBeforeDash(boss, boss.Position);
            SetDashDir(boss, Vector3.left);

            boss.CollisionMaskHostile |=
                EntityCollisionHelper.MASK_PLANT |
                EntityCollisionHelper.MASK_ENEMY |
                EntityCollisionHelper.MASK_OBSTACLE |
                EntityCollisionHelper.MASK_BOSS;

            Fly(boss);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (entity.IsDead)
                return;
            stateMachine.UpdateAI(entity);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            stateMachine.UpdateLogic(entity);
        }
        public override void PostDeath(Entity entity, DeathInfo deathInfo)
        {
            base.PostDeath(entity, deathInfo);

            entity.PlaySound(VanillaSoundID.crescentShock);

            stateMachine.StartState(entity, STATE_DEAD);
        }
        public override void PreTakeDamage(DamageInput input, CallbackResult result)
        {
            base.PreTakeDamage(input, result);
            if (input.Amount > 600)
            {
                input.SetAmount(600);
            }
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            var other = collision.Other;
            var self = collision.Entity;
            if (!other.Exists() || !other.IsHostile(self))
                return;
            if (self.State == STATE_LINE_DASH && stateMachine.GetSubState(self) > 0 && stateMachine.GetSubState(self) < 4)
            {
                collision.OtherCollider.TakeDamage(self.GetDamage() * 0.05f, new DamageEffectList(VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN), self);
            }
        }
        public static void Appear(Entity entity)
        {
            stateMachine.StartState(entity, STATE_APPEAR);
        }
        public static void Fly(Entity entity)
        {
            var flyBuff = entity.AddBuff<FlyBuff>();
            flyBuff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, HEIGHT);
        }
        public static void StopFly(Entity entity)
        {
            entity.RemoveBuffs<FlyBuff>();
        }

        private static void SetBehaviourProperty<T>(Entity entity, PropertyKey<T> name, T value) => entity.SetBehaviourField<T>(name, value);
        private static T GetBehaviourProperty<T>(Entity entity, PropertyKey<T> name) => entity.GetBehaviourField<T>(name);
        public static Vector3 GetPositionBeforeDash(Entity entity) => GetBehaviourProperty<Vector3>(entity, PROP_PREV_POSITION);
        public static void SetPositionBeforeDash(Entity entity, Vector3 value) => SetBehaviourProperty(entity, PROP_PREV_POSITION, value);
        public static Vector3 GetDashDir(Entity entity) => GetBehaviourProperty<Vector3>(entity, PROP_DASH_DIRECTION);
        public static void SetDashDir(Entity entity, Vector3 value) => SetBehaviourProperty(entity, PROP_DASH_DIRECTION, value);
        public static bool AlterJump(Entity entity) => GetBehaviourProperty<bool>(entity, PROP_ALTER_JUMP);
        public static void SetAlterJump(Entity entity, bool value) => SetBehaviourProperty(entity, PROP_ALTER_JUMP, value);
        private static readonly VanillaEntityPropertyMeta<Vector3> PROP_PREV_POSITION = new VanillaEntityPropertyMeta<Vector3>("prevPosition");
        private static readonly VanillaEntityPropertyMeta<Vector3> PROP_DASH_DIRECTION = new VanillaEntityPropertyMeta<Vector3>("DashDirection");
        private static readonly VanillaEntityPropertyMeta<bool> PROP_ALTER_JUMP = new VanillaEntityPropertyMeta<bool>("alterJump");

        public const int STATE_APPEAR = VanillaEntityStates.BOSS_APPEAR;
        public const int STATE_IDLE = VanillaEntityStates.IDLE;
        public const int STATE_LINE_DASH = VanillaEntityStates.ATTACK;
        public const int STATE_SPACE = VanillaEntityStates.BOSS_ATTACK_2;
        public const int STATE_DIVE = VanillaEntityStates.BOSS_ATTACK_3;
        public const int STATE_REST = VanillaEntityStates.BOSS_SPECIAL;
        public const int STATE_DEAD = VanillaEntityStates.DEAD;
        public const float HEIGHT = 30;
        private static CrescentStateMachine stateMachine = new CrescentStateMachine();
    }
}