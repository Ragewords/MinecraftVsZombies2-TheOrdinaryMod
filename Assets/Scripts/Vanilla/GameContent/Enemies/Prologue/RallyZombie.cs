using System.Linq;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.rallyZombie)]
    public class RallyZombie : FlagZombie
    {
        public RallyZombie(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var level = entity.Level;
            if (!level.IsIZombie())
            {
                for (var lane = 0; lane < entity.Level.GetMaxLaneCount(); lane++)
                {
                    var id = level.GetEnemyPool().Where(s => !spawnFilter.Contains(s)).Random(entity.RNG) ?? VanillaSpawnID.zombie;
                    var spawn = level.Content.GetSpawnDefinition(id);
                    var enemy = entity.Level.SpawnEnemy(spawn, lane);
                    if (enemy.IsEntityOf(VanillaEnemyID.undeadFlyingObject))
                    {
                        enemy.SetVariant(UndeadFlyingObject.VARIANT_RAINBOW);
                        UndeadFlyingObject.SetTargetGridX(enemy, entity.Level.GetMaxColumnCount() - 1);
                        UndeadFlyingObject.SetTargetGridY(enemy, lane);
                    }
                }
            }
        }
        public static readonly NamespaceID[] spawnFilter = new NamespaceID[]
        {
            VanillaSpawnID.zombie,
            VanillaSpawnID.leatherCappedZombie,
            VanillaSpawnID.ironHelmettedZombie
        };
    }
}
