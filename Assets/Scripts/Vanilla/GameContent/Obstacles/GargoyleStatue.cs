﻿using System.Collections.Generic;
using System.Linq;
using MVZ2.GameContent.Buffs;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Obstacles
{
    [EntityBehaviourDefinition(VanillaObstacleNames.gargoyleStatue)]
    public class GargoyleStatue : ObstacleBehaviour
    {
        public GargoyleStatue(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.AddBuff<TemporaryUpdateBeforeGameBuff>();
            entity.TriggerAnimation("Rise");
            entity.PlaySound(VanillaSoundID.dirtRise);
            entity.UpdateTakenGrids();

            entity.InitFragment();

            var grid = entity.GetGrid();
            var statueTakenLayers = new List<NamespaceID>();
            entity.GetTakingGridLayers(grid, statueTakenLayers);
            var entityTakenLayers = new List<NamespaceID>();
            foreach (var contraption in entity.Level.FindEntities(e => e.Type == EntityTypes.PLANT && e.GetGrid() == grid))
            {
                entityTakenLayers.Clear();
                contraption.GetTakingGridLayers(grid, entityTakenLayers);
                if (!entityTakenLayers.Any(l => statueTakenLayers.Contains(l)))
                    continue;
                contraption.Die();
            }
        }
        public override void PostTakeDamage(DamageOutput result)
        {
            base.PostTakeDamage(result);
            var bodyResult = result.BodyResult;
            if (bodyResult != null)
            {
                bodyResult.Entity.AddFragmentTickDamage(bodyResult.Amount);
            }
        }
        public override void PostDeath(Entity entity, DeathInfo damageInfo)
        {
            base.PostDeath(entity, damageInfo);
            entity.PostFragmentDeath(damageInfo);
            entity.Remove();
        }
        public override void PostRemove(Entity entity)
        {
            base.PostRemove(entity);
            entity.ClearTakenGrids();
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            entity.SetAnimationInt("HealthState", entity.GetHealthState(5));

            entity.UpdateTakenGrids();

            entity.UpdateFragment();
        }
    }
}
