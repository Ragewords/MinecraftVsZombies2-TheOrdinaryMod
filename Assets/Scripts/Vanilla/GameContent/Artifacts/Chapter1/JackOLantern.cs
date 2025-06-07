using System.Collections.Generic;
using MVZ2.GameContent.Buffs;
using MVZ2.Vanilla.Entities;
using MVZ2Logic;
using MVZ2Logic.Artifacts;
using MVZ2Logic.Level;
using PVZEngine.Auras;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;

namespace MVZ2.GameContent.Artifacts
{
    [ArtifactDefinition(VanillaArtifactNames.jackOLantern)]
    public class JackOLantern : ArtifactDefinition
    {
        public JackOLantern(string nsp, string name) : base(nsp, name)
        {
            AddAura(new Aura());
            AddTrigger(LevelCallbacks.POST_ENTITY_INIT, PostContraptionInitCallback, filter: EntityTypes.PLANT);
        }
        public override void PostUpdate(Artifact artifact)
        {
            base.PostUpdate(artifact);
            artifact.SetGlowing(true);
        }
        private void PostContraptionInitCallback(EntityCallbackParams param, CallbackResult result)
        {
            var contraption = param.entity;
            if (contraption.Definition.IsLightSource())
                return;
            var level = contraption.Level;
            var lantern = level.GetArtifact(GetID());
            if (lantern == null)
                return;
            var aura = lantern.GetAuraEffect<Aura>();
            if (aura == null)
                return;
            aura.UpdateAura();
        }
        public class Aura : AuraEffectDefinition
        {
            public Aura()
            {
                BuffID = VanillaBuffID.jackOLantern;
                UpdateInterval = 4;
            }

            public override void GetAuraTargets(AuraEffect auraEffect, List<IBuffTarget> results)
            {
                var level = auraEffect.Source.GetLevel();
                updateBuffer.Clear();
                level.FindEntitiesNonAlloc(e => !e.Definition.IsLightSource() && e.Type == EntityTypes.PLANT, updateBuffer);
                foreach (var entity in updateBuffer)
                {
                    results.Add(entity);
                }
            }
            private List<Entity> updateBuffer = new List<Entity>();
        }
    }
}
