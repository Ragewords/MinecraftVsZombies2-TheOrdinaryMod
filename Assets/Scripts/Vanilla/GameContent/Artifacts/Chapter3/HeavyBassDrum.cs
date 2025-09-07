using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2Logic;
using MVZ2Logic.Artifacts;
using MVZ2Logic.Level;
using PVZEngine.Callbacks;

namespace MVZ2.GameContent.Artifacts
{
    [ArtifactDefinition(VanillaArtifactNames.heavyBassDrum)]
    public class HeavyBassDrum : ArtifactDefinition
    {
        public HeavyBassDrum(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.POST_BODY_TAKE_DAMAGE, PostBodyTakeDamageCallback);
            AddTrigger(VanillaLevelCallbacks.POST_ARMOR_TAKE_DAMAGE, PostArmorTakeDamageCallback);
        }
        public override void PostUpdate(Artifact artifact)
        {
            base.PostUpdate(artifact);
            artifact.SetGlowing(true);
            AddArtifactTime();
        }
        private void PostBodyTakeDamageCallback(VanillaLevelCallbacks.PostBodyTakeDamageParams param, CallbackResult result)
        {
            var entity = param.output?.Entity;
            if (entity == null)
                return;
            if (!entity.IsHostileEntity())
                return;
            var level = entity.Level;
            var artifacts = level.GetArtifacts();
            bool valid = false;
            foreach (var artifact in artifacts)
            {
                if (artifact == null)
                    continue;
                if (artifact.Definition != this)
                    continue;
                if (!IsTimeInterval(60))
                    continue;
                artifact.Highlight();
                valid = true;
            }
            if (valid && entity.CanDeactive())
            {
                entity.Stun(30);
                entity.PlaySound(VanillaSoundID.punch);
            }
        }
        private void PostArmorTakeDamageCallback(VanillaLevelCallbacks.PostArmorTakeDamageParams param, CallbackResult result)
        {
            var entity = param.result?.Entity;
            if (entity == null)
                return;
            if (!entity.IsHostileEntity())
                return;
            var level = entity.Level;
            var artifacts = level.GetArtifacts();
            bool valid = false;
            foreach (var artifact in artifacts)
            {
                if (artifact == null)
                    continue;
                if (artifact.Definition != this)
                    continue;
                if (!IsTimeInterval(60))
                    continue;
                artifact.Highlight();
                valid = true;
            }
            if (valid && entity.CanDeactive())
            {
                entity.Stun(30);
                entity.PlaySound(VanillaSoundID.punch);
            }
        }
        private void AddArtifactTime()
        {
            artifactTime++;
        }
        private bool IsTimeInterval(long interval, long offset = 0)
        {
            return artifactTime % interval == offset;
        }
        private long artifactTime = 0;
    }
}
