using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.spike)]
    public class Spike : ProjectileBehaviour
    {
        public Spike(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var projectile = hitResult.Projectile;

            var hitCount = GetHitCount(projectile);
            hitCount++;
            SetHitCount(projectile, hitCount);
            if (hitCount >= MAX_HIT_COUNT)
            {
                hitResult.Pierce = false;
                return;
            }
        }
        public static int GetHitCount(Entity entity) => entity.GetBehaviourField<int>(PROP_HIT_COUNT);
        public static void SetHitCount(Entity entity, int value) => entity.SetBehaviourField(PROP_HIT_COUNT, value);
        public static readonly VanillaEntityPropertyMeta<int> PROP_HIT_COUNT = new VanillaEntityPropertyMeta<int>("HitCount");
        public const int MAX_HIT_COUNT = 3;
    }
}
