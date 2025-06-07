using MVZ2.Vanilla.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.arrowBullet)]
    public class ArrowBullet : ProjectileBehaviour
    {
        public ArrowBullet(string nsp, string name) : base(nsp, name)
        {
        }
    }
}
