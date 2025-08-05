using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.brainwasherExplosion)]
    public class BrainwasherExplosion : Explosion
    {

        #region 公有方法
        public BrainwasherExplosion(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion
    }
}