using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.jackDullahan)]
    public class JackDullahan : StateEnemy
    {
        public JackDullahan(string nsp, string name) : base(nsp, name)
        {
            spinDetector = new JackDullahanSpinDetector(48);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.ChangeModel(VanillaModelID.jackDullahanMain);
            var param = entity.GetSpawnParams();
            if (entity.IsPreviewEnemy())
            {
                param.SetProperty(VanillaEnemyProps.PREVIEW_ENEMY, true);
            }
            var horse = entity.Spawn(VanillaEnemyID.soulSkeletonHorse, entity.Position, param);
            entity.RideOn(horse);
            entity.SetAnimationBool("Sitting", true);
            entity.SetAnimationBool("HoldingHead", !IsHeadDropped(entity));
        }
        public override void PreTakeDamage(DamageInput input, CallbackResult result)
        {
            base.PreTakeDamage(input, result);
            if (input.Effects.HasEffect(VanillaDamageEffects.GOLD))
            {
                input.Multiply(3);
            }
        }
        protected override int GetActionState(Entity enemy)
        {
            var baseState = base.GetActionState(enemy);
            if (baseState == VanillaEntityStates.WALK)
            {
                var horse = enemy.GetRidingEntity();
                var hasHorse = horse.ExistsAndAlive();
                if (hasHorse)
                {
                    return STATE_IDLE;
                }
                else
                {
                    return STATE_CAST;
                }
            }
            return baseState;
        }
        protected override void UpdateAI(Entity enemy)
        {
            base.UpdateAI(enemy);
            var horse = enemy.GetRidingEntity();
            if (horse == null)
            {
                DropHead(enemy);
            }
            else if (horse.IsEntityOf(VanillaEnemyID.soulSkeletonHorse))
            {
                if (horse.State == SkeletonHorse.STATE_JUMP)
                {
                    DropHead(enemy);
                }
            }
            if (enemy.State == STATE_CAST)
            {
                var vel = enemy.Velocity;
                vel.x = enemy.GetSpeed() * 1.5f * enemy.GetFacingX();
                enemy.Velocity = vel;
                if (enemy.IsTimeInterval(10))
                {
                    enemy.PlaySound(VanillaSoundID.swing, pitch: enemy.RNG.Next(0.8f, 1.2f));
                }
                spinBuffer.Clear();
                spinDetector.DetectEntities(enemy, spinBuffer);
                foreach (var target in spinBuffer)
                {
                    target.TakeDamage(5, new DamageEffectList(VanillaDamageEffects.MUTE), enemy);
                    target.InflictWither(150);
                }
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);

            var horse = entity.GetRidingEntity();
            var hasHorse = horse.ExistsAndAlive();
            entity.SetAnimationBool("Sitting", hasHorse);
            entity.SetAnimationBool("HoldingHead", !IsHeadDropped(entity));
            entity.SetModelDamagePercent();
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.NO_DEATH_TRIGGER))
            {
                return;
            }
            DropHead(entity);
        }
        public static void DropHead(Entity entity)
        {
            if (IsHeadDropped(entity))
                return;
            entity.SpawnWithParams(VanillaEnemyID.jackDullahanHead, entity.GetCenter());
            SetHeadDropped(entity, true);
        }

        public static bool IsHeadDropped(Entity entity) => entity.GetBehaviourField<bool>(ID, FIELD_HEAD_DROPPED);
        public static void SetHeadDropped(Entity entity, bool value) => entity.SetBehaviourField(ID, FIELD_HEAD_DROPPED, value);

        public static readonly VanillaEntityPropertyMeta<bool> FIELD_HEAD_DROPPED = new VanillaEntityPropertyMeta<bool>("HeadDropped");
        public const int STATE_IDLE = VanillaEntityStates.IDLE;
        public const int STATE_CAST = VanillaEntityStates.ENEMY_CAST;
        private static readonly NamespaceID ID = VanillaEnemyID.jackDullahan;
        private Detector spinDetector;
        private List<Entity> spinBuffer = new List<Entity>();
    }
}
