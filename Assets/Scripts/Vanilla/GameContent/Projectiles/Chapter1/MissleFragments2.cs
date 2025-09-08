using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.missile_fragments_2)]
    public class MissileFragments2 : ProjectileExplodeBehaviour
    {
        public MissileFragments2(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Update(Entity projectile)
        {
            base.Update(projectile);
            float angleSpeed = -projectile.Velocity.x;
            projectile.RenderRotation += Vector3.forward * angleSpeed;
        }
    }
}