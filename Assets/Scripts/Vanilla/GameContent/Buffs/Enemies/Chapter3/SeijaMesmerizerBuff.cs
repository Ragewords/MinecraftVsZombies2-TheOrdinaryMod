using MVZ2.GameContent.Bosses;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Level;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.seijaMesmerizer)]
    public class SeijaMesmerizerBuff : BuffDefinition
    {
        public SeijaMesmerizerBuff(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
            AddTrigger(LevelCallbacks.POST_ENTITY_REMOVE, PostEntityRemoveCallback);
        }
        private void PostEntityDeathCallback(LevelCallbacks.PostEntityDeathParams param, CallbackResult result)
        {
            var entity = param.entity;
            var info = param.deathInfo;
            if (!entity.HasBuff<SeijaMesmerizerBuff>())
                return;
            foreach (var seija in entity.Level.FindEntities(VanillaBossID.seija))
            {
                if (seija.IsDead)
                    continue;
                seija.Die();
            }
        }
        private void PostEntityRemoveCallback(EntityCallbackParams param, CallbackResult result)
        {
            var enemy = param.entity;
            if (!enemy.HasBuff<SeijaMesmerizerBuff>())
                return;
            foreach (var seija in enemy.Level.FindEntities(VanillaBossID.seija))
            {
                if (seija.IsDead)
                    continue;
                seija.Die();
            }
        }
    }
}
