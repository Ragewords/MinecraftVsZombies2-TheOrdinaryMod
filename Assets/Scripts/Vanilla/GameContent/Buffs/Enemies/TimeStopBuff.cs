using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Models;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Models;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.timeStop)]
    public class TimeStopBuff : BuffDefinition
    {
        public TimeStopBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new BooleanModifier(VanillaEntityProps.AI_FROZEN, true));
            AddModelInsertion(LogicModelHelper.ANCHOR_CENTER, VanillaModelKeys.timeStop, VanillaModelID.timeStop);
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMEOUT, MAX_TIMEOUT);
        }
        private void PostEntityDeathCallback(Entity entity, DeathInfo info)
        {
            entity.RemoveBuffs<TimeStopBuff>();
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var enemy = buff.GetEntity();
            if (enemy == null)
                return;
            var timeout = buff.GetProperty<int>(PROP_TIMEOUT);
            timeout--;
            buff.SetProperty(PROP_TIMEOUT, timeout);
            if (timeout <= 0)
            {
                buff.Remove();
            }

            var model = buff.GetInsertedModel(VanillaModelKeys.timeStop);
            if (model != null)
            {
                model.SetAnimationFloat("Clock", (timeout - RING_DURATION) / (float)(MAX_TIMEOUT - RING_DURATION));
            }
        }
        public static readonly VanillaBuffPropertyMeta PROP_TIMEOUT = new VanillaBuffPropertyMeta("Timeout");
        private const float RING_DURATION = 0.5f;
        private const int MAX_TIMEOUT = 30;
    }
}
