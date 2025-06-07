using System;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Models;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Models;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Models;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.curseOfTheAbyss)]
    public class CurseOfTheAbyssBuff : BuffDefinition
    {
        public CurseOfTheAbyssBuff(string nsp, string name) : base(nsp, name)
        {
            AddModelInsertion(LogicModelHelper.ANCHOR_CENTER, VanillaModelKeys.curseOfTheAbyss, VanillaModelID.curseOfTheAbyss);
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback, filter: EntityTypes.PLANT);
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMEOUT, 5);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timeout = buff.GetProperty<int>(PROP_TIMEOUT);
            timeout--;
            buff.SetProperty(PROP_TIMEOUT, timeout);
            if (timeout <= 0)
            {
                buff.Remove();
            }
        }
        private void PostEntityDeathCallback(LevelCallbacks.PostEntityDeathParams param, CallbackResult result)
        {
            var entity = param.entity;
            if (entity == null)
                return;
            if (!entity.HasBuff<CurseOfTheAbyssBuff>())
                return;
            var gargoyle = entity.Spawn(VanillaEnemyID.gargoyle, entity.Position);
            var buff = entity.GetFirstBuff<CurseOfTheAbyssBuff>();
            entity.PlaySound(VanillaSoundID.lightningAttack);
            gargoyle.SetFaction(buff.GetProperty<int>(PROP_FACTION));
            entity.RemoveBuffs<CurseOfTheAbyssBuff>();
        }
        public static void SetCurseFaction(Buff buff, int value)
        {
            buff.SetProperty(PROP_FACTION, value);
        }
        public static readonly VanillaBuffPropertyMeta<int> PROP_TIMEOUT = new VanillaBuffPropertyMeta<int>("Timeout");
        public static readonly VanillaBuffPropertyMeta<int> PROP_FACTION = new VanillaBuffPropertyMeta<int>("Faction");
    }
}
