using MVZ2.Vanilla.Entities;
using MVZ2Logic;
using MVZ2Logic.Artifacts;
using PVZEngine;
using PVZEngine.Entities;

namespace MVZ2.GameContent.Artifacts
{
    [ArtifactDefinition(VanillaArtifactNames.sweetSleepPillow)]
    public class SweetSleepPillow : ArtifactDefinition
    {
        public SweetSleepPillow(string nsp, string name) : base(nsp, name)
        {
        }
        public override void PostUpdate(Artifact artifact)
        {
            base.PostUpdate(artifact);
            artifact.SetGlowing(true);
            var level = artifact.Level;
            foreach (var contraption in level.GetEntities(EntityTypes.PLANT))
            {
                var healAmount = 0.33333333f;
                if (contraption.IsAIFrozen())
                    healAmount *= 3;
                contraption.HealEffects(healAmount, contraption);
            }
        }
        public static readonly NamespaceID ID = VanillaArtifactID.sweetSleepPillow;
    }
}
