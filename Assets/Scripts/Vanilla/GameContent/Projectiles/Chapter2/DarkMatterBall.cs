using System.Linq;
using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Buffs.Projectiles;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Callbacks;
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
        protected override void PostHitEntity(ProjectileHitOutput hitResult, DamageOutput damage)
        {
            base.PostHitEntity(hitResult, damage);
            var bullet = hitResult.Projectile;
            bool plantEvoke = hitResult.Other.Type == EntityTypes.PLANT && hitResult.Other.IsEvoked();
            bool fromParent = hitResult.Other == bullet.Parent;
            if (plantEvoke || hitResult.Other.IsInvincible() || fromParent)
            {
                hitResult.Pierce = true;
                Deflect(bullet);
                return;
            }
            Explode(bullet, bullet.GetRange(), bullet.GetDamage());
        }
        public override void PostContactGround(Entity projectile, Vector3 velocity)
        {
            base.PostContactGround(projectile, velocity);
            Deflect(projectile);
            Explode(projectile, projectile.GetRange(), projectile.GetDamage());
        }
        private void Deflect(Entity projectile)
        {
            projectile.PlaySound(VanillaSoundID.reflection);
            var level = projectile.Level;
            var grids = level.GetAllGrids().Where(g => g.IsEmpty()).RandomTake(1, projectile.RNG);
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
            projectile.AddBuff<InvertedMirrorBuff>();
            projectile.SetDamage(500);
        }
        public static DamageOutput[] Explode(Entity entity, float range, float damage)
        {
            var damageEffects = new DamageEffectList(VanillaDamageEffects.MUTE, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.EXPLOSION);
            var damageOutputs = entity.Explode(entity.Position, range, entity.GetFaction(), damage, damageEffects);
            foreach (var output in damageOutputs)
            {
                var result = output?.Entity;
                if (result.Type == EntityTypes.PLANT && result.CanDeactive())
                {
                    result.ShortCircuit(90);
                    result.PlaySound(VanillaSoundID.powerOff);
                }
                if (entity.Parent.ExistsAndAlive())
                {
                    entity.Parent.HealEffects(50, entity);
                }
            }
            var param = entity.GetSpawnParams();
            param.SetProperty(EngineEntityProps.SIZE, Vector3.one * (range * 2));
            param.SetProperty(EngineEntityProps.TINT, Color.black);
            entity.Spawn(VanillaEffectID.explosion, entity.GetCenter(), param);
            entity.PlaySound(VanillaSoundID.explosion);
            entity.Level.ShakeScreen(10, 0, 15);

            return damageOutputs;
        }
        public static NamespaceID ID => VanillaProjectileID.fireCharge;
    }
}

