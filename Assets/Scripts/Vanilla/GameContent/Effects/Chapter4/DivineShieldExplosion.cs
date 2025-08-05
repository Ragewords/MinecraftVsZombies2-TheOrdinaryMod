using MVZ2.Vanilla.Entities;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.divineShieldExplosion)]
    public class DivineShieldExplosion : Explosion
    {

        #region 公有方法
        public DivineShieldExplosion(string nsp, string name) : base(nsp, name)
        {
        }
        public static new Entity Spawn(Entity spawner, Vector3 position, Vector3 size)
        {
            var param = spawner.GetSpawnParams();
            param.SetProperty(EngineEntityProps.SIZE, size);
            return spawner.Spawn(VanillaEffectID.divineShieldExplosion, position, param);
        }
        public static new Entity Spawn(Entity spawner, Vector3 position, float radius)
        {
            return Spawn(spawner, position, Vector3.one * (radius * 2));
        }
        #endregion
    }
}