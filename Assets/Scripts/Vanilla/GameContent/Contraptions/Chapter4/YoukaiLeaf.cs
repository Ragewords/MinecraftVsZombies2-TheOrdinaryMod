using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
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
            detector = new LawnDetector();
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelProperty("Evoked", entity.IsEvoked());

            detectBuffer.Clear();
            var collider = detector.DetectWithTheMost(entity, e => e.Entity.Health);

            var target = collider?.Entity;
            if (target == null)
                return;
            if (target.Type != EntityTypes.ENEMY)
                return;
            var enemy = entity.SpawnWithParams(target.GetDefinitionID(), entity.Position);
            enemy.AddBuff<YoukaiLeafBuff>();
            enemy.Health = enemy.GetMaxHealth();
            if (entity.IsEvoked())
                enemy.AddBuff<YoukaiLeafRegenerationBuff>();
            entity.Remove();
            var effect = entity.Level.Spawn(VanillaEffectID.smokeCluster, entity.GetCenter(), entity);
            effect.SetTint(new Color(0, 1, 0, 1));
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.SetEvoked(true);
            entity.PlaySound(VanillaSoundID.meow);
        }
        private Detector detector;
        private List<Entity> detectBuffer = new List<Entity>();
    }
}
