﻿using MVZ2.GameContent.Buffs;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.dullahanHead)]
    public class DullahanHead : MeleeEnemy
    {
        public DullahanHead(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.POST_ENTITY_CHARM, PostEntityCharmCallback);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 30);
        }
        public override void PreTakeDamage(DamageInput input)
        {
            base.PreTakeDamage(input);
            if (input.Effects.HasEffect(VanillaDamageEffects.GOLD))
            {
                input.Multiply(3);
            }
        }
        private void PostEntityCharmCallback(Entity entity, Buff buff)
        {
            if (!entity.IsEntityOf(VanillaEnemyID.dullahanHead))
                return;
            var body = GetBody(entity);
            if (!body.ExistsAndAlive())
                return;
            body.Die();
        }
        public static Entity GetBody(Entity entity)
        {
            var entityID = entity.GetBehaviourField<EntityID>(ID, FIELD_BODY);
            return entityID?.GetEntity(entity.Level);
        }
        public static void SetBody(Entity entity, Entity value)
        {
            entity.SetBehaviourField(ID, FIELD_BODY, new EntityID(value));
        }
        public static readonly VanillaEntityPropertyMeta FIELD_BODY = new VanillaEntityPropertyMeta("Body");
        private static readonly NamespaceID ID = VanillaEnemyID.dullahanHead;
    }
}
