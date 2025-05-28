using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Models;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Models;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.poisoned)]
    public class PoisonedBuff : BuffDefinition
    {
        public PoisonedBuff(string nsp, string name) : base(nsp, name)
        {
            AddModelInsertion(LogicModelHelper.ANCHOR_CENTER, VanillaModelKeys.witherParticles, VanillaModelID.witherParticles);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);

            var entity = buff.GetEntity();
            if (entity != null)
            {
                if (entity.Health > 20)
                    entity.TakeDamageNoSource(WITHER_DAMAGE, new DamageEffectList(VanillaDamageEffects.DAMAGE_BOTH_ARMOR_AND_BODY, VanillaDamageEffects.MUTE));
                else if (entity.GetMainArmor() != null)
                    entity.GetMainArmor().TakeDamage(WITHER_DAMAGE, new DamageEffectList(VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE), null);
            }

            var timeout = buff.GetProperty<int>(PROP_TIMEOUT);
            timeout--;
            buff.SetProperty(PROP_TIMEOUT, timeout);
            if (timeout <= 0)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<int> PROP_TIMEOUT = new VanillaBuffPropertyMeta<int>("Timeout");
        public const float WITHER_DAMAGE = 2 / 3f;
    }
}
