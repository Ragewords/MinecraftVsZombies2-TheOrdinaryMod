using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Contraptions;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.jackDullahanHead)]
    public class JackDullahanHead : DullahanHead
    {
        public JackDullahanHead(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.POST_ENEMY_MELEE_ATTACK, PostEnemyMeleeAttackCallback);
        }
        private void PostEnemyMeleeAttackCallback(VanillaLevelCallbacks.EnemyMeleeAttackParams param, CallbackResult result)
        {
            var enemy = param.enemy;
            var target = param.target;
            if (!enemy.IsEntityOf(VanillaEnemyID.jackDullahanHead))
                return;
            if (!target.IsHostile(enemy))
                return;
            target.InflictWither(30);
        }
    }
}
