using MVZ2.GameContent.Seeds;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine;
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
            AddModifier(new NamespaceIDModifier(VanillaLevelProps.PICKAXE_DISABLE_ID, PROP_DISABLE_ID_PICKAXE));
            AddModifier(new BooleanModifier(VanillaLevelProps.PICKAXE_DISABLE_ICON, PROP_DISABLE_ICON_PICKAXE));

            AddModifier(new NamespaceIDModifier(VanillaLevelProps.STARSHARD_DISABLE_ID, PROP_DISABLE_ID_STARSHARD));
            AddModifier(new BooleanModifier(VanillaLevelProps.STARSHARD_DISABLE_ICON, PROP_DISABLE_ICON_STARSHARD));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMEOUT, new FrameTimer(MAX_TIMEOUT));
            buff.SetProperty(PROP_DISABLE_ICON_PICKAXE, true);
            buff.SetProperty(PROP_DISABLE_ICON_STARSHARD, false);
            buff.SetProperty(PROP_DISABLE_ID_PICKAXE, VanillaBlueprintErrors.decrepify);
            buff.SetProperty<NamespaceID>(PROP_DISABLE_ID_STARSHARD, null);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timeout = buff.GetProperty<FrameTimer>(PROP_TIMEOUT);
            timeout.Run();
            bool disablePickaxe = buff.GetProperty<bool>(PROP_DISABLE_ICON_PICKAXE);
            bool disableShard = buff.GetProperty<bool>(PROP_DISABLE_ICON_STARSHARD);
            if (timeout.PassedInterval(15))
            {
                buff.SetProperty(PROP_DISABLE_ICON_PICKAXE, !disablePickaxe);
                buff.SetProperty(PROP_DISABLE_ICON_STARSHARD, !disableShard);
            }
            buff.SetProperty(PROP_DISABLE_ID_PICKAXE, disablePickaxe ? VanillaBlueprintErrors.decrepify : null);
            buff.SetProperty(PROP_DISABLE_ID_STARSHARD, disableShard ? VanillaBlueprintErrors.decrepify : null);
            if (timeout.Expired)
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<NamespaceID> PROP_DISABLE_ID_PICKAXE = new VanillaBuffPropertyMeta<NamespaceID>("DisableIDPickaxe");
        public static readonly VanillaBuffPropertyMeta<NamespaceID> PROP_DISABLE_ID_STARSHARD = new VanillaBuffPropertyMeta<NamespaceID>("DisableIDStarshard");
        public static readonly VanillaBuffPropertyMeta<bool> PROP_DISABLE_ICON_PICKAXE = new VanillaBuffPropertyMeta<bool>("DisableIconPickaxe");
        public static readonly VanillaBuffPropertyMeta<bool> PROP_DISABLE_ICON_STARSHARD = new VanillaBuffPropertyMeta<bool>("DisableIconStarshard");
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMEOUT = new VanillaBuffPropertyMeta<FrameTimer>("Timeout");
        public const int MAX_TIMEOUT = 1800;
    }
}
