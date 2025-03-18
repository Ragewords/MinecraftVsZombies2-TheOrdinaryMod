using MVZ2.GameContent.Buffs;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Entities;
using PVZEngine.Buffs;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEditor.UI;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.flagZombie)]
    public class FlagZombie : Zombie
    {
        public FlagZombie(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.SetAnimationBool("HasFlag", true);
            foreach (var target in entity.Level.GetEntities(EntityTypes.ENEMY))
            {
                if (target.IsFriendly(entity))
                {
                    if (!target.HasBuff<FlagZombieEnemySpeedBuff>())
                    {
                        var buff = target.AddBuff<FlagZombieEnemySpeedBuff>();
                        buff.SetProperty(FlagZombieEnemySpeedBuff.PROP_SPEED_MULTIPLIER, 2);
                    }
                }
            }
        }
        protected override float GetRandomSpeedMultiplier(Entity entity)
        {
            return 2;
        }
    }
}
