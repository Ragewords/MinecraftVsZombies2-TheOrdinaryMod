using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.paratrooperZombie)]
    public class ParatrooperZombie : MeleeEnemy
    {
        public ParatrooperZombie(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var level = entity.Level;
            var lane = entity.GetLane();
            if (level.IsWaterLane(lane) || level.IsAirLane(lane))
            {
                entity.AddBuff<BoatBuff>();
                entity.SetModelProperty("HasBoat", true);
            }
            if (!entity.IsPreviewEnemy())
            {
                entity.AddBuff<ParatroopBuff>();
                entity.PlaySound(VanillaSoundID.wind, volume: 0.5f);
                SetTargetGridX(entity, entity.RNG.Next(3, 6));
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelDamagePercent();
            FindTargetGridAndSetSpeed(entity);
            entity.SetModelProperty("HasBoat", entity.HasBuff<BoatBuff>());
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
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
        private void FindTargetGridAndSetSpeed(Entity entity)
        {
            if (entity.HasBuff<ParatroopBuff>())
            {
                var targetPosition = GetTargetPosition(entity);
                var targetVelocity = targetPosition - entity.Position;
                targetVelocity = targetVelocity.normalized * Mathf.Min(MAX_MOVE_SPEED, targetVelocity.magnitude);
                var velocity = entity.Velocity;
                velocity.x = velocity.x * (1 - MOVE_FACTOR) + targetVelocity.x * MOVE_FACTOR;
                entity.Velocity = velocity;
            }
        }
        private static Vector3 GetTargetPosition(Entity enemy)
        {
            var level = enemy.Level;
            var column = GetTargetGridX(enemy);
            var lane = enemy.GetLane();
            var x = level.GetEntityColumnX(column);
            var z = level.GetEntityLaneZ(lane);
            var y = level.GetGroundY(x, z);
            return new Vector3(x, y, z);
        }
        public static int GetTargetGridX(Entity entity) => entity.GetBehaviourField<int>(PROP_TARGET_GRID_X);
        public static void SetTargetGridX(Entity entity, int value) => entity.SetBehaviourField(PROP_TARGET_GRID_X, value);
        public const float MAX_MOVE_SPEED = 10f;
        public const float MOVE_FACTOR = 0.7f;
        public static readonly VanillaEntityPropertyMeta<int> PROP_TARGET_GRID_X = new VanillaEntityPropertyMeta<int>("target_grid_x");
    }
}
