using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Models;
using MVZ2Logic.Models;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.totenserWeb)]
    public class TotenserWebBuff : BuffDefinition
    {
        public TotenserWebBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new BooleanModifier(VanillaEntityProps.AI_FROZEN, true));
            AddModelInsertion(LogicModelHelper.ANCHOR_CENTER, VanillaModelKeys.totenserWeb, VanillaModelID.totenserWeb);
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
        }
        private void PostEntityDeathCallback(Entity entity, DeathInfo info)
        {
            if (info.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            foreach (var buff in entity.GetBuffs<TotenserWebBuff>())
            {
                buff.Remove();
            }
        }
    }
}
