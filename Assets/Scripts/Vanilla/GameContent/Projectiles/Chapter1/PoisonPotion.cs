using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.poisonPotion)]
    public class PoisonPotion : ProjectileBehaviour
    {
        public PoisonPotion(string nsp, string name) : base(nsp, name)
        {
        }
        public override void PostDeath(Entity entity, DeathInfo damageInfo)
        {
            base.PostDeath(entity, damageInfo);
            if (damageInfo.Effects.HasEffect(VanillaDamageEffects.NO_DEATH_TRIGGER))
                return;

            entity.PlaySound(VanillaSoundID.glassBreak);
            entity.PlaySound(VanillaSoundID.poisonGas);
            entity.SpawnWithParams(VanillaProjectileID.poisonGas, entity.Position);
        }
    }
}
