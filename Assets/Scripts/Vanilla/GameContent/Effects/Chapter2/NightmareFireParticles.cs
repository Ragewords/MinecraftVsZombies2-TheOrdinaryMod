using MVZ2.Vanilla.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.nightmareFireParticles)]
    public class NightmareFireParticles : EffectBehaviour
    {

        #region 公有方法
        public NightmareFireParticles(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion
    }
}