using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;
using Tools;
using MVZ2.Vanilla.Callbacks;
using PVZEngine.Callbacks;
using PVZEngine.Damages;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.hellfire)]
    public class Hellfire : ContraptionBehaviour
    {
        public Hellfire(string nsp, string name) : base(nsp, name)
        {
            detector = new HellfireIgniteDetector(32)
            {
                factionTarget = FactionTarget.Friendly,
                mask = EntityCollisionHelper.MASK_PROJECTILE,
            };
            burnDetector = new SphereDetector(BURN_RADIUS);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetDamageCooldown(entity, new FrameTimer(DAMAGE_COOLDOWN));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);

            detectBuffer.Clear();
            burnDetector.DetectEntities(entity, detectBuffer);

            var cooldown = GetDamageCooldown(entity);
            cooldown.Run(entity.GetAttackSpeed());
            if (cooldown.Expired)
            {
                var damageMultipiler = IsCursed(entity) ? 2 : 1;
                foreach (var target in detectBuffer)
                {
                    target.TakeDamage(entity.GetDamage() * damageMultipiler, new DamageEffectList(VanillaDamageEffects.FIRE), entity);
                    if (!IsCursed(entity))
                        target.Spawn(VanillaEffectID.fireburn, target.GetCenter());
                    else
                        target.Spawn(VanillaEffectID.cursedFireburn, target.GetCenter());
                    target.PlaySound(VanillaSoundID.fire, volume: 0.6f);
                }
                cooldown.Reset();
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            UpdateIgnite(entity);
            entity.SetAnimationBool("Evoked", IsCursed(entity));
        }
        public override void PostDeath(Entity entity, DeathInfo deathInfo)
        {
            base.PostDeath(entity, deathInfo);
            if (deathInfo.HasEffect(VanillaDamageEffects.NO_DEATH_TRIGGER) || deathInfo.HasEffect(VanillaDamageEffects.DIG))
                return;

            var damageMultipiler = IsCursed(entity) ? 2 : 1;
            Explode(entity, entity.GetDamage() * 45 * damageMultipiler);
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
        private void UpdateIgnite(Entity hellfire)
        {
            bool cursed = IsCursed(hellfire);
            igniteBuffer.Clear();
            detector.DetectEntities(hellfire, igniteBuffer);
            foreach (Entity target in igniteBuffer)
            {
                var behaviour = target.Definition?.GetBehaviour<IHellfireIgniteBehaviour>();
                if (behaviour == null)
                    return;
                behaviour.Ignite(target, hellfire, cursed);
            }
        }
        public static void Curse(Entity entity)
        {
            SetCursed(entity, true);
            entity.AddBuff<HellfireCursedBuff>();
        }
        public static DamageOutput[] Explode(Entity entity, float damage)
        {
            var level = entity.Level;
            List<DamageOutput> damageOutputs = new List<DamageOutput>();
            var border_distance = VanillaLevelExt.RIGHT_BORDER - VanillaLevelExt.LEFT_BORDER;

            var center = new Vector3(VanillaLevelExt.LAWN_CENTER_X, 500, entity.Position.z);
            foreach (var entityCollider in level.OverlapBox(center, new Vector3(border_distance, 1000, 80), entity.GetFaction(), EntityCollisionHelper.MASK_VULNERABLE, 0))
            {
                var damageEffects = new DamageEffectList(VanillaDamageEffects.FIRE, VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN);
                var damageOutput = entityCollider.TakeDamage(damage, damageEffects, entity);
                if (damageOutput != null)
                {
                    damageOutputs.Add(damageOutput);
                }
            }

            entity.Level.ShakeScreen(10, 0, 15);
            Explosion.Spawn(entity, entity.GetCenter(), BURN_RADIUS);
            entity.PlaySound(VanillaSoundID.explosion);
            entity.PlaySound(VanillaSoundID.flame);
            for (var i = 0; i < Mathf.CeilToInt(border_distance / 64); i++)
            {
                var x_pos = VanillaLevelExt.LEFT_BORDER + 64 * i;
                var block = entity.Spawn(VanillaEffectID.fireblock, new Vector3(x_pos, entity.Level.GetGroundY(x_pos, entity.Position.z), entity.Position.z));
                Fireblock.SetCursed(block, IsCursed(entity));
                if (IsCursed(entity))
                    block.AddBuff<HellfireCursedBuff>();
                block.Timeout += i * 2;
            }
            entity.Level.Triggers.RunCallbackFiltered(VanillaLevelCallbacks.POST_CONTRAPTION_DETONATE, new EntityCallbackParams(entity), entity.GetDefinitionID());

            return damageOutputs.ToArray();
        }
        public static void SetCursed(Entity entity, bool value) => entity.SetProperty(PROP_CURSED, value);
        public static bool IsCursed(Entity entity) => entity.GetProperty<bool>(PROP_CURSED);
        public static void SetMeteor(Entity entity, EntityID value) => entity.SetProperty(PROP_METEOR, value);
        public static EntityID GetMeteor(Entity entity) => entity.GetProperty<EntityID>(PROP_METEOR);
        public static FrameTimer GetDamageCooldown(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_DAMAGE_COOLDOWN);
        public static void SetDamageCooldown(Entity entity, FrameTimer value) => entity.SetBehaviourField(PROP_DAMAGE_COOLDOWN, value);
        public static readonly VanillaBuffPropertyMeta<bool> PROP_CURSED = new VanillaBuffPropertyMeta<bool>("cursed");
        public static readonly VanillaBuffPropertyMeta<EntityID> PROP_METEOR = new VanillaBuffPropertyMeta<EntityID>("meteor");
        private Detector detector;
        private List<Entity> igniteBuffer = new List<Entity>();
        private static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_DAMAGE_COOLDOWN = new VanillaEntityPropertyMeta<FrameTimer>("DamageCooldown");
        private Detector burnDetector;
        private List<Entity> detectBuffer = new List<Entity>();

        public const float BURN_RADIUS = 40;
        public const int DAMAGE_COOLDOWN = 30;
    }
    public interface IHellfireIgniteBehaviour
    {
        void Ignite(Entity entity, Entity hellfire, bool cursed);
    }
}
