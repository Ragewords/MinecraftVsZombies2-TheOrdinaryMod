using System.Collections.Generic;
using MVZ2.GameContent.Armors;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Enemies;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Areas
{
    [AreaDefinition(VanillaAreaNames.ship)]
    public class Ship : AreaDefinition
    {
        public Ship(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(LevelCallbacks.POST_WAVE, PostWave);
        }
        public override void Setup(LevelEngine level)
        {
            base.Setup(level);
            SetSkyOffsetSpeed(level, SKY_OFFSET_SPEED_NORMAL);
            SetRNG(level, level.CreateRNG());
            SetBreezeRNG(level, level.CreateRNG());
        }
        public override void Update(LevelEngine level)
        {
            base.Update(level);
            var skyOffsetSpeed = GetSkyOffsetSpeed(level);
            var targetSpeed = SKY_OFFSET_SPEED_NORMAL;
            if (level.IsDuringHugeWave())
            {
                targetSpeed = SKY_OFFSET_SPEED_FAST;
            }
            var accel = (targetSpeed - skyOffsetSpeed) * SKY_OFFSET_ACCELERATION;
            if (skyOffsetSpeed != targetSpeed)
            {
                if (skyOffsetSpeed < targetSpeed == skyOffsetSpeed + accel > targetSpeed)
                {
                    skyOffsetSpeed = targetSpeed;
                }
                else
                {
                    skyOffsetSpeed += accel;
                }
            }
            SetSkyOffsetSpeed(level, skyOffsetSpeed);
            level.SetModelAnimatorFloat("SkyOffsetSpeed", skyOffsetSpeed);

            var breezeSpeed = GetBreezeSpeed(level);
            var nextSpeed = GetNextBreezeSpeed(level) * BREEZE_OFFSET_MULTIPILER * (level.IsDuringHugeWave() ? 2 : 1);
            var breezeAccel = (nextSpeed - breezeSpeed) * BREEZE_OFFSET_ACCELERATION;
            if (skyOffsetSpeed != targetSpeed)
            {
                if (breezeSpeed < nextSpeed == breezeSpeed + breezeAccel > nextSpeed)
                {
                    breezeSpeed = nextSpeed;
                }
                else
                {
                    breezeSpeed += breezeAccel;
                }
            }
            SetBreezeSpeed(level, breezeSpeed);
            BlowEntities(level, breezeSpeed);
            level.SetModelAnimatorFloat("BreezeOffsetSpeed", breezeSpeed);
        }
        public override void PostHugeWaveEvent(LevelEngine level)
        {
            base.PostHugeWaveEvent(level);
            SpawnParatroops(level, 3);
        }
        public static void SpawnParatroops(LevelEngine level, int count)
        {
            List<LawnGrid> valid = new List<LawnGrid>();
            List<int> weights = new List<int>();

            for (int col = SPAWNER_MIN_COLUMN; col < level.GetMaxColumnCount(); col++)
            {
                for (int lane = 0; lane < level.GetMaxLaneCount(); lane++)
                {
                    var grid = level.GetGrid(col, lane);
                    valid.Add(grid);
                    weights.Add(GetParatroopWeight(col));
                }
            }
            count = Mathf.Clamp(count, 0, valid.Count);
            if (count <= 0)
                return;

            var rng = GetRNG(level);
            var grids = valid.WeightedRandomTake(weights.ToArray(), count, rng);
            foreach (var grid in grids)
            {
                var entityToSpawn = GetParatroopToSpawn(rng);
                SpawnParatroopOnGrid(level, entityToSpawn, grid);
            }
            level.PlaySound(VanillaSoundID.wind);
        }
        public static Entity SpawnParatroopOnGrid(LevelEngine level, NamespaceID enemyID, LawnGrid grid)
        {
            var position = grid.GetEntityPosition() + Vector3.up * 600;
            var entity = level.Spawn(enemyID, position, null);
            entity.EquipArmorTo(VanillaArmorSlots.shield, VanillaArmorID.umbrellaShield);
            entity.AddBuff<ParatroopBuff>();
            return entity;
        }
        private static int GetParatroopWeight(int column)
        {
            return column - SPAWNER_MIN_COLUMN + 1;
        }
        private static NamespaceID GetParatroopToSpawn(RandomGenerator rng)
        {
            return paratroopsToSpawn.Random(rng);
        }
        private void PostWave(LevelCallbacks.PostWaveParams param, CallbackResult result)
        {
            var level = param.level;
            var wave = param.wave;
            if (level.AreaDefinition != this)
                return;
            var rng = GetBreezeRNG(level);
            SetNextBreezeSpeed(level, rng.Next(-0.3f, 0.2f));
        }
        private void BlowEntities(LevelEngine level, float speed)
        {
            foreach (var enemy in level.FindEntities(e => e.Type == EntityTypes.ENEMY))
            {
                if (enemy.State != VanillaEntityStates.ATTACK && enemy.State != VanillaEntityStates.ENEMY_PARACHUTE)
                    enemy.Position += ENEMY_BLOW_MULTIPILER * speed * Vector3.left;
            }
            foreach (var projectile in level.FindEntities(e => e.Type == EntityTypes.PROJECTILE))
            {
                if (projectile.Velocity.magnitude < 30)
                    projectile.Velocity += PROJECTILE_BLOW_MULTIPILER * speed * Vector3.left;
            }
        }
        public static float GetSkyOffsetSpeed(LevelEngine level) => level.GetProperty<float>(PROP_SKY_OFFSET_SPEED);
        public static void SetSkyOffsetSpeed(LevelEngine level, float value) => level.SetProperty<float>(PROP_SKY_OFFSET_SPEED, value);
        public static float GetBreezeSpeed(LevelEngine level) => level.GetProperty<float>(PROP_BREEZE_SPEED);
        public static void SetBreezeSpeed(LevelEngine level, float value) => level.SetProperty<float>(PROP_BREEZE_SPEED, value);
        public static float GetNextBreezeSpeed(LevelEngine level) => level.GetProperty<float>(PROP_NEXT_BREEZE_SPEED);
        public static void SetNextBreezeSpeed(LevelEngine level, float value) => level.SetProperty<float>(PROP_NEXT_BREEZE_SPEED, value);
        public static RandomGenerator GetRNG(LevelEngine level) => level.GetBehaviourField<RandomGenerator>(PROP_RNG);
        public static void SetRNG(LevelEngine level, RandomGenerator rng) => level.SetBehaviourField(PROP_RNG, rng);
        public static RandomGenerator GetBreezeRNG(LevelEngine level) => level.GetBehaviourField<RandomGenerator>(PROP_BREEZE_RNG);
        public static void SetBreezeRNG(LevelEngine level, RandomGenerator rng) => level.SetBehaviourField(PROP_BREEZE_RNG, rng);

        public static readonly NamespaceID[] paratroopsToSpawn = new NamespaceID[]
        {
            VanillaEnemyID.zombie,
            VanillaEnemyID.leatherCappedZombie,
            VanillaEnemyID.ironHelmettedZombie
        };
        public const int SPAWNER_MIN_COLUMN = 5;
        public const float SKY_OFFSET_SPEED_NORMAL = 1;
        public const float SKY_OFFSET_SPEED_FAST = 10;
        public const float SKY_OFFSET_ACCELERATION = 0.1f;
        public const float ENEMY_BLOW_MULTIPILER = 0.05f;
        public const float PROJECTILE_BLOW_MULTIPILER = 0.025f;
        public const float BREEZE_OFFSET_ACCELERATION = 0.01f;
        public const float BREEZE_OFFSET_MULTIPILER = 10f;
        public static readonly VanillaLevelPropertyMeta<RandomGenerator> PROP_RNG = new VanillaLevelPropertyMeta<RandomGenerator>("SpawnerRNG");
        public static readonly VanillaLevelPropertyMeta<float> PROP_SKY_OFFSET_SPEED = new VanillaLevelPropertyMeta<float>("sky_offset_speed");
        public static readonly VanillaLevelPropertyMeta<float> PROP_BREEZE_SPEED = new VanillaLevelPropertyMeta<float>("breeze_speed");
        public static readonly VanillaLevelPropertyMeta<float> PROP_NEXT_BREEZE_SPEED = new VanillaLevelPropertyMeta<float>("next_breeze_speed");
        public static readonly VanillaLevelPropertyMeta<RandomGenerator> PROP_BREEZE_RNG = new VanillaLevelPropertyMeta<RandomGenerator>("BreezeRNG");
    }
}