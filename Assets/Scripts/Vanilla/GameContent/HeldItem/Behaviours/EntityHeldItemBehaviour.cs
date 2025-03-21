﻿using MVZ2.HeldItems;
using MVZ2Logic;
using MVZ2Logic.HeldItems;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.SeedPacks;

namespace MVZ2.GameContent.HeldItems
{
    [HeldItemDefinition(VanillaHeldItemNames.entity)]
    public class EntityHeldItemBehaviour : HeldItemBehaviour
    {
        public EntityHeldItemBehaviour(HeldItemDefinition definition) : base(definition)
        {
        }
        public override bool IsValidFor(HeldItemTarget target, IHeldItemData data)
        {
            var entity = GetEntity(target.GetLevel(), data.ID);
            var behaviour = GetBehaviour(entity);
            return behaviour?.CheckRaycast(entity, target, data) ?? false;
        }
        public override HeldHighlight GetHighlight(HeldItemTarget target, IHeldItemData data)
        {
            var entity = GetEntity(target.GetLevel(), data.ID);
            var behaviour = GetBehaviour(entity);
            return behaviour?.GetHighlight(entity, target, data) ?? HeldHighlight.None;
        }
        public override void Use(HeldItemTarget target, IHeldItemData data, PointerInteraction interaction)
        {
            var entity = GetEntity(target.GetLevel(), data.ID);
            var behaviour = GetBehaviour(entity);
            behaviour?.Use(entity, target, data, interaction);
        }
        public static Entity GetEntity(LevelEngine level, long id)
        {
            return level.FindEntityByID(id);
        }
        public static IHeldEntityBehaviour GetBehaviour(Entity entity)
        {
            if (entity == null)
                return null;
            return entity.Definition.GetBehaviour<IHeldEntityBehaviour>();
        }
    }
}
