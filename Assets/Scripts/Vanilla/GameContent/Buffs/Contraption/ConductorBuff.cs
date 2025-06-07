using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Fragments;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Models;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Models;
using PVZEngine;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.conductor)]
    public class ConductorBuff : BuffDefinition
    {
        public ConductorBuff(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.PRE_ENTITY_TAKE_DAMAGE, PreEntityTakeDamageCallback, priority: -100);
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var input = param.input;
            var entity = input.Entity;
            var buff = entity.GetFirstBuff<ConductorBuff>();
            if (buff == null)
                return;
            if (input.Effects.HasEffect(VanillaDamageEffects.LIGHTNING))
                result.SetFinalValue(false);
        }
        private void PostEntityDeathCallback(LevelCallbacks.PostEntityDeathParams param, CallbackResult result)
        {
            var entity = param.entity;
            foreach (var buff in entity.GetBuffs<ConductorBuff>())
                buff.Remove();
        }
    }
}
