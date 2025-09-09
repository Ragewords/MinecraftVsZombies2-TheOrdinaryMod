using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Shells;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.gargoyle)]
    public class Gargoyle : MeleeEnemy
    {
        public Gargoyle(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelDamagePercent();
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            var other = collision.Other;
            var entity = collision.Entity;
            if (entity == null)
                return;
            if (entity.Target != other)
                return;
            if (other.Type != EntityTypes.PLANT)
                return;
            if (other.GetShellDefinition().GetID() != VanillaShellID.stone && other.GetShellDefinition().GetID() != VanillaShellID.netherrack)
                return;
            if (!other.HasBuff<CurseOfTheAbyssBuff>())
            {
                var buff = other.NewBuff<CurseOfTheAbyssBuff>();
                CurseOfTheAbyssBuff.SetCurseFaction(buff, entity.GetFaction());
                other.AddBuff(buff);
            }
            else
            {
                var buff = other.GetFirstBuff<CurseOfTheAbyssBuff>();
                buff.SetProperty(CurseOfTheAbyssBuff.PROP_TIMEOUT, 5);
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            entity.Remove();
        }
    }
}
