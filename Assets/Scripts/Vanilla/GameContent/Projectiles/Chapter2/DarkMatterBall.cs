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
            if (plantEvoke || hitResult.Other.IsInvincible())
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
        }
        private void Deflect(Entity projectile)
        {
            projectile.PlaySound(VanillaSoundID.reflection);
            var level = projectile.Level;
            float X = level.GetEntityColumnX(3);
            float Z = level.GetEntityLaneZ(4);
            var targetPos = projectile.Parent.ExistsAndAlive() ? projectile.Parent.Position : new Vector3(X, 0, Z);
            projectile.Velocity = VanillaProjectileExt.GetLobVelocityByTime(projectile.Position, targetPos, 45, projectile.GetGravity());
            projectile.AddBuff<InvertedMirrorBuff>();
            projectile.SetDamage(1000);
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
                if (result.GetDefinitionID() == VanillaBossID.theEye)
                {
                    TheEye.FakeStun(result);
                }
            }
            var param = entity.GetSpawnParams();
            param.SetProperty(EngineEntityProps.SIZE, Vector3.one * (range * 2));
            param.SetProperty(EngineEntityProps.TINT, Color.black);
            entity.Spawn(VanillaEffectID.explosion, entity.GetCenter(), param);
            entity.PlaySound(VanillaSoundID.explosion);
            entity.Level.ShakeScreen(10, 0, 15);
            entity.Spawn(VanillaEffectID.confusingPlanet, entity.GetCenter());

            return damageOutputs;
        }
        public static NamespaceID ID => VanillaProjectileID.fireCharge;
    }
}

