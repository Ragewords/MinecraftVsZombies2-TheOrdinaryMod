using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.web)]
    public class Web : ProjectileBehaviour
    {
        public Web(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var enemy = hitResult.Other;
            if (enemy.Type != EntityTypes.ENEMY)
                return;
            if (!enemy.CanDeactive())
            {
                hitResult.Pierce = true;
                return;
            }
            hitResult.Pierce = false;
            if (enemy.HasBuff<TotenserWebBuff>())
                return;
            enemy.AddBuff<TotenserWebBuff>();
        }
    }
}
