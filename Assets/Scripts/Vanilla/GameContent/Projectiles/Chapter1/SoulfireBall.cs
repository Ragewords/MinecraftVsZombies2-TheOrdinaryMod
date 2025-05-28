using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
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
            var entity = hitResult.Projectile;
            var other = hitResult.Other;

            bool blocksFire = damageOutput.WillDamageBlockFire();

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
                    var projectile = other.ShootProjectile(new ShootParams()
                    {
                        projectileID = VanillaProjectileID.soulfireBall,
                        position = entity.Position,
                        velocity = velocity,
                        faction = entity.GetFaction(),
                        damage = entity.GetDamage() / 4
                    });
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
                entity.SplashDamage(hitResult.Collider, entity.Position, 40, entity.GetFaction(), entity.GetDamage() / 4f, damageEffects);
            }
        }
        public static void SetBlast(Entity entity, bool value)
        {
            entity.SetBehaviourField(PROP_BLAST, value);
        }
        public static void SetSplit(Entity entity, bool value)
        {
            entity.SetBehaviourField(PROP_SPLIT, value);
        }
        public static bool IsBlast(Entity entity)
        {
            return entity.GetBehaviourField<bool>(PROP_BLAST);
        }
        public static Entity GetSplitSource(Entity entity)
        {
            var entityID = entity.GetBehaviourField<EntityID>(SPLIT_SOURCE);
            return entityID?.GetEntity(entity.Level);
        }
        public static void SetSplitSource(Entity entity, Entity value)
        {
            entity.SetBehaviourField(SPLIT_SOURCE, new EntityID(value));
        }

        public static bool IsSplit(Entity entity)
        {
            return entity.GetBehaviourField<bool>(PROP_SPLIT);
        }
        public static readonly VanillaEntityPropertyMeta<bool> PROP_BLAST = new VanillaEntityPropertyMeta<bool>("Blast");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_SPLIT = new VanillaEntityPropertyMeta<bool>("Split");
        public static readonly VanillaEntityPropertyMeta<EntityID> SPLIT_SOURCE = new VanillaEntityPropertyMeta<EntityID>("SplitSource");
    }
}
