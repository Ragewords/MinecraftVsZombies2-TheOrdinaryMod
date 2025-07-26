using System.Collections.Generic;
using MVZ2.GameContent.Buffs;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Auras;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.lightningOrb)]
    public class LightningOrb : ContraptionBehaviour
    {
        public LightningOrb(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.PRE_PROJECTILE_HIT, PreProjectileHitCallback);
            AddAura(new EnergyShield());
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.AddBuff<LightningOrbEnergyShieldBuff>();
            SetShieldRegenerateTimer(entity, new FrameTimer(REGENERATE_TIME));
        }
        protected override void UpdateAI(Entity contraption)
        {
            base.UpdateAI(contraption);
            if (contraption.HasBuff<LightningOrbEnergyShieldBuff>())
                return;
            var timer = GetShieldRegenerateTimer(contraption);
            timer.Run();
            if (timer.Expired)
            {
                timer.ResetTime(REGENERATE_TIME);
                contraption.AddBuff<LightningOrbEnergyShieldBuff>();
            }
        }
        protected override void UpdateLogic(Entity contraption)
        {
            base.UpdateLogic(contraption);
            contraption.SetAnimationFloat("Damaged", 1 - contraption.Health / contraption.GetMaxHealth());
            contraption.SetAnimationBool("Absorbing", contraption.HasBuff<LightningOrbEvokedBuff>());
            contraption.SetAnimationBool("Charged", contraption.HasBuff<LightningOrbEnergyShieldBuff>());
        }
        private void PreProjectileHitCallback(VanillaLevelCallbacks.PreProjectileHitParams param, CallbackResult result)
        {
            var hit = param.hit;
            var damage = param.damage;
            if (NamespaceID.IsValid(damage.ShieldTarget))
                return;
            var orb = hit.Other;
            if (!orb.Definition.HasBehaviour(this))
                return;

            var projectile = hit.Projectile;

            orb.HealEffects(100, projectile);
            foreach (var buff in orb.GetBuffs<LightningOrbEvokedBuff>())
            {
                LightningOrbEvokedBuff.AddTakenDamage(buff, damage.Amount);
            }
            foreach (var buff in orb.GetBuffs<LightningOrbEnergyShieldBuff>())
            {
                LightningOrbEnergyShieldBuff.Heal(buff, 1);
            }
            projectile.Remove();
            result.SetFinalValue(false);
            orb.PlaySound(VanillaSoundID.energyShieldHit);
        }
        public override bool CanEvoke(Entity entity)
        {
            if (entity.HasBuff<LightningOrbEvokedBuff>())
                return false;
            return base.CanEvoke(entity);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            entity.PlaySound(VanillaSoundID.lightningAttack);
            if (entity.HasBuff<LightningOrbEnergyShieldBuff>())
                entity.RemoveBuffs<LightningOrbEnergyShieldBuff>();
            entity.AddBuff<LightningOrbEvokedBuff>();
            var timer = GetShieldRegenerateTimer(entity);
            timer.ResetTime(REGENERATE_TIME_EVOKED);
        }
        public class EnergyShield : AuraEffectDefinition
        {
            public EnergyShield()
            {
                BuffID = VanillaBuffID.lightningOrbEnergyShieldProtected;
                UpdateInterval = 3;
                protectDetector = new LightningOrbEnergyShieldDetector()
                {
                    mask = EntityCollisionHelper.MASK_VULNERABLE,
                    factionTarget = FactionTarget.Friendly
                };
            }
            public override void GetAuraTargets(AuraEffect auraEffect, List<IBuffTarget> results)
            {
                var source = auraEffect.Source;
                var entity = source.GetEntity();
                if (entity == null)
                    return;
                if (!entity.HasBuff<LightningOrbEnergyShieldBuff>())
                    return;
                protectDetectBuffer.Clear();
                protectDetector.DetectEntities(entity, protectDetectBuffer);
                foreach (var id in protectDetectBuffer)
                {
                    if (id.HasBuff<LightningOrbEnergyShieldBuff>())
                        continue;
                    if (id.HasBuff<DevourerInvincibleBuff>())
                        continue;
                    results.Add(id);
                }
            }
            public override void UpdateTargetBuff(AuraEffect effect, IBuffTarget target, Buff buff)
            {
                base.UpdateTargetBuff(effect, target, buff);
                var entity = effect?.Source?.GetEntity();
                if (!entity.ExistsAndAlive())
                    return;
                LightningOrbEnergyShieldProtectedBuff.SetSource(buff, new EntityID(entity));
            }
            private Detector protectDetector;
            private List<Entity> protectDetectBuffer = new List<Entity>();
        }
        public static FrameTimer GetShieldRegenerateTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_TIMER);
        public static void SetShieldRegenerateTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_TIMER, timer);
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("timer");
        public const float HEAL_AMOUNT = 100;
        public const int REGENERATE_TIME = 600;
        public const int REGENERATE_TIME_EVOKED = 155;
    }
}
