using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

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
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();
            if (entity != null)
            {
                var orbID = GetOrbID(buff);
                var orb = orbID.GetEntity(buff.Level);
                if (!orb.ExistsAndAlive())
                {
                    buff.Remove();
                    return;
                }
            }
        }
        public static EntityID GetOrbID(Buff buff) => buff.GetProperty<EntityID>(PROP_ORB);
        public static void SetOrbID(Buff buff, EntityID value) => buff.SetProperty(PROP_ORB, value);
        public static readonly VanillaBuffPropertyMeta<EntityID> PROP_ORB = new VanillaBuffPropertyMeta<EntityID>("orb");
    }
}
