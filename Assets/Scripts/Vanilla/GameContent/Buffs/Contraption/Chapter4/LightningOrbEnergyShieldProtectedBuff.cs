using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.lightningOrbEnergyShieldProtected)]
    public class LightningOrbEnergyShieldProtectedBuff : BuffDefinition
    {
        public LightningOrbEnergyShieldProtectedBuff(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.PRE_PROJECTILE_HIT, PreProjectileHitCallback);
            AddTrigger(VanillaLevelCallbacks.PRE_ENTITY_TAKE_DAMAGE, PreEntityTakeDamageCallback);
        }
        private void PreProjectileHitCallback(VanillaLevelCallbacks.PreProjectileHitParams param, CallbackResult result)
        {
            var hit = param.hit;
            var damage = param.damage;
            var entity = hit.Other;
            var level = entity.Level;
            var projectile = hit.Projectile;
            Buff buff = null;
            Entity source = null;
            foreach (var b in entity.GetBuffs(this))
            {
                var sourceID = GetSource(b);
                if (sourceID != null)
                {
                    var src = sourceID.GetEntity(level);
                    if (src.ExistsAndAlive())
                    {
                        buff = b;
                        source = src;
                        break;
                    }
                }
            }
            if (buff == null || !source.ExistsAndAlive())
                return;
            foreach (var shieldBuff in source.GetBuffs<LightningOrbEnergyShieldBuff>())
            {
                LightningOrbEnergyShieldBuff.Heal(shieldBuff, 1);
            }
            projectile.Remove();
            result.SetFinalValue(false);

            source.PlaySound(VanillaSoundID.energyShieldHit);
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var damage = param.input;
            var entity = damage.Entity;
            var level = entity.Level;
            Buff buff = null;
            Entity source = null;
            foreach (var b in entity.GetBuffs(this))
            {
                var sourceID = GetSource(b);
                if (sourceID != null)
                {
                    var src = sourceID.GetEntity(level);
                    if (src.ExistsAndAlive())
                    {
                        buff = b;
                        source = src;
                        break;
                    }
                }
            }
            if (buff == null || !source.ExistsAndAlive())
                return;
            foreach (var shieldBuff in source.GetBuffs<LightningOrbEnergyShieldBuff>())
            {
                LightningOrbEnergyShieldBuff.Damage(shieldBuff, 1);
                result.SetFinalValue(false);
            }
        }
        public static void SetSource(Buff buff, EntityID id) => buff.SetProperty<EntityID>(PROP_SOURCE, id);
        public static EntityID GetSource(Buff buff) => buff.GetProperty<EntityID>(PROP_SOURCE);
        public static readonly VanillaBuffPropertyMeta<EntityID> PROP_SOURCE = new VanillaBuffPropertyMeta<EntityID>("source");
    }
}
