using System.Collections.Generic;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Fragments;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Auras;
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
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
            AddModifier(new Vector3Modifier(EngineEntityProps.SIZE, NumberOperator.Multiply, new Vector3(4.6f, 2.5f, 4.6f)));
            AddModifier(new IntModifier(VanillaEntityProps.VEHICLE_INTERACTION, NumberOperator.Set, VehicleInteraction.BLOCK));
            AddAura(new EnergyShieldAura());
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            ResetHealth(buff);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();
            if (entity != null)
            {
                if (!entity.IsEntityOf(VanillaContraptionID.lightningOrb))
                {
                    buff.Remove();
                    return;
                }
                entity.SetAnimationFloat("ShieldDamaged", GetHealth(buff) / MAX_HEALTH);
                entity.SetAnimationFloat("ShieldSpeed", 1 + (1 - GetHealth(buff) / MAX_HEALTH) * 3);
            }

            if (GetHealth(buff) <= 0)
            {
                Break(buff);
                buff.Remove();
            }
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var damage = param.input;
            var entity = damage.Entity;
            var amount = damage.Amount;
            foreach (var buff in entity.GetBuffs<LightningOrbEnergyShieldBuff>())
            {
                TakeDamage(buff, amount);
                result.SetFinalValue(false);
            }
        }
        private void PostEntityDeathCallback(LevelCallbacks.PostEntityDeathParams param, CallbackResult result)
        {
            var entity = param.entity;
            var buffs = entity.GetBuffs<LightningOrbEnergyShieldBuff>();
            entity.RemoveBuffs(buffs);
        }
        public static float GetHealth(Buff buff) => buff.GetProperty<float>(PROP_TAKEN_DAMAGE);
        public static void SetHealth(Buff buff, float value) => buff.SetProperty(PROP_TAKEN_DAMAGE, value);
        public static void TakeDamage(Buff buff, float value)
        {
            SetHealth(buff, GetHealth(buff) - Mathf.Min(value, MAX_TAKE_DAMAGE));
            var entity = buff.GetEntity();
            entity?.CreateFragmentAndPlay(VanillaFragmentID.lightningOrbEnergyShield, value);
        }
        public static void Heal(Buff buff, float value)
        {
            var entity = buff.GetEntity();
            if (entity != null)
            {
                var hpBefore = GetHealth(buff);
                SetHealth(buff, Mathf.Min(MAX_HEALTH, GetHealth(buff) + value));
                entity.AddTickHealing(GetHealth(buff) - hpBefore);
            }
        }
        public static void Break(Buff buff)
        {
            var entity = buff.GetEntity();
            if (entity != null)
            {
                entity.PlaySound(VanillaSoundID.glassBreak);
                entity.PlaySound(VanillaSoundID.energyShieldBreak);
                entity.AddBuff<LightningOrbEnergyShieldBreakBuff>();
                entity.CreateFragmentAndPlay(VanillaFragmentID.lightningOrbEnergyShield);
            }
        }
        public static void ResetHealth(Buff buff) => SetHealth(buff, MAX_HEALTH);
        public const float MAX_HEALTH = 3000;
        public const float MAX_TAKE_DAMAGE = 1000;
        public static readonly VanillaBuffPropertyMeta<float> PROP_TAKEN_DAMAGE = new VanillaBuffPropertyMeta<float>("Health");

        public class EnergyShieldAura : AuraEffectDefinition
        {
            public EnergyShieldAura()
            {
                BuffID = VanillaBuffID.lightningOrbEnergyShieldProtected;
                UpdateInterval = 3;
                protectDetector = new LightningOrbEnergyShieldDetector()
                {
                    factionTarget = FactionTarget.Friendly
                };
            }

            public override void GetAuraTargets(AuraEffect auraEffect, List<IBuffTarget> results)
            {
                var source = auraEffect.Source;
                var entity = source.GetEntity();
                if (entity == null)
                    return;
                detectBuffer.Clear();
                protectDetector.DetectEntities(entity, detectBuffer);
                results.AddRange(detectBuffer);
            }
            private Detector protectDetector;
            private List<Entity> detectBuffer = new List<Entity>();
        }
    }
}
