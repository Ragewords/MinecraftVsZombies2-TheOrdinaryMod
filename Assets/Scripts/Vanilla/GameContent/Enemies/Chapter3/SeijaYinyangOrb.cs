using System.Collections.Generic;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Contraptions;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.seijaYinyangOrb)]
    public class SeijaYinyangOrb : EnemyBehaviour
    {
        public SeijaYinyangOrb(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 30);
            SetShootTimer(entity, new FrameTimer(150));
            SetRepeatTime(entity, 5);
            SetRepeatShootTimer(entity, new FrameTimer(5));
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            if (!entity.Parent.ExistsAndAlive())
            {
                entity.Die(new DamageEffectList(VanillaDamageEffects.NO_NEUTRALIZE), entity);
                return;
            }

            // Orbit.
            var angle = GetOrbitAngle(entity);
            angle += ORBIT_ANGLE_SPEED;
            SetOrbitAngle(entity, angle);

            var orbitOffset = Vector2.right.RotateClockwise(angle) * ORBIT_DISTANCE;
            var targetPosition = entity.Parent.Position + new Vector3(orbitOffset.x, 0, orbitOffset.y);
            targetPosition.y = Mathf.Max(targetPosition.y, entity.Level.GetGroundY(targetPosition));
            entity.Position = targetPosition;

            // Shoot.
            Color color = Color.red;
            var timer = GetShootTimer(entity);
            timer.Run();
            if (timer.Expired)
            {
                var reptimes = GetRepeatTime(entity);
                if (reptimes > 0)
                {
                    var reptimer = GetRepeatShootTimer(entity);
                    reptimer.Run();
                    if (reptimer.Expired)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            var shoot_angle = reptimes * 16 + i * 90;
                            var param = entity.GetShootParams();
                            param.projectileID = VanillaProjectileID.seijaBullet;
                            param.position = entity.GetCenter();
                            param.damage = entity.GetDamage();
                            param.velocity = new Vector3(Mathf.Cos(shoot_angle * Mathf.Deg2Rad), 0, Mathf.Sin(shoot_angle * Mathf.Deg2Rad)) * SeijaBullet.LIGHT_SPEED;
                            var bullet = entity.ShootProjectile(param);
                            if (reptimes == 2 || reptimes == 4)
                                color = Color.blue;
                            bullet.SetTint(color);
                        }
                        SetRepeatTime(entity, reptimes - 1);
                        reptimer.Reset();
                        entity.PlaySound(VanillaSoundID.danmaku, volume: 0.5f);
                    }
                }
                else
                {
                    SetRepeatTime(entity, 5);
                    timer.Reset();
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            var smoke = entity.Spawn(VanillaEffectID.smoke, entity.GetCenter());
            smoke.SetSize(entity.GetSize());
            entity.Remove();
        }
        public static float GetOrbitAngle(Entity entity) => entity.GetBehaviourField<float>(PROP_ORBIT_ANGLE);
        public static void SetOrbitAngle(Entity entity, float value) => entity.SetBehaviourField(PROP_ORBIT_ANGLE, value);
        public static float GetRepeatTime(Entity entity) => entity.GetBehaviourField<float>(REPEAT_TIME);
        public static void SetRepeatTime(Entity entity, float value) => entity.SetBehaviourField(REPEAT_TIME, value);
        public static FrameTimer GetShootTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, SHOOT_TIMER);
        public static void SetShootTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(ID, SHOOT_TIMER, value);
        public static FrameTimer GetRepeatShootTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, REPEAT_SHOOT_TIMER);
        public static void SetRepeatShootTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(ID, REPEAT_SHOOT_TIMER, value);

        private static readonly VanillaEntityPropertyMeta PROP_ORBIT_ANGLE = new VanillaEntityPropertyMeta("OrbitAngle");
        private static readonly VanillaEntityPropertyMeta SHOOT_TIMER = new VanillaEntityPropertyMeta("ShootTimer");
        private static readonly VanillaEntityPropertyMeta REPEAT_SHOOT_TIMER = new VanillaEntityPropertyMeta("RepeatShootTimer");
        private static readonly VanillaEntityPropertyMeta REPEAT_TIME = new VanillaEntityPropertyMeta("RepeatTime");
        public const float ORBIT_DISTANCE = 80;
        public const float ORBIT_ANGLE_SPEED = -2;
        private static readonly NamespaceID ID = VanillaEnemyID.seijaYinyangOrb;
    }
}
