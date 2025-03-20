using MVZ2.GameContent.Bosses;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2.Vanilla.Shells;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.knife)]
    public class Knife : ProjectileBehaviour
    {
        public Knife(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Update(Entity projectile)
        {
            base.Update(projectile);
            if (!IsSpecial(projectile))
            {
                var shootPoint = projectile.Position;
                if (projectile.Velocity.magnitude >= 1)
                    projectile.Velocity -= projectile.Velocity / 10;
                else
                {
                    var shootDir = (GetDestination(projectile) - shootPoint).normalized;
                    projectile.Velocity = 10 * shootDir;
                    SetSpecial(projectile, true);
                }
            }
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damageOutput)
        {
            base.PostHitEntity(hitResult, damageOutput);
            if (damageOutput == null)
                return;
            var bodyResult = damageOutput.BodyResult;
            var armorResult = damageOutput.ArmorResult;
            var bodyShell = bodyResult?.ShellDefinition;
            var armorShell = armorResult?.ShellDefinition;
            var bodyBlocksSlice = bodyShell != null ? bodyShell.BlocksSlice() : false;
            var armorBlocksSlice = armorShell != null ? armorShell.BlocksSlice() : false;
            var blocksSlice = bodyBlocksSlice || armorBlocksSlice;
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
        public static bool IsSpecial(Entity entity)
        {
            return entity.GetBehaviourField<bool>(ID, PROP_SPECIAL);
        }
        public static void SetSpecial(Entity entity, bool value)
        {
            entity.SetBehaviourField(ID, PROP_SPECIAL, value);
        }
        public static readonly NamespaceID ID = VanillaProjectileID.knife;
        public static readonly VanillaEntityPropertyMeta PROP_DESTINATION = new VanillaEntityPropertyMeta("Destination");
        public static readonly VanillaEntityPropertyMeta PROP_SPECIAL = new VanillaEntityPropertyMeta("Special");
    }
}
