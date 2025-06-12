using System.Collections.Generic;
using MVZ2.GameContent.Buffs;
using MVZ2Logic;
using MVZ2Logic.Artifacts;
using PVZEngine.Auras;
using PVZEngine.Buffs;
using PVZEngine.Entities;

namespace MVZ2.GameContent.Artifacts
{
    [ArtifactDefinition(VanillaArtifactNames.infinityDoor)]
    public class InfinityDoor : ArtifactDefinition
    {
        public InfinityDoor(string nsp, string name) : base(nsp, name)
        {
            AddAura(new Aura());
        }
        public override void PostUpdate(Artifact artifact)
        {
            base.PostUpdate(artifact);
            artifact.SetGlowing(true);
        }
        public class Aura : AuraEffectDefinition
        {
            public Aura()
            {
                BuffID = VanillaBuffID.infinityDoor;
            }
            public override void GetAuraTargets(AuraEffect auraEffect, List<IBuffTarget> results)
            {
                var level = auraEffect.Source.GetLevel();
                results.AddRange(level.GetEntities(EntityTypes.PROJECTILE));
            }
        }
    }
}


