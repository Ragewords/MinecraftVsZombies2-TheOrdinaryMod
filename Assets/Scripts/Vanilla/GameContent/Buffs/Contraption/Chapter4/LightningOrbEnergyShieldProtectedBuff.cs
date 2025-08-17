using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.lightningOrbEnergyShieldProtected)]
    public class LightningOrbEnergyShieldProtectedBuff : BuffDefinition
    {
        public LightningOrbEnergyShieldProtectedBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new BooleanModifier(VanillaEntityProps.INVISIBLE, true));
            AddModifier(new BooleanModifier(EngineEntityProps.INVINCIBLE, true));
            AddModifier(new BooleanModifier(VanillaEntityProps.ETHEREAL, true));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMER, new FrameTimer(5));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timer = buff.GetProperty<FrameTimer>(PROP_TIMER);
            if (timer == null || timer.Expired)
                buff.Remove();
            else
                timer.Run();
        }
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("timer");
    }
}
