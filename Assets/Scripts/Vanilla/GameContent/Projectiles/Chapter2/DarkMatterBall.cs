using System.Linq;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Grids;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Projectiles
{
    [EntityBehaviourDefinition(VanillaProjectileNames.darkMatterBall)]
    public class DarkMatterBall : ProjectileBehaviour
    {
        public DarkMatterBall(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Update(Entity projectile)
        {
            base.Update(projectile);
            var hitCount = GetHitCount(projectile);
            if (hitCount >= 10)
            {
                projectile.Remove();
            }
        }
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var projectile = hitResult.Projectile;
            bool fromParent = hitResult.Other == projectile.Parent;
            hitResult.Pierce = true;
            if (fromParent)
            {
                return;
            }
            Explode(projectile, projectile.GetRange(), projectile.GetDamage());
            Deflect(projectile);
            var hitCount = GetHitCount(projectile);
            hitCount++;
            SetHitCount(projectile, hitCount);
        }
        public override void PostContactGround(Entity projectile, Vector3 velocity)
        {
            base.PostContactGround(projectile, velocity);
            Deflect(projectile);
            Explode(projectile, projectile.GetRange(), projectile.GetDamage());
            var hitCount = GetHitCount(projectile);
            hitCount++;
            SetHitCount(projectile, hitCount);
        }
        private void Deflect(Entity projectile)
        {
            projectile.PlaySound(VanillaSoundID.reflection);
            var level = projectile.Level;
            var grids = level.GetAllGrids().Where(g => !g.IsWater() && !g.IsCloud()).RandomTake(1, projectile.RNG);
            if (grids == null)
            {
                projectile.Velocity = VanillaProjectileExt.GetLobVelocityByTime(projectile.Position, projectile.Parent.Position, 45, projectile.GetGravity());
            }
            foreach (var grid in grids)
            {
                float X = level.GetEntityColumnX(grid.Column);
                float Z = level.GetEntityLaneZ(grid.Lane);
                var targetPos = new Vector3(X, 0, Z);
                projectile.Velocity = VanillaProjectileExt.GetLobVelocityByTime(projectile.Position, targetPos, 45, projectile.GetGravity());
            }
        }
        public static DamageOutput[] Explode(Entity entity, float range, float damage)
        {
            var damageEffects = new DamageEffectList(VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.EXPLOSION);
            var damageOutputs = entity.Explode(entity.Position, range, entity.GetFaction(), damage, damageEffects);
            Explosion.Spawn(entity, entity.GetCenter(), range);
            entity.PlaySound(VanillaSoundID.explosion);
            entity.Level.ShakeScreen(10, 0, 15);

            return damageOutputs;
        }
        public static int GetHitCount(Entity entity) => entity.GetBehaviourField<int>(PROP_HIT_COUNT);
        public static void SetHitCount(Entity entity, int value) => entity.SetBehaviourField(PROP_HIT_COUNT, value);
        public static readonly VanillaEntityPropertyMeta<int> PROP_HIT_COUNT = new VanillaEntityPropertyMeta<int>("HitCount");
    }
}

