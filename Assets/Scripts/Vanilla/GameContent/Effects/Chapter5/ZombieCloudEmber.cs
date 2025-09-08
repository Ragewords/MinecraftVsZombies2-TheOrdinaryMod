using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.zombieCloudEmber)]
    public class ZombieCloudEmber : EntityBehaviourDefinition
    {
        public ZombieCloudEmber(string nsp, string name) : base(nsp, name)
        {
        }
        public override void PostContactGround(Entity entity, Vector3 velocity)
        {
            base.PostContactGround(entity, velocity);
            var position = entity.Position;
            position.y = entity.Level.GetGroundY(position.x, position.y);
            if (!entity.Level.IsAirAt(position.x, position.z) && !entity.Level.IsWaterAt(position.x, position.z))
            {
                TinyBlaze.UpdateBlaze(entity.Level, position, entity, entity.GetSpawnParams());
            }
            entity.Remove();
        }
    }
}