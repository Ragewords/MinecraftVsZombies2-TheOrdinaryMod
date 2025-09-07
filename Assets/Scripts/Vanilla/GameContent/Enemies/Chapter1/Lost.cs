using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Stages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.lost)]
    public class Lost : StateEnemy
    {
        public Lost(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            if (!entity.HasBuff<GhostBuff>())
            {
                entity.AddBuff<GhostBuff>();
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelDamagePercent();
            if (!entity.HasBuff<GhostBuff>())
            {
                entity.AddBuff<GhostBuff>();
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            var ghostBuff = entity.GetBuffs<GhostBuff>();
            foreach (var buff in ghostBuff)
            {
                GhostBuff.Illuminate(buff);
            }
            if (entity.Level.HasBehaviour<WhackAGhostBehaviour>())
            {
                entity.PlaySound(VanillaSoundID.thumbsDown);
                entity.Level.AddBuff<LostDeathPunishBuff>();
            }
        }
        public static readonly NamespaceID ID = VanillaEnemyID.ghost;
    }
}
