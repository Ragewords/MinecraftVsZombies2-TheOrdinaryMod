using System.Linq;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Grids;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.youkaiLeaf)]
    public class YoukaiLeaf : ContraptionBehaviour
    {
        public YoukaiLeaf(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.POST_ENEMY_MELEE_ATTACK, PostEnemyMeleeAttackCallback);
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelProperty("Evoked", entity.IsEvoked());
        }
        private void PostEnemyMeleeAttackCallback(VanillaLevelCallbacks.EnemyMeleeAttackParams param, CallbackResult result)
        {
            var enemy = param.enemy;
            var target = param.target;
            if (!target.IsEntityOf(VanillaContraptionID.youkaiLeaf))
                return;
            if (!target.IsHostile(enemy))
                return;
            if (target.IsAIFrozen())
                return;
            var imitate = target.SpawnWithParams(enemy.GetDefinitionID(), target.Position);
            imitate.AddBuff<YoukaiLeafBuff>();
            var effect = target.Level.Spawn(VanillaEffectID.smokeCluster, target.GetCenter(), target);
            effect.SetTint(new Color(0, 1, 0, 1));
            target.Remove();
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.PlaySound(VanillaSoundID.meow);
            entity.PlaySound(VanillaSoundID.dataCopy);
            var enemies = entity.Level.FindEntities(e => e.Type == EntityTypes.ENEMY && !e.IsNotActiveEnemy() && entity.GetGrid().CanSpawnEntity(e.GetDefinitionID())).RandomTake(3, entity.RNG);
            if (enemies.Count() <= 0)
                return;
            var imitate_number = enemies.Count();
            var choose_imitate = enemies.Random(entity.RNG);
            foreach (var enemy in enemies)
            {
                var imitate = entity.SpawnWithParams(enemy.GetDefinitionID(), entity.Position);
                imitate.AddBuff<YoukaiLeafBuff>();
            }
            if (imitate_number < 3)
            {
                for (var i = 0; i < 3 - imitate_number; i++)
                {
                    var imitate = entity.SpawnWithParams(choose_imitate.GetDefinitionID(), entity.Position);
                    imitate.AddBuff<YoukaiLeafBuff>();
                }
            }
            var effect = entity.Spawn(VanillaEffectID.smokeCluster, entity.GetCenter());
            effect.SetTint(new Color(0, 1, 0, 1));
            entity.Remove();
        }
    }
}
