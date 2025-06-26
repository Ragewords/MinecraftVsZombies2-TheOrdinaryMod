using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.snowball)]
    public class Snowball : ProjectileBehaviour, IHellfireIgniteBehaviour
    {
        public Snowball(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new NamespaceIDArrayModifier(VanillaProjectileProps.DAMAGE_EFFECTS, DMG_EFFECT));
        }
        public override void Init(Entity entity)
        {
            SetDMGEffects(entity, ICE_EFFECT);
        }
        public void Ignite(Entity entity, Entity hellfire, bool cursed)
        {
            entity.SetModelProperty("Melted", true);
            SetDMGEffects(entity, NONE_EFFECT);
        }
        public static void SetDMGEffects(Entity entity, NamespaceID[] value) => entity.SetBehaviourField(DMG_EFFECT, value);
        private static readonly VanillaEntityPropertyMeta<NamespaceID[]> DMG_EFFECT = new VanillaEntityPropertyMeta<NamespaceID[]>("DMGEffect");
        private static NamespaceID[] ICE_EFFECT = new NamespaceID[] { VanillaDamageEffects.ICE, VanillaDamageEffects.SLOW };
        private static NamespaceID[] NONE_EFFECT = new NamespaceID[] { };

    }
}
