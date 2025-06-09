using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.whitehole)]
    public class Whitehole : EffectBehaviour
    {

        #region 公有方法
        public Whitehole(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.PlaySound(VanillaSoundID.gravitation);
        }

        public override void Update(Entity entity)
        {
            base.Update(entity);

            entity.SetDisplayScale(Vector3.one * (entity.GetRange() / 80));

            bool active = entity.Timeout > 5;
            entity.SetAnimationBool("Started", active);
        }
    }
}