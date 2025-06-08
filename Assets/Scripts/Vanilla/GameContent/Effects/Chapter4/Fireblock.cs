using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.fireblock)]
    public class Fireblock : EffectBehaviour
    {

        #region 公有方法
        public Fireblock(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion

        public override void Update(Entity entity)
        {
            entity.SetAnimationBool("Cursed", IsCursed(entity));
        }
        public static void SetCursed(Entity entity, bool value) => entity.SetProperty(PROP_CURSED, value);
        public static bool IsCursed(Entity entity) => entity.GetProperty<bool>(PROP_CURSED);
        public static readonly VanillaBuffPropertyMeta<bool> PROP_CURSED = new VanillaBuffPropertyMeta<bool>("cursed");
    }
}