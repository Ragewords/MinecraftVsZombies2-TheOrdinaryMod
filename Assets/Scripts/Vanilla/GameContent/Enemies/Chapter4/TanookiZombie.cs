using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.tanookiZombie)]
    public class TanookiZombie : MeleeEnemy
    {
        public TanookiZombie(string nsp, string name) : base(nsp, name)
        {
            smashDetector = new CollisionDetector();
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetJumpTimer(entity, new FrameTimer(210));
            SetStatueTimer(entity, new FrameTimer(30));
        }
        protected override int GetActionState(Entity enemy)
        {
            var state = base.GetActionState(enemy);
            var jumpTimer = GetJumpTimer(enemy);
            if (state == VanillaEntityStates.WALK && IsJumping(enemy))
            {
                return STATE_CAST;
            }
            return state;
        }
        protected override void WalkUpdate(Entity enemy)
        {
            var jumpTimer = GetJumpTimer(enemy);
            if (jumpTimer.Expired)
                return;
            base.WalkUpdate(enemy);
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);

            if (entity.IsDead)
                return;
            if (entity.State == VanillaEntityStates.ATTACK)
                return;
            if (entity.HasBuff<TanookiZombieStoneBuff>())
                return;
            var jumpTarget = entity.Level.GetEntityGridPosition(entity.GetColumn() + entity.GetFacingX(), entity.GetLane());

            var jumpTimer = GetJumpTimer(entity);
            var statueTimer = GetStatueTimer(entity);
            jumpTimer.Run(entity.GetAttackSpeed());
            if (jumpTimer.Expired)
            {
                if (entity.IsOnGround && !IsJumping(entity))
                {
                    entity.PlaySound(VanillaSoundID.jizo_appear);
                    entity.Velocity = VanillaProjectileExt.GetLobVelocityByTime(entity.Position, jumpTarget + Vector3.up * 240, 30, entity.GetGravity());
                    SetJumping(entity, true);
                }
                else if (IsJumping(entity))
                {
                    statueTimer.Run();
                    if (statueTimer.Expired)
                    {
                        entity.AddBuff<TanookiZombieStoneBuff>();
                        var effect = entity.Level.Spawn(VanillaEffectID.smokeCluster, entity.GetCenter(), entity);
                        effect.SetTint(new Color(0.5f, 0.5f, 0.5f, 1));
                        effect.SetSize(entity.GetSize() * 2);
                        SetJumping(entity, false);
                        statueTimer.Reset();
                        jumpTimer.Reset();
                    }
                }
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelDamagePercent();
            entity.SetAnimationBool("Stone", entity.HasBuff<TanookiZombieStoneBuff>());
        }
        public override void PostContactGround(Entity anvil, Vector3 velocity)
        {
            base.PostContactGround(anvil, velocity);

            if (!anvil.HasBuff<TanookiZombieStoneBuff>())
                return;
            if (velocity != Vector3.zero)
            {
                smashBuffer.Clear();
                smashDetector.DetectMultiple(anvil, smashBuffer);
                foreach (var target in smashBuffer)
                {
                    var other = target.Entity;
                    if (anvil.IsHostile(other))
                    {
                        float damageModifier = Mathf.Clamp(velocity.magnitude, 0, 1);
                        target.TakeDamage(300 * damageModifier, new DamageEffectList(VanillaDamageEffects.PUNCH, VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BOTH_ARMOR_AND_BODY), anvil);
                    }
                }
            }
        }

        public static readonly NamespaceID ID = VanillaEnemyID.tanookiZombie;
        public const int STATE_CAST = VanillaEntityStates.TANOOKI_ZOMBIE_JUMP;
        public static void SetJumping(Entity entity, bool value) => entity.SetBehaviourField(ID, PROP_JUMPING, value);
        public static bool IsJumping(Entity entity) => entity.GetBehaviourField<bool>(ID, PROP_JUMPING);
        public static void SetJumpTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_JUMP_TIMER, timer);
        public static FrameTimer GetJumpTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_JUMP_TIMER);
        public static void SetStatueTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_STATUE_TIMER, timer);
        public static FrameTimer GetStatueTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_STATUE_TIMER);
        public static readonly VanillaBuffPropertyMeta<bool> PROP_JUMPING = new VanillaBuffPropertyMeta<bool>("jumping");
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_JUMP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("jumpTimer");
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_STATUE_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("statueTimer");
        private List<IEntityCollider> smashBuffer = new List<IEntityCollider>();
        private Detector smashDetector;
    }
}
