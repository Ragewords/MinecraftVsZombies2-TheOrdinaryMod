using MVZ2.Vanilla.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.fire)]
    public class Fire : EffectBehaviour
    {

        #region 公有方法
        public Fire(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion
    }
}