using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;
using static UnityEngine.Networking.UnityWebRequest;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.missile_fragments_1)]
    public class MissileFragments1 : ProjectileBehaviour
    {
        public MissileFragments1(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Update(Entity projectile)
        {
            base.Update(projectile);
            float angleSpeed = -projectile.Velocity.x * 2f;
            projectile.RenderRotation += Vector3.forward * angleSpeed;
        }
        protected override void PreHitEntity(ProjectileHitInput hit, DamageInput damage, CallbackResult result)
        {
            base.PreHitEntity(hit, damage, result);
            result.SetFinalValue(false);
        }
        public override void PostDeath(Entity entity, DeathInfo damageInfo)
        {
            base.PostDeath(entity, damageInfo);
            if (damageInfo.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            Explode(entity);
        }
        public static void Explode(Entity entity)
        {
            var range = entity.GetRange();
            entity.PlaySound(VanillaSoundID.explosion);
            var explosion = entity.Level.Spawn(VanillaEffectID.explosion, entity.GetCenter(), entity);
            explosion.SetSize(Vector3.one * range);
            var damageEffects = new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.MUTE);
            entity.Level.Explode(entity.Position, range, entity.GetFaction(), entity.GetDamage(), damageEffects, entity);
        }
        public static NamespaceID ID => VanillaProjectileID.fireCharge;
    }
}
