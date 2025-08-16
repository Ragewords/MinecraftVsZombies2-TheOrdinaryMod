using MVZ2.GameContent.Bosses;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.nightmareMeteor)]
    public class NightmareMeteor : EffectBehaviour
    {

        #region 公有方法
        public NightmareMeteor(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion

        public override void PostContactGround(Entity entity, Vector3 velocity)
        {
            base.PostContactGround(entity, velocity);
            entity.Spawn(VanillaEffectID.nightmareFireParticles, entity.GetCenter());
            entity.Spawn(VanillaBossID.theEye, entity.Position);

            entity.PlaySound(VanillaSoundID.meteorLand);
            entity.Level.ShakeScreen(10, 0, 15);
            entity.Remove();
        }
    }
}