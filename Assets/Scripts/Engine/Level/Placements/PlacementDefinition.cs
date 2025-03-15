﻿using PVZEngine.Base;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Grids;
using PVZEngine.Triggers;

namespace PVZEngine.Placements
{
    public abstract class PlacementDefinition : Definition
    {
        public PlacementDefinition(string nsp, string name) : base(nsp, name)
        {
        }
        public virtual void CanPlaceEntityOnGrid(LawnGrid grid, EntityDefinition entity, TriggerResultNamespaceID error)
        {
        }
        public sealed override string GetDefinitionType() => EngineDefinitionTypes.PLACEMENT;
    }
}
