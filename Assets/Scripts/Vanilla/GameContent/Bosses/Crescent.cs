using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Bosses
{
    [EntityBehaviourDefinition(VanillaBossNames.crescent)]
    public class Crescent : BossBehaviour
    {
        public Crescent(string nsp, string name) : base(nsp, name)
        {
        }
    }
}