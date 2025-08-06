using MVZ2.Vanilla.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.leaf)]
    public class Leaf : ProjectileBehaviour
    {
        public Leaf(string nsp, string name) : base(nsp, name)
        {
        }
    }
}
