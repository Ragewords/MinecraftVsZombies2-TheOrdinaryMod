using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Models;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Models;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.bleeding)]
    public class BleedingBuff : BuffDefinition
    {
        public BleedingBuff(string nsp, string name) : base(nsp, name)
        {
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMEOUT, new FrameTimer(150));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);

            var entity = buff.GetEntity();
            var timeout = buff.GetProperty<FrameTimer>(PROP_TIMEOUT);
            timeout.Run();
            
            if (timeout.PassedInterval(5))
            {
                if (entity != null)
                {
                    entity.TakeDamage(WITHER_DAMAGE, new DamageEffectList(VanillaDamageEffects.SLICE, VanillaDamageEffects.IGNORE_ARMOR, VanillaDamageEffects.MUTE), entity);
                }
            }

            buff.SetProperty(PROP_TIMEOUT, timeout);
            if (timeout.Expired)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMEOUT = new VanillaBuffPropertyMeta<FrameTimer>("Timeout");
        public const float WITHER_DAMAGE = 1f;
    }
}
