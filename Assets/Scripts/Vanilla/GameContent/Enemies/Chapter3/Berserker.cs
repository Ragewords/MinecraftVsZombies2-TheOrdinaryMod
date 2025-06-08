using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.berserker)]
    public class Berserker : MeleeEnemy
    {
        public Berserker(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationInt("HealthState", entity.GetHealthState(2));
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            if (info.Effects.HasEffect(VanillaDamageEffects.DROWN))
                return;
            Explode(entity, entity.GetDamage() * 3, entity.GetFaction());
            entity.Remove();
        }
        public static void Explode(Entity entity, float damage, int faction)
        {
            var scale = entity.GetFinalScale();
            var scaleX = Mathf.Abs(scale.x);
            var range = entity.GetRange() * scaleX;
            entity.Explode(entity.GetCenter(), range, faction, damage, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE));
            entity.Level.Explode(entity.GetCenter(), range * 2, faction, damage / 2, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.FIRE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE), entity);
            entity.Level.Explode(entity.GetCenter(), range * 3, faction, damage / 6, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.LIGHTNING, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE), entity);

            var explosion = entity.Level.Spawn(VanillaEffectID.explosion, entity.GetCenter(), entity);
            explosion.SetSize(Vector3.one * (range * 2));
            float arcLength = range * 3;
            float fireLength = range * 2;
            var level = entity.Level;
            for (int i = 0; i < 8; i++)
            {
                var arc = level.Spawn(VanillaEffectID.electricArc, entity.Position, entity);

                float degree = i * 45;
                float rad = degree * Mathf.Deg2Rad;
                Vector3 pos = entity.Position + new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * arcLength;
                ElectricArc.Connect(arc, pos);
                ElectricArc.UpdateArc(arc);
            }
            for (int i = 0; i < 30; i++)
            {
                float degree = i * 12;
                float rad = degree * Mathf.Deg2Rad;
                Vector3 pos = entity.Position + new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * fireLength;
                entity.Level.Spawn(VanillaEffectID.fireburn, pos, entity);
            }
            for (int i = 0; i < 30; i++)
            {
                float degree = i * 12 + 6;
                float rad = degree * Mathf.Deg2Rad;
                Vector3 pos = entity.Position + new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * (fireLength / 2);
                entity.Level.Spawn(VanillaEffectID.fireburn, pos, entity);
            }
            entity.PlaySound(VanillaSoundID.explosion, scaleX == 0 ? 1000 : 1 / (scaleX));
            entity.PlaySound(VanillaSoundID.darkSkiesImpact, scaleX == 0 ? 1000 : 1 / (scaleX));
            entity.PlaySound(VanillaSoundID.powerOff, scaleX == 0 ? 1000 : 1 / (scaleX));
        }
    }
}
