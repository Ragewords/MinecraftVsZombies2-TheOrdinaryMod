using System;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Enemies;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.necrotombstone)]
    public class Necrotombstone : ContraptionBehaviour
    {
        public Necrotombstone(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetProductionTimer(entity, new FrameTimer(SPAWN_INTERVAL));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            ProductionUpdate(entity);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var pos = entity.Position;
            pos.y = entity.GetGroundY() - 100;
            var mageClass = SkeletonMage.mageClasses.Random(entity.RNG);
            var grid = entity.GetGrid();
            var lane = grid.Lane;
            for (int i = -1; i <= 1; i++)
            {
                var mage = entity.SpawnWithParams(VanillaEnemyID.skeletonMage, pos + new Vector3(80 * i, 0));
                MageUpdate(mage, mageClass);
            }
            if (lane != 0)
            {
                var mage = entity.SpawnWithParams(VanillaEnemyID.skeletonMage, pos + new Vector3(0, 0, 80));
                MageUpdate(mage, mageClass);
            }
            if (lane != entity.Level.GetMaxLaneCount() - 1)
            {
                var mage = entity.SpawnWithParams(VanillaEnemyID.skeletonMage, pos - new Vector3(0, 0, 80));
                MageUpdate(mage, mageClass);
            }
        }
        public static FrameTimer GetProductionTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_PRODUCTION_TIMER);
        public static void SetProductionTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_PRODUCTION_TIMER, timer);
        private void ProductionUpdate(Entity entity)
        {
            var productionTimer = GetProductionTimer(entity);
            if (!entity.Level.IsCleared)
            {
                productionTimer.Run(entity.GetProduceSpeed());
            }
            if (productionTimer.Expired)
            {
                if (SkeletonOutOfLimit(entity))
                {
                    productionTimer.Frame = RECHECK_INTERVAL;
                }
                else
                {
                    var pos = entity.Position;
                    pos.y = entity.GetGroundY() - 100;
                    var enemyID = GetRandomProductionEnemyID(entity.RNG);
                    var warrior = entity.SpawnWithParams(enemyID, pos);
                    warrior.AddBuff<NecrotombstoneRisingBuff>();
                    warrior.UpdateModel();

                    warrior.PlaySound(VanillaSoundID.dirtRise);

                    productionTimer.ResetTime(SPAWN_INTERVAL);
                }
            }
        }
        private bool SkeletonOutOfLimit(Entity entity)
        {
            return entity.Level.GetEntityCount(VanillaEnemyID.skeletonWarrior) >= SKELETON_LIMIT;
        }
        private void MageUpdate(Entity mage, int mageClass)
        {
            SkeletonMage.SetClass(mage, mageClass);
            mage.AddBuff<NecrotombstoneRisingBuff>();
            mage.UpdateModel();

            mage.PlaySound(VanillaSoundID.dirtRise);
            mage.PlaySound(VanillaSoundID.boneWallBuild);
        }
        private NamespaceID GetRandomProductionEnemyID(RandomGenerator rng)
        {
            var index = rng.WeightedRandom(productionPoolWeights);
            return productionPool[index];
        }
        private static NamespaceID[] productionPool = new NamespaceID[]
        {
            VanillaEnemyID.skeleton,
            VanillaEnemyID.ghost,
            VanillaEnemyID.necromancer,
            VanillaEnemyID.skelebomb,
            VanillaEnemyID.skeletonHorse,
            VanillaEnemyID.skeletonWarrior
        };
        private static int[] productionPoolWeights = new int[]
        {
            2,
            2,
            1,
            3,
            4,
            10
        };
        public const int SKELETON_LIMIT = 30;
        public const int SPAWN_INTERVAL = 450;
        public const int RECHECK_INTERVAL = 60;
        public const int MAGE_COUNT = 3;
        private static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_PRODUCTION_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("ProductionTimer");
    }
}
