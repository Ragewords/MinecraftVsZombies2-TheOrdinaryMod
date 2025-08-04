using MVZ2.GameContent.Seeds;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;

namespace MVZ2.GameContent.Buffs.Level
{
    [BuffDefinition(VanillaBuffNames.Level.nightmareDecrepifyAltered)]
    public class NightmareDecrepifyAlteredBuff : BuffDefinition
    {
        public NightmareDecrepifyAlteredBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new NamespaceIDModifier(VanillaLevelProps.PICKAXE_DISABLE_ID, VanillaBlueprintErrors.decrepify));
            AddModifier(new BooleanModifier(VanillaLevelProps.PICKAXE_DISABLE_ICON, PROP_DISABLE));

            AddModifier(new NamespaceIDModifier(VanillaLevelProps.STARSHARD_DISABLE_ID, VanillaBlueprintErrors.decrepify));
            AddModifier(new BooleanModifier(VanillaLevelProps.STARSHARD_DISABLE_ICON, PROP_DISABLE));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMEOUT, new FrameTimer(MAX_TIMEOUT));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timeout = buff.GetProperty<FrameTimer>(PROP_TIMEOUT);
            timeout.Run();
            if (timeout.PassedInterval(90))
            {
                buff.SetProperty(PROP_DISABLE, !buff.GetProperty<bool>(PROP_DISABLE));
            }
            if (timeout.Expired)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<bool> PROP_DISABLE = new VanillaBuffPropertyMeta<bool>("Disable");
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMEOUT = new VanillaBuffPropertyMeta<FrameTimer>("Timeout");
        public const int MAX_TIMEOUT = 1800;
    }
}
