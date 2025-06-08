using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Projectiles;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.hellfire)]
    public class Hellfire : ContraptionBehaviour
    {
        public Hellfire(string nsp, string name) : base(nsp, name)
        {
            burnDetector = new SphereDetector(BURN_RADIUS)
            {
                mask = EntityCollisionHelper.MASK_PLANT
                | EntityCollisionHelper.MASK_ENEMY
                | EntityCollisionHelper.MASK_OBSTACLE
                | EntityCollisionHelper.MASK_BOSS
            };
            jalapenoDetector = new LaneDetector(40, 40);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.CollisionMaskFriendly = EntityCollisionHelper.MASK_PROJECTILE;
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationBool("Evoked", IsCursed(entity));
            
            var cooldown = GetDamageCooldown(entity);
            cooldown--;
            if (cooldown <= 0)
            {
                detectBuffer.Clear();
                burnDetector.DetectEntities(entity, detectBuffer);
                var damageMultipiler = IsCursed(entity) ? 2 : 1;
                foreach (var target in detectBuffer)
                {
                    target.TakeDamage(entity.GetDamage() * damageMultipiler, new DamageEffectList(VanillaDamageEffects.FIRE), entity);
                    if (!IsCursed(entity))
                        target.Spawn(VanillaEffectID.fireburn, target.GetCenter());
                    else
                        target.Spawn(VanillaEffectID.cursedFireburn, target.GetCenter());
                    target.PlaySound(VanillaSoundID.fire, volume : 0.6f);
                }
                cooldown = DAMAGE_COOLDOWN;
            }
            SetDamageCooldown(entity, cooldown);
        }
        public override void PostTakeDamage(DamageOutput result)
        {
            base.PostTakeDamage(result);
            if (result.HasAnyFatal())
            {
                var entity = result.Entity;
                jalapenoDetectBuffer.Clear();
                jalapenoDetector.DetectEntities(entity, jalapenoDetectBuffer);
                var damageMultipiler = IsCursed(entity) ? 2 : 1;
                foreach (var target in jalapenoDetectBuffer)
                {
                    target.TakeDamage(entity.GetDamage() * 60 * damageMultipiler, new DamageEffectList(VanillaDamageEffects.FIRE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN), entity);
                }
                entity.PlaySound(VanillaSoundID.flame);
                var border_distance = VanillaLevelExt.PROJECTILE_RIGHT_BORDER - VanillaLevelExt.PROJECTILE_LEFT_BORDER;
                for (var i = 0; i < Mathf.CeilToInt(border_distance / 48); i++)
                {
                    var x_pos = VanillaLevelExt.PROJECTILE_LEFT_BORDER + 48 * i;
                    var block = entity.Spawn(VanillaEffectID.fireblock, new Vector3(x_pos, entity.Level.GetGroundY(x_pos, entity.Position.z), entity.Position.z));
                    Fireblock.SetCursed(block, IsCursed(entity));
                    block.Timeout += i * 3;
                }
            }
        }
        public override bool CanEvoke(Entity entity)
        {
            if (IsCursed(entity))
                return false;
            var meteor = GetMeteor(entity);
            if (meteor != null && meteor.Exists(entity.Level))
                return false;
            return base.CanEvoke(entity);

        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var pos = entity.Position + new Vector3(0, 1280, 0);
            var meteor = entity.SpawnWithParams(VanillaEffectID.cursedMeteor, pos);
            meteor.SetParent(entity);
            SetMeteor(entity, new EntityID(meteor));
            meteor.PlaySound(VanillaSoundID.bombFalling);
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            if (state == EntityCollisionHelper.STATE_EXIT)
                return;
            var other = collision.Other;
            var canIgnite = other.Definition?.HasBehaviour<HellfireIgnitedProjectileBehaviour>() ?? false;
            if (!canIgnite)
                return;
            var self = collision.Entity;
            if (!self.IsFriendly(other))
                return;
            if (other.HasBuff<HellfireIgnitedBuff>())
                return;
            var buff = other.AddBuff<HellfireIgnitedBuff>();
            if (IsCursed(self))
            {
                HellfireIgnitedBuff.Curse(buff);
            }
        }
        public static void Curse(Entity entity)
        {
            SetCursed(entity, true);
            entity.AddBuff<HellfireCursedBuff>();
        }
        public static void SetCursed(Entity entity, bool value) => entity.SetProperty(PROP_CURSED, value);
        public static bool IsCursed(Entity entity) => entity.GetProperty<bool>(PROP_CURSED);
        public static void SetMeteor(Entity entity, EntityID value) => entity.SetProperty(PROP_METEOR, value);
        public static EntityID GetMeteor(Entity entity) => entity.GetProperty<EntityID>(PROP_METEOR);
        public static int GetDamageCooldown(Entity entity) => entity.GetBehaviourField<int>(PROP_DAMAGE_COOLDOWN);
        public static void SetDamageCooldown(Entity entity, int value) => entity.SetBehaviourField(PROP_DAMAGE_COOLDOWN, value);
        public static readonly VanillaBuffPropertyMeta<bool> PROP_CURSED = new VanillaBuffPropertyMeta<bool>("cursed");
        public static readonly VanillaBuffPropertyMeta<EntityID> PROP_METEOR = new VanillaBuffPropertyMeta<EntityID>("meteor");
        private static readonly VanillaEntityPropertyMeta<int> PROP_DAMAGE_COOLDOWN = new VanillaEntityPropertyMeta<int>("DamageCooldown");
        private List<Entity> detectBuffer = new List<Entity>();
        private List<Entity> jalapenoDetectBuffer = new List<Entity>();
        private Detector burnDetector;
        private Detector jalapenoDetector;
        public const float BURN_RADIUS = 40;
        public const int DAMAGE_COOLDOWN = 30;
    }
}
