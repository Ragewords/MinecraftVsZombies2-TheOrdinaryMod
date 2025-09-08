using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.missile)]
    public class Missile : ProjectileExplodeBehaviour
    {
        public Missile(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Explode(Entity entity)
        {
            base.Explode(entity);
            for (var i = 0; i < 6; i++)
            {
                var rng = entity.RNG;
                var xspeed = rng.Next(-8f, 8f);
                var zspeed = rng.Next(-8f, 8f);
                var yspeed = rng.Next(10f);
                var shootparams = VanillaProjectileID.missile_fragments_3;
                if (i == 0)
                    shootparams = VanillaProjectileID.missile_fragments_1;
                if (i == 1)
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
    }
}
