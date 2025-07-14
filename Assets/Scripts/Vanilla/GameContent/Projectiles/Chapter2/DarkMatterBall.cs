using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.darkMatterBall)]
    public class DarkMatterBall : ProjectileBehaviour
    {
        public DarkMatterBall(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void PreHitEntity(ProjectileHitInput hit, DamageInput damage, CallbackResult result)
        {
            base.PreHitEntity(hit, damage, result);
            result.SetFinalValue(false);
        }
        public override void PostDeath(Entity entity, DeathInfo damageInfo)
        {
            base.PostDeath(entity, damageInfo);
            if (damageInfo.HasEffect(VanillaDamageEffects.NO_DEATH_TRIGGER))
                return;
            Explode(entity, entity.GetRange(), entity.GetDamage());
        }
        public static DamageOutput[] Explode(Entity entity, float range, float damage)
        {
            var damageEffects = new DamageEffectList(VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.EXPLOSION);
            var damageOutputs = entity.Explode(entity.Position, range, entity.GetFaction(), damage, damageEffects);
            foreach (var output in damageOutputs)
            {
                var result = output?.Entity;
                if (result.Type == EntityTypes.PLANT && result.CanDeactive())
                {
                    result.ShortCircuit(90);
                    result.PlaySound(VanillaSoundID.powerOff);
                }
            }
            var param = entity.GetSpawnParams();
            param.SetProperty(EngineEntityProps.SIZE, Vector3.one * (range * 2));
            param.SetProperty(EngineEntityProps.TINT, Color.black);
            entity.Spawn(VanillaEffectID.explosion, entity.GetCenter(), param);
            entity.PlaySound(VanillaSoundID.explosion);
            entity.Level.ShakeScreen(10, 0, 15);

            return damageOutputs;
        }
        public static NamespaceID ID => VanillaProjectileID.fireCharge;
    }
}

