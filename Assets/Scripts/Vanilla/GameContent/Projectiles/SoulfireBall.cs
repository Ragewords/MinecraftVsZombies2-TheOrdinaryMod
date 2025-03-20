using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2.Vanilla.Shells;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using System.Security.Cryptography;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.soulfireBall)]
    public class SoulfireBall : ProjectileBehaviour
    {
        public SoulfireBall(string nsp, string name) : base(nsp, name)
        {
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damageOutput)
        {
            base.PostHitEntity(hitResult, damageOutput);
            if (damageOutput == null)
                return;
            var bodyResult = damageOutput.BodyResult;
            var armorResult = damageOutput.ArmorResult;

            var entity = hitResult.Projectile;
            var other = hitResult.Other;

            var bodyShell = bodyResult?.ShellDefinition;
            var armorShell = armorResult?.ShellDefinition;
            var bodyBlocksFire = bodyShell != null ? bodyShell.BlocksFire() : false;
            var armorBlocksFire = armorShell != null ? armorShell.BlocksFire() : false;
            var blocksFire = bodyBlocksFire || armorBlocksFire;

            var blast = IsBlast(entity);
            if (blast)
            {
                entity.PlaySound(VanillaSoundID.darkSkiesImpact);
                entity.Level.ShakeScreen(3, 3, 3);
                entity.Level.Spawn(VanillaEffectID.soulfireBlast, entity.Position, entity);
            }
            else if (!blocksFire)
            {
                entity.Level.Spawn(VanillaEffectID.soulfire, entity.Position, entity);
            }
            if (!IsSplit(entity) && !blast)
            {
                for (int i = 0; i < 4; i++)
                {
                    var direction = Quaternion.Euler(0, 45 - i * 30, 0) * entity.Velocity.normalized;
                    var velocity = direction * 15;
                    var projectile = other.ShootProjectile(VanillaProjectileID.soulfireBall, velocity);
                    projectile.Position = entity.Position;
                    projectile.SetFactionAndDirection(entity.GetFaction());
                    projectile.SetDamage(entity.GetDamage() / 4);
                    projectile.SetScale(new Vector3(0.5f, 0.5f, 0.5f));
                    projectile.SetDisplayScale(new Vector3(0.5f, 0.5f, 0.5f));
                    projectile.SetShadowScale(new Vector3(0.5f, 0.5f, 0.5f));
                    SetSplit(projectile, true);
                }
            }
            {
                entity.Level.Spawn(VanillaEffectID.soulfire, entity.Position, entity);
            }

            if (!blocksFire || blast)
            {
                var damageEffects = new DamageEffectList(VanillaDamageEffects.FIRE, VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.MUTE);
                entity.Level.Explode(entity.Position, 40, entity.GetFaction(), entity.GetDamage() / 3f, damageEffects, entity);
            }
        }
        public static void SetBlast(Entity entity, bool value)
        {
            entity.SetBehaviourField(ID, PROP_BLAST, value);
        }
        public static void SetSplit(Entity entity, bool value)
        {
            entity.SetBehaviourField(ID, PROP_SPLIT, value);
        }
        public static bool IsBlast(Entity entity)
        {
            return entity.GetBehaviourField<bool>(ID, PROP_BLAST);
        }
        public static Entity GetSplitSource(Entity entity)
        {
            var entityID = entity.GetBehaviourField<EntityID>(ID, SPLIT_SOURCE);
            return entityID?.GetEntity(entity.Level);
        }
        public static void SetSplitSource(Entity entity, Entity value)
        {
            entity.SetBehaviourField(ID, SPLIT_SOURCE, new EntityID(value));
        }

        public static bool IsSplit(Entity entity)
        {
            return entity.GetBehaviourField<bool>(ID, PROP_SPLIT);
        }
        public static NamespaceID ID => VanillaProjectileID.soulfireBall;
        public static readonly VanillaEntityPropertyMeta PROP_BLAST = new VanillaEntityPropertyMeta("Blast");
        public static readonly VanillaEntityPropertyMeta PROP_SPLIT = new VanillaEntityPropertyMeta("Split");
        public static readonly VanillaEntityPropertyMeta SPLIT_SOURCE = new VanillaEntityPropertyMeta("SplitSource");
    }
}
