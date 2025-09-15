using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.rallyZombie)]
    public class RallyZombie : FlagZombie
    {
        public RallyZombie(string nsp, string name) : base(nsp, name)
        {
            IZombieOnly = false;
        }
    }
}
