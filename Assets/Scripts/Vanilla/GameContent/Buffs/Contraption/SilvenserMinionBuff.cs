using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Stages;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.silvenser_minion)]
    public class SilvenserMinion : BuffDefinition
    {
        public SilvenserMinion(string nsp, string name) : base(nsp, name)
        {
            AddModifier(ColorModifier.Multiply(EngineEntityProps.TINT, PROP_TINT_MULTIPLIER));
            AddModifier(new BooleanModifier(VanillaEntityProps.ETHEREAL, PROP_ETHEREAL));
            AddModifier(new FloatModifier(VanillaEntityProps.SHADOW_ALPHA, NumberOperator.Multiply, PROP_SHADOW_ALPHA));
            AddTrigger(VanillaLevelCallbacks.PRE_ENTITY_TAKE_DAMAGE, PreEntityTakeDamageCallback);
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TINT_MULTIPLIER, new Color(1, 1, 1, GetMinAlpha(buff)));
            buff.SetProperty(PROP_ETHEREAL, true);
            buff.SetProperty(PROP_SHADOW_ALPHA, SHADOW_ALPHA_MIN);
        }
        private void PreEntityTakeDamageCallback(DamageInput damageInfo)
        {
            var entity = damageInfo.Entity;
            if (entity == null)
                return;
            buffBuffer.Clear();
            entity.GetBuffs<SilvenserMinion>(buffBuffer);
            if (buffBuffer.Count <= 0)
                return;
            foreach (var buff in buffBuffer)
            {
                if (buff.GetProperty<bool>(PROP_ETHEREAL))
                {
                    damageInfo.Multiply(0.1f);
                    break;
                }
            }
        }
        private static float GetMinAlpha(Buff buff)
        {
            return TINT_ALPHA_MIN;
        }
        public static readonly VanillaBuffPropertyMeta PROP_EVER_ILLUMINATED = new VanillaBuffPropertyMeta("EverIlluminated");
        public static readonly VanillaBuffPropertyMeta PROP_TINT_MULTIPLIER = new VanillaBuffPropertyMeta("TintMultiplier");
        public static readonly VanillaBuffPropertyMeta PROP_SHADOW_ALPHA = new VanillaBuffPropertyMeta("ShadowAlpha");
        public static readonly VanillaBuffPropertyMeta PROP_ETHEREAL = new VanillaBuffPropertyMeta("Ethereal");
        public const float TINT_ALPHA_MIN = 0.5f;
        public const float TINT_ALPHA_MAX = 1;
        public const float TINT_SPEED = 0.02f;
        public const float SHADOW_ALPHA_MIN = 0;
        public const float SHADOW_ALPHA_MAX = 1;
        public const float SHADOW_ALPHA_SPEED = 0.04f;
        private List<Buff> buffBuffer = new List<Buff>();
        private static List<Buff> checkBuffer = new List<Buff>();
    }
}
