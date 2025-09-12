using System.Collections.Generic;
using MVZ2.GameContent.Armors;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.admiralZombie)]
    public class AdmiralZombie : MeleeEnemy
    {
        public AdmiralZombie(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var level = entity.Level;
            var lane = entity.GetLane();
            SetRNG(entity, entity.RNG);
            SetStateTimer(entity, new FrameTimer(CAST_COOLDOWN));
            if (level.IsWaterLane(lane) || level.IsAirLane(lane))
            {
                entity.AddBuff<BoatBuff>();
                entity.SetModelProperty("HasBoat", true);
            }
        }
        protected override int GetActionState(Entity enemy)
        {
            var state = base.GetActionState(enemy);
            if (state == VanillaEntityStates.WALK && IsCasting(enemy))
            {
                return STATE_CAST;
            }
            return state;
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelDamagePercent();
            entity.SetModelProperty("HasBoat", entity.HasBuff<BoatBuff>());
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);

            if (entity.IsDead)
                return;
            if (entity.State == VanillaEntityStates.ATTACK)
                return;
            var stateTimer = GetStateTimer(entity);
            if (entity.State == STATE_CAST)
            {
                stateTimer.Run(entity.GetAttackSpeed());
                if (stateTimer.Expired)
                {
                    EndCasting(entity);
                    SummonParatroopers(entity, 3);
                }
            }
            else
            {
                stateTimer.Run(entity.GetAttackSpeed());
                int childrenCount = GetAliveParatroopCount(entity);
                if (stateTimer.Expired)
                {
                    if (childrenCount >= MAX_PARATROOPERS_COUNT)
                    {
                        stateTimer.ResetTime(CALL_DETECT_TIME);
                    }
                    else
                    {
                        StartCasting(entity);
                    }
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (entity.State == STATE_CAST)
            {
                EndCasting(entity);
            }
            if (entity.HasBuff<BoatBuff>())
            {
                entity.RemoveBuffs<BoatBuff>();
                // 掉落碎船掉落物
                var effect = entity.Level.Spawn(VanillaEffectID.brokenArmor, entity.GetCenter(), entity);
                effect.Velocity = new Vector3(effect.RNG.NextFloat() * 20 - 10, 5, 0);
                effect.ChangeModel(VanillaModelID.boatItem);
                effect.SetDisplayScale(entity.GetDisplayScale());
            }
        }
        public static void SetCasting(Entity entity, bool timer) => entity.SetBehaviourField(ID, PROP_CASTING, timer);
        public static bool IsCasting(Entity entity) => entity.GetBehaviourField<bool>(ID, PROP_CASTING);
        public static void SetStateTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_STATE_TIMER, timer);
        public static FrameTimer GetStateTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);
        public static RandomGenerator GetRNG(Entity entity) => entity.GetBehaviourField<RandomGenerator>(ID, PROP_RNG);
        public static void SetRNG(Entity entity, RandomGenerator rng) => entity.SetBehaviourField(ID, PROP_RNG, rng);
        private void StartCasting(Entity entity)
        {
            SetCasting(entity, true);
            entity.PlaySound(VanillaSoundID.admiralCall);
            var stateTimer = GetStateTimer(entity);
            stateTimer.ResetTime(CAST_TIME);
        }

        private void EndCasting(Entity entity)
        {
            SetCasting(entity, false);
            var stateTimer = GetStateTimer(entity);
            stateTimer.ResetTime(CAST_COOLDOWN);
        }
        private void SummonParatroopers(Entity entity, int count)
        {
            List<LawnGrid> valid = new List<LawnGrid>();
            List<int> weights = new List<int>();
            var actualCount = Mathf.Max(Mathf.Min(MAX_PARATROOPERS_COUNT - GetAliveParatroopCount(entity), count), 1);

            var smallestColumn = Mathf.Max(entity.GetColumn() - 1, 0);
            var largestColumn = Mathf.Min(entity.GetColumn() + 1, entity.Level.GetMaxColumnCount() - 1);
            var smallestLane = Mathf.Max(entity.GetLane() - 1, 0);
            var largestLane = Mathf.Min(entity.GetLane() + 1, entity.Level.GetMaxLaneCount() - 1);
            for (int col = smallestColumn; col <= largestColumn; col++)
            {
                for (int lane = smallestLane; lane <= largestLane; lane++)
                {
                    var grid = entity.Level.GetGrid(col, lane);
                    valid.Add(grid);
                    weights.Add(Mathf.Abs(col - entity.GetColumn()) + 1);
                }
            }
            count = Mathf.Clamp(count, 0, Mathf.Min(valid.Count, actualCount));
            if (count <= 0)
                return;

            var rng = GetRNG(entity);
            var grids = valid.WeightedRandomTake(weights.ToArray(), count, rng);
            foreach (var grid in grids)
            {
                var entityToSpawn = paratroopsToSpawn.Random(rng);
                var paratrooper = SpawnParatroopOnGrid(entity, entityToSpawn, grid);
                paratrooper.SetParent(entity);
            }
            entity.Level.PlaySound(VanillaSoundID.wind);
        }
        public static Entity SpawnParatroopOnGrid(Entity entity, NamespaceID enemyID, LawnGrid grid)
        {
            var position = grid.GetEntityPosition() + Vector3.up * 600;
            var paratrooper = entity.SpawnWithParams(enemyID, position);
            paratrooper.EquipArmorTo(VanillaArmorSlots.shield, VanillaArmorID.umbrellaShield);
            paratrooper.AddBuff<ParatroopBuff>();
            return paratrooper;
        }
        public static int GetAliveParatroopCount(Entity entity)
        {
            var children = entity.GetChildren();
            int childrenCount = 0;
            foreach (var child in children)
            {
                if (child.ExistsAndAlive())
                {
                    childrenCount++;
                }
            }
            return childrenCount;
        }
        #region 常量
        public const int STATE_CAST = VanillaEntityStates.ADMIRAL_ZOMBIE_CALL;
        public const int CAST_COOLDOWN = 300;
        public const int CAST_TIME = 75;
        public const int CALL_DETECT_TIME = 60;
        public const int MAX_PARATROOPERS_COUNT = 5;
        public static readonly NamespaceID[] paratroopsToSpawn = new NamespaceID[]
        {
            VanillaEnemyID.zombie,
            VanillaEnemyID.leatherCappedZombie,
            VanillaEnemyID.ironHelmettedZombie
        };
        public static readonly NamespaceID ID = VanillaEnemyID.admiralZombie;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_CASTING = new VanillaEntityPropertyMeta<bool>("Casting");
        public static readonly VanillaLevelPropertyMeta<RandomGenerator> PROP_RNG = new VanillaLevelPropertyMeta<RandomGenerator>("SpawnerRNG");
        #endregion 常量
    }
}
