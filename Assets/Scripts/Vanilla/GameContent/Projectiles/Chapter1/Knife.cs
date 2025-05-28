using System.Linq;
using MVZ2.GameContent.Buffs.Projectiles;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2.Vanilla.Shells;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.knife)]
    public class Knife : ProjectileBehaviour
    {
        public Knife(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity projectile)
        {
            base.Init(projectile);
            SetWaitTimer(projectile, new FrameTimer(30));
            SetNoDelay(projectile, false);
        }
        public override void Update(Entity projectile)
        {
            base.Update(projectile);
            var timer = GetWaitTimer(projectile);
            var shootPoint = projectile.Position;
            if (!projectile.HasBuff<ProjectileWaitBuff>())
                timer.Run();

            if (!IsNoDelay(projectile))
            {
                if (timer.Expired)
                {
                    var shootDir = (GetDestination(projectile) - shootPoint).normalized;
                    {
                        projectile.Velocity = 10 * shootDir;
                        SetNoDelay(projectile, true);
                    }
                }
                else if (!projectile.HasBuff<ProjectileWaitBuff>())
                {
                    projectile.Velocity -= projectile.Velocity / 10;
                }
            }
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damageOutput)
        {
            base.PostHitEntity(hitResult, damageOutput);
            if (damageOutput == null)
                return;
            var blocksSlice = damageOutput.GetAllResults().Any(e => e?.ShellDefinition?.BlocksSlice() ?? false);
            if (!blocksSlice)
            {
                hitResult.Pierce = true;
            }
        }
        public static Vector3 GetDestination(Entity entity)
        {
            return entity.GetBehaviourField<Vector3>(ID, PROP_DESTINATION);
        }
        public static void SetDestination(Entity entity, Vector3 target)
        {
            entity.SetBehaviourField(ID, PROP_DESTINATION, target);
        }
        public static bool IsNoDelay(Entity entity)
        {
            return entity.GetBehaviourField<bool>(ID, PROP_NO_DELAY);
        }
        public static void SetNoDelay(Entity entity, bool value)
        {
            entity.SetBehaviourField(ID, PROP_NO_DELAY, value);
        }
        public static FrameTimer GetWaitTimer(Entity entity)
        {
            return entity.GetBehaviourField<FrameTimer>(ID, PROP_WAIT_TIMER);
        }
        public static void SetWaitTimer(Entity entity, FrameTimer value)
        {
            entity.SetBehaviourField(ID, PROP_WAIT_TIMER, value);
        }
        public static readonly NamespaceID ID = VanillaProjectileID.knife;
        public static readonly VanillaEntityPropertyMeta<Vector3> PROP_DESTINATION = new VanillaEntityPropertyMeta<Vector3>("Destination");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_NO_DELAY = new VanillaEntityPropertyMeta<bool>("NoDelay");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_WAIT_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("WaitTimer");
    }
}
