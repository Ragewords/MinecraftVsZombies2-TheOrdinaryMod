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
        public override void Update(Entity entity)
        {
            base.Update(entity);
            if (entity.Timeout <= 0)
            {
                var effectPosition = entity.GetCenter() + Vector3.up * 37;
                entity.Spawn(VanillaEffectID.nightmareFireParticles, effectPosition);
                entity.Spawn(VanillaBossID.crescent, entity.Position);

                entity.PlaySound(VanillaSoundID.meteorLand);
                entity.PlaySound(VanillaSoundID.explosion);
                Explosion.Spawn(entity, effectPosition, 120);
                entity.Level.ShakeScreen(10, 0, 15);
                entity.Remove();
            }
        }
    }
}