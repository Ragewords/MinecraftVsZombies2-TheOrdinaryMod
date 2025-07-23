using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Effects
{
    [EntityBehaviourDefinition(VanillaEffectNames.confusingPlanet)]
    public class ConfusingPlanet : EffectBehaviour
    {
        #region 公有方法
        public ConfusingPlanet(string nsp, string name) : base(nsp, name)
        {
        }
        #endregion
        public override void Init(Entity planet)
        {
            base.Init(planet);
            planet.CollisionMaskHostile =
                EntityCollisionHelper.MASK_VULNERABLE;
        }
        public override void Update(Entity entity)
        {
            base.Update(entity);
            bool inactive = entity.Timeout <= 100;
            entity.SetAnimationBool("Disappear", inactive);

            RotateUpdate(entity, inactive);
        }
        public override void PostCollision(EntityCollision collision, int state)
        {
            base.PostCollision(collision, state);
            var other = collision.Other;
            var self = collision.Entity;
            bool inactive = self.Timeout <= 100;
            if (inactive)
                return;
            if (!other.Exists())
                return;
            if (other.IsEntityOf(VanillaBossID.theGiant) && TheGiant.IsSnake(other))
            {
                other.TakeDamage(COLLIDE_SELF_DAMAGE, new DamageEffectList(VanillaDamageEffects.MUTE), self);
                TheGiant.KillSnake(other);
            }
            if (state != EntityCollisionHelper.STATE_ENTER)
                return;
            other.TakeDamage(self.GetDamage(), new DamageEffectList(), self);
        }
        private void RotateUpdate(Entity entity, bool inactive)
        {
            const float angelicSpeed = -6.5f;
            const float targetRadius = 160;

            var center = GetRotateCenter(entity);
            if (center == null)
                return;
            var center2D = new Vector2(center.x, center.z);
            var pos2D = new Vector2(entity.Position.x, entity.Position.z);
            var velocity2D = new Vector2(entity.Velocity.x, entity.Velocity.z);

            Vector2 pos2Center2D = pos2D - center2D;
            var radius = pos2Center2D.magnitude;
            var direction = pos2Center2D.normalized;

            radius = radius * 0.5f + targetRadius * 0.5f;
            var nextPosition = inactive ? center2D : center2D + direction.RotateClockwise(angelicSpeed) * radius;
            var targetVelocity = nextPosition - pos2D;
            velocity2D = velocity2D * 0.5f + targetVelocity * 0.5f;

            var velocity = new Vector3(velocity2D.x, 0, velocity2D.y);
            entity.Velocity = velocity;
        }
        public const float COLLIDE_SELF_DAMAGE = 600;
        public static void SetRotateCenter(Entity entity, Vector3 value) => entity.SetProperty(PROP_ROTATE_CENTER, value);
        public static Vector3 GetRotateCenter(Entity entity) => entity.GetProperty<Vector3>(PROP_ROTATE_CENTER);
        public static float GetOrbitAngle(Entity entity) => entity.GetBehaviourField<float>(PROP_ORBIT_ANGLE);
        public static void SetOrbitAngle(Entity entity, float value) => entity.SetBehaviourField(PROP_ORBIT_ANGLE, value);
        private static readonly VanillaEntityPropertyMeta<float> PROP_ORBIT_ANGLE = new VanillaEntityPropertyMeta<float>("OrbitAngle");

        public static readonly VanillaBuffPropertyMeta<Vector3> PROP_ROTATE_CENTER = new VanillaBuffPropertyMeta<Vector3>("rotate_center");
    }
}