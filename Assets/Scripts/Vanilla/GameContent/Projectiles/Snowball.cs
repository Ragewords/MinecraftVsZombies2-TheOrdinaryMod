using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Shells;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using static UnityEngine.EventSystems.EventTrigger;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.snowball)]
    public class Snowball : ProjectileBehaviour
    {
        public Snowball(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var enemy = hitResult.Other;
            if (enemy.Type != EntityTypes.ENEMY)
                return;
            else if (!enemy.HasBuff<FreezeSlowBuff>())
                enemy.PlaySound(VanillaSoundID.freeze);
            enemy.FreezeSlow(150);
        }
    }
}
