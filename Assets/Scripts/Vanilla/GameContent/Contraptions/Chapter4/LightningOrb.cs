using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.lightningOrb)]
    public class LightningOrb : ContraptionBehaviour
    {
        public LightningOrb(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.PRE_PROJECTILE_HIT, PreProjectileHitCallback);
        }
        protected override void UpdateAI(Entity contraption)
        {
            base.UpdateAI(contraption);
            if (GetChargedAbsorbCount(contraption) >= 60)
                SetCharged(contraption, false);
        }
        protected override void UpdateLogic(Entity contraption)
        {
            base.UpdateLogic(contraption);
            contraption.SetAnimationFloat("Damaged", 1 - contraption.Health / contraption.GetMaxHealth());
            contraption.SetAnimationBool("Absorbing", contraption.HasBuff<LightningOrbEvokedBuff>());
            contraption.SetAnimationBool("Charged", IsCharged(contraption));
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
            projectile.Remove();
            result.SetFinalValue(false);
            orb.PlaySound(VanillaSoundID.energyShieldHit);
            if (IsCharged(orb))
            {
                for (var i = 0; i < 3; i++)
                {
                    var sparam = orb.GetShootParams();
                    sparam.soundID = null;
                    orb.ShootProjectile(sparam);
                }
                AddChargedAbsorbCount(orb, 1);
            }
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
            entity.PlaySound(VanillaSoundID.teslaAttack);
            entity.PlaySound(VanillaSoundID.lightningAttack);
            entity.AddBuff<LightningOrbEvokedBuff>();
            SetCharged(entity, true);
            ResetChargedAbsorbCount(entity);
        }
        public static void SetCharged(Entity entity, bool value)
        {
            entity.SetBehaviourField(PROP_CHARGED, value);
        }
        public static bool IsCharged(Entity entity)
        {
            return entity.GetBehaviourField<bool>(PROP_CHARGED);
        }
        public static int GetChargedAbsorbCount(Entity entity) => entity.GetBehaviourField<int>(PROP_CHARGED_ABSORB_COUNT);
        public static void SetChargedAbsorbCount(Entity entity, int value) => entity.SetBehaviourField(PROP_CHARGED_ABSORB_COUNT, value);
        public static void AddChargedAbsorbCount(Entity entity, int value) => SetChargedAbsorbCount(entity, GetChargedAbsorbCount(entity) + value);
        public static void ResetChargedAbsorbCount(Entity entity) => SetChargedAbsorbCount(entity, 0);
        public static readonly VanillaEntityPropertyMeta<bool> PROP_CHARGED = new VanillaEntityPropertyMeta<bool>("Charged");
        private static readonly VanillaEntityPropertyMeta<int> PROP_CHARGED_ABSORB_COUNT = new VanillaEntityPropertyMeta<int>("ChargedAbsorbCount");
        public const float HEAL_AMOUNT = 100;
    }
}
