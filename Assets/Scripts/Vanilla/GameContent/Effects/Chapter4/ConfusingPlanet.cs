using System.Collections.Generic;
using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.confusingPlanet)]
    public class ConfusingPlanet : EffectBehaviour
    {
        #region 公有方法
        public ConfusingPlanet(string nsp, string name) : base(nsp, name)
        {
            absorbDetector = new SphereDetector(120)
            {
                mask = EntityCollisionHelper.MASK_PLANT | EntityCollisionHelper.MASK_ENEMY,
                canDetectInvisible = true,
            };
        }
        #endregion
        public override void Init(Entity planet)
        {
            base.Init(planet);
            planet.CollisionMaskHostile |=
                EntityCollisionHelper.MASK_BOSS;
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            bool inactive = entity.Timeout <= 90;
            entity.SetAnimationBool("Disappear", inactive && !IsAnnihilate(entity));
            entity.SetAnimationBool("Collapse", inactive && IsAnnihilate(entity));

            BlackHoleUpdate(entity);
            AnnihilationUpdate(entity);

            if (inactive)
                return;
                
            detectBuffer.Clear();
            absorbDetector.DetectMultiple(entity, detectBuffer);
            foreach (var target in detectBuffer)
            {
                target.Entity.Slow(120);
            }
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            var other = collision.Other;
            var self = collision.Entity;
            bool inactive = self.Timeout <= 90;
            if (inactive)
                return;
            if (!other.Exists())
                return;
            if (other.IsEntityOf(VanillaBossID.theGiant) && TheGiant.IsSnake(other))
            {
                other.TakeDamage(COLLIDE_SELF_DAMAGE, new DamageEffectList(VanillaDamageEffects.MUTE), self);
                TheGiant.KillSnake(other);
            }
        }
        private void BlackHoleUpdate(Entity entity)
        {
            if (IsAnnihilate(entity)) return;

            if (entity.Timeout == 80)
            {
                var range = entity.GetRange();
                var damage = entity.GetDamage();

                var blackholeParam = entity.GetSpawnParams();
                blackholeParam.SetProperty(VanillaEntityProps.DAMAGE, damage);
                blackholeParam.SetProperty(VanillaEntityProps.RANGE, range);
                var blackhole = entity.Spawn(VanillaEffectID.blackhole, entity.GetCenter(), blackholeParam);
                entity.PlaySound(VanillaSoundID.gravitation);
                entity.Level.ShakeScreen(10, 0, 15);
            }
        }
        private void AnnihilationUpdate(Entity entity)
        {
            if (!IsAnnihilate(entity)) return;

            if (entity.Timeout <= 0)
            {
                var range = entity.GetRange();
                var fieldParam = entity.GetSpawnParams();
                fieldParam.SetProperty(VanillaEntityProps.RANGE, range);
                var field = entity.Spawn(VanillaEffectID.annihilationField, entity.GetCenter(), fieldParam);

                entity.PlaySound(VanillaSoundID.explosion);
                entity.PlaySound(VanillaSoundID.gravitation);
                entity.Level.ShakeScreen(10, 0, 15);
            }
        }
        public const float COLLIDE_SELF_DAMAGE = 600;
        public static void SetAnnihilate(Entity entity, bool value) => entity.SetProperty(PROP_ANNIHILATE, value);
        public static bool IsAnnihilate(Entity entity) => entity.GetProperty<bool>(PROP_ANNIHILATE);
        public static readonly VanillaBuffPropertyMeta<bool> PROP_ANNIHILATE = new VanillaBuffPropertyMeta<bool>("annihilate");
        private List<IEntityCollider> detectBuffer = new List<IEntityCollider>();
        private Detector absorbDetector;
    }
}