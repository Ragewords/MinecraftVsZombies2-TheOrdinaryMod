using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.missile)]
    public class Missile : ProjectileBehaviour
    {
        public Missile(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void PreHitEntity(ProjectileHitInput hit, DamageInput damage)
        {
            base.PreHitEntity(hit, damage);
            damage.Cancel();
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
            explosion.SetSize(Vector3.one * (range * 2));
            var damageEffects = new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.MUTE);
            entity.Level.Explode(entity.Position, range, entity.GetFaction(), entity.GetDamage(), damageEffects, entity);
            for (var i = 1; i <= 6; i++)
            {
                var rng = entity.RNG;
                var xspeed = rng.Next(-8f, 8f);
                var zspeed = rng.Next(-8f, 8f);
                var yspeed = rng.Next(10f);
                var shootparams = VanillaProjectileID.missile_fragments_3;
                if (i == 1)
                    shootparams = VanillaProjectileID.missile_fragments_1;
                if (i == 2)
                    shootparams = VanillaProjectileID.missile_fragments_2;
                var param = new ShootParams()
                {
                    damage = entity.GetDamage() * 0.3f,
                    faction = entity.GetFaction(),
                    position = entity.GetCenter(),
                    projectileID = shootparams,
                    velocity = new Vector3(xspeed, yspeed, zspeed),
                };
                entity.ShootProjectile(param);

            }
        }
        public static NamespaceID ID => VanillaProjectileID.fireCharge;
    }
}
