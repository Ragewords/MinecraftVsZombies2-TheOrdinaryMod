using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
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
            imitate.Health = imitate.GetMaxHealth();
            if (target.IsEvoked())
                imitate.AddBuff<YoukaiLeafRegenerationBuff>();
            var effect = target.Level.Spawn(VanillaEffectID.smokeCluster, target.GetCenter(), target);
            effect.SetTint(new Color(0, 1, 0, 1));
            target.Remove();
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.SetEvoked(true);
            entity.PlaySound(VanillaSoundID.meow);
        }
    }
}
