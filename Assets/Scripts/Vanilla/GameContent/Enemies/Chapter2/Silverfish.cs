using MVZ2.Vanilla.Enemies;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.silverfish)]
    public class Silverfish : MeleeEnemy
    {
        public Silverfish(string nsp, string name) : base(nsp, name)
        {
        }
    }
}