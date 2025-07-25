using MVZ2.GameContent.Armors;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.brainwasher)]
    public class Brainwasher : Mesmerizer
    {
        public Brainwasher(string nsp, string name) : base(nsp, name)
        {
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.NO_DEATH_TRIGGER))
                return;
            entity.Level.Spawn(VanillaEffectID.brainwasherExplosion, entity.GetCenter(), entity);
            var targets = entity.Level.FindEntities(e => e.IsHostile(entity) && e.Type == EntityTypes.PLANT && e.GetDefinitionID() != VanillaContraptionID.glowstone).RandomTake(5, entity.RNG);
            foreach (var target in targets)
            {
                target.Charm(entity.GetFaction());
                entity.PlaySound(VanillaSoundID.mindControl);
            }
            var targets_enemy = entity.Level.FindEntities(e => e.IsFriendly(entity) && e.Type == EntityTypes.ENEMY).RandomTake(10, entity.RNG);
            foreach (var target in targets_enemy)
            {
                target.Mesmerize();
                entity.PlaySound(VanillaSoundID.mindControl);
            }
        }
    }
}
