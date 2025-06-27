using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.brainwasherExplosion)]
    public class BrainwasherExplosion : EffectBehaviour
    {

        #region 公有方法
        public BrainwasherExplosion(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            entity.SetModelProperty("Size", entity.GetScaledSize());
        }
        #endregion
    }
}