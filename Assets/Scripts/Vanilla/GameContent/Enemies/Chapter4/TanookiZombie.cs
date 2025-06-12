using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.tanookiZombie)]
    public class TanookiZombie : MeleeEnemy
    {
        public TanookiZombie(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.PRE_ENTITY_TAKE_DAMAGE, PreEntityTakeDamageCallback);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetTakenDamage(entity, 0);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelDamagePercent();
            entity.SetAnimationBool("Stone", entity.HasBuff<TanookiZombieStoneBuff>());

            var damage = GetTakenDamage(entity);
            if (damage >= MAX_DAMAGE)
            {
                entity.AddBuff<TanookiZombieStoneBuff>();
                SetTakenDamage(entity, 0);
                var effect = entity.Level.Spawn(VanillaEffectID.smokeCluster, entity.GetCenter(), entity);
                effect.SetTint(new Color(0.5f, 0.5f, 0.5f, 1));
                effect.SetSize(entity.GetSize() * 2);
            }
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var damage = param.input;
            var entity = damage.Entity;
            AddTakenDamage(entity, damage.Amount);
        }
        public const float MAX_DAMAGE = 100;
        public const int STATE_CAST = VanillaEntityStates.GNAWED_STONE;
        public static float GetTakenDamage(Entity entity) => entity.GetProperty<float>(PROP_TAKEN_DAMAGE);
        public static void SetTakenDamage(Entity entity, float value) => entity.SetProperty(PROP_TAKEN_DAMAGE, value);
        public static void AddTakenDamage(Entity entity, float value) => SetTakenDamage(entity, GetTakenDamage(entity) + value);
        public static readonly VanillaEntityPropertyMeta<float> PROP_TAKEN_DAMAGE = new VanillaEntityPropertyMeta<float>("takenDamage");
    }
}
