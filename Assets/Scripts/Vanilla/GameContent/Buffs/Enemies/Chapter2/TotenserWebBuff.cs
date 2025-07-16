using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Models;
using MVZ2Logic.Models;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Enemies
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
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var ent = buff.GetEntity();
            if (ent == null)
            {
                buff.Remove();
            }
            if (IsOutsideView(ent))
            {
                ent.TakeDamage(20, new DamageEffectList(VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN), ent);
                buff.Remove();
            }
        }
        private void PostEntityDeathCallback(LevelCallbacks.PostEntityDeathParams param, CallbackResult result)
        {
            var entity = param.entity;
            entity.RemoveBuffs<TotenserWebBuff>();
        }
        private bool IsOutsideView(Entity proj)
        {
            var bounds = proj.GetBounds();
            var position = proj.Position;
            return bounds.max.x < VanillaLevelExt.ATTACK_LEFT_BORDER ||
                bounds.min.x > VanillaLevelExt.ATTACK_RIGHT_BORDER;
        }
    }
}
