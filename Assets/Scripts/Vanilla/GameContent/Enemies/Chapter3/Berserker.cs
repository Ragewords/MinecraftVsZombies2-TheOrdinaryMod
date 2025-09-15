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
            entity.SetModelDamagePercent();
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.HasEffect(VanillaDamageEffects.NO_DEATH_TRIGGER))
                return;
            Explode(entity, entity.GetDamage() * 3, entity.GetFaction());
            entity.Remove();
        }
        public static void Explode(Entity entity, float damage, int faction)
        {
            var scale = entity.GetFinalScale();
            var scaleX = Mathf.Abs(scale.x);
            var range = entity.GetRange() * scaleX;
            var fireRange = range * 1.5f;
            var ligntningRange = range * 2.5f;
            entity.Explode(entity.GetCenter(), range, faction, damage, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE));
            entity.Explode(entity.GetCenter(), fireRange, faction, damage / 2, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.FIRE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE));
            entity.Explode(entity.GetCenter(), ligntningRange, faction, damage / 4, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.LIGHTNING, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE));

            Explosion.Spawn(entity, entity.GetCenter(), range);

            for (int i = 0; i < 15; i++)
            {
                float degree = i * 24;
                float rad = degree * Mathf.Deg2Rad;
                Vector3 pos = entity.GetCenter() + new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * (fireRange / 2);
                entity.Spawn(VanillaEffectID.fireburn, pos);
            }

            for (int i = 0; i < 30; i++)
            {
                float degree = i * 12 + 6;
                float rad = degree * Mathf.Deg2Rad;
                Vector3 pos = entity.GetCenter() + new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * fireRange;
                entity.Spawn(VanillaEffectID.fireburn, pos);
            }

            for (int i = 0; i < 10; i++)
            {
                var arc = entity.Spawn(VanillaEffectID.electricArc, entity.Position);

                float degree = i * 36;
                float rad = degree * Mathf.Deg2Rad;
                Vector3 pos = entity.Position + new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad)) * ligntningRange;
                ElectricArc.Connect(arc, pos);
                ElectricArc.UpdateArc(arc);
            }
            entity.PlaySound(VanillaSoundID.explosion, scaleX == 0 ? 1000 : 1 / (scaleX));
            entity.PlaySound(VanillaSoundID.darkSkiesImpact, scaleX == 0 ? 1000 : 1 / (scaleX));
            entity.PlaySound(VanillaSoundID.powerOff, scaleX == 0 ? 1000 : 1 / (scaleX));
        }
    }
}
