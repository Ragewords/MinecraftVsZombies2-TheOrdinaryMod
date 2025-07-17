using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.darkMatterParticlesAbsorbing)]
    public class DarkMatterParticlesAbsorbing : EffectBehaviour
    {

        #region 公有方法
        public DarkMatterParticlesAbsorbing(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            if (entity.Parent != null && entity.Parent.Exists())
            {
                entity.Position = entity.Parent.Position;
            }
        }
        #endregion
    }
}