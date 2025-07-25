using System.Linq;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using PVZEngine;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.divineShield)]
    public class TheWaferBuff : BuffDefinition
    {
        public TheWaferBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new ColorModifier(EngineEntityProps.COLOR_OFFSET, new Color(0.3f, 0.4f, 0.6f, 0.7f)));
            AddTrigger(VanillaLevelCallbacks.PRE_ENTITY_TAKE_DAMAGE, PreEntityTakeDamageCallback, priority: -100);
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            var entity = buff.GetEntity();
            if (entity == null)
                return;
            entity.PlaySound(VanillaSoundID.holy);
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var input = param.input;
            if (NamespaceID.IsValid(input.ShieldTarget))
                return;
            var entity = input.Entity;
            var buff = entity.GetFirstBuff<TheWaferBuff>();
            if (buff == null)
                return;

            var buffs = entity.GetBuffs<TheWaferBuff>();
            input.Multiply(Mathf.Max(1 - buffs.Count() / 10, 0));
            buff.Remove();
        }
        private void PostEntityDeathCallback(LevelCallbacks.PostEntityDeathParams param, CallbackResult result)
        {
            var entity = param.entity;
            var info = param.deathInfo;
            if (info.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            foreach (var buff in entity.GetBuffs<TheWaferBuff>())
            {
                buff.Remove();
            }
        }
    }
}
