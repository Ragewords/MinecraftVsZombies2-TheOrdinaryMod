using System.Collections.Generic;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.nightmareIllusionEye)]
    public class IllusionEyeBuff : BuffDefinition
    {
        public IllusionEyeBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(ColorModifier.Multiply(EngineEntityProps.TINT, PROP_TINT_MULTIPLIER));
            AddModifier(new BooleanModifier(VanillaEntityProps.ETHEREAL, PROP_ETHEREAL));
            AddModifier(new FloatModifier(VanillaEntityProps.SHADOW_ALPHA, NumberOperator.Multiply, PROP_SHADOW_ALPHA));
            AddTrigger(VanillaLevelCallbacks.PRE_ENTITY_TAKE_DAMAGE, PreEntityTakeDamageCallback);
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TINT_MULTIPLIER, new Color(1, 1, 1, 0.5f));
            buff.SetProperty(PROP_ETHEREAL, true);
            buff.SetProperty(PROP_SHADOW_ALPHA, SHADOW_ALPHA_MIN);
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var damageInfo = param.input;
            var entity = damageInfo.Entity;
            if (entity == null)
                return;
            if (damageInfo.Effects.HasEffect(VanillaDamageEffects.WHACK))
                return;
            buffBuffer.Clear();
            entity.GetBuffs<IllusionEyeBuff>(buffBuffer);
            if (buffBuffer.Count <= 0)
                return;
            foreach (var buff in buffBuffer)
            {
                if (buff.GetProperty<bool>(PROP_ETHEREAL))
                {
                    damageInfo.Multiply(0.3f);
                    break;
                }
            }
        }
        public static readonly VanillaBuffPropertyMeta<Color> PROP_TINT_MULTIPLIER = new VanillaBuffPropertyMeta<Color>("TintMultiplier");
        public static readonly VanillaBuffPropertyMeta<float> PROP_SHADOW_ALPHA = new VanillaBuffPropertyMeta<float>("ShadowAlpha");
        public static readonly VanillaBuffPropertyMeta<bool> PROP_ETHEREAL = new VanillaBuffPropertyMeta<bool>("Ethereal");
        public const float SHADOW_ALPHA_MIN = 0;
        private List<Buff> buffBuffer = new List<Buff>();
    }
}
