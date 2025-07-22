using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.lightningOrbEnergyShield)]
    public class LightningOrbEnergyShieldBuff : BuffDefinition
    {
        public LightningOrbEnergyShieldBuff(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.PRE_ENTITY_TAKE_DAMAGE, PreEntityTakeDamageCallback);
            AddModifier(new Vector3Modifier(EngineEntityProps.SIZE, NumberOperator.Multiply, Vector3.one * 5));
            AddModifier(new BooleanModifier(VanillaContraptionProps.BLOCKS_JUMP, true));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            if (GetDamageCount(buff) >= MAX_DAMAGE)
                buff.Remove();
        }
        public override void PostRemove(Buff buff)
        {
            base.PostRemove(buff);
            var entity = buff.GetEntity();
            if (entity != null)
            {
                entity.PlaySound(VanillaSoundID.energyShieldBreak);
                entity.AddBuff<LightningOrbEnergyShieldBreakBuff>();
            }
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var damage = param.input;
            var entity = damage.Entity;
            foreach (var buff in entity.GetBuffs<LightningOrbEnergyShieldBuff>())
            {
                AddDamageCount(buff, 1);
                result.SetFinalValue(false);
            }
        }
        public static float GetDamageCount(Buff buff) => buff.GetProperty<float>(PROP_TAKEN_DAMAGE);
        public static void SetDamageCount(Buff buff, float value) => buff.SetProperty(PROP_TAKEN_DAMAGE, value);
        public static void AddDamageCount(Buff buff, float value) => SetDamageCount(buff, GetDamageCount(buff) + value);
        public static void ResetDamageCount(Buff buff) => SetDamageCount(buff, 0);
        public const float MAX_DAMAGE = 360;
        public static readonly VanillaBuffPropertyMeta<float> PROP_TAKEN_DAMAGE = new VanillaBuffPropertyMeta<float>("DamageCount");
    }
}
