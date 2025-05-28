using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Modifiers;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.seijaYinyangOrb)]
    public class SeijaYinyangOrb : EnemyBehaviour
    {
        public SeijaYinyangOrb(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(LevelCallbacks.POST_ENTITY_INIT, PostPlantInitCallback, filter: EntityTypes.PLANT);
            AddModifier(new IntModifier(EngineEntityProps.COLLISION_DETECTION, NumberOperator.Set, EntityCollisionHelper.DETECTION_IGNORE, VanillaModifierPriorities.FORCE));
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 20f);
            SetShootTimer(entity, new FrameTimer(300));
            SetRepeatTime(entity, 5);
            SetRepeatShootTimer(entity, new FrameTimer(5));
            var x = entity.Level.GetEntityColumnX(entity.Level.GetMaxColumnCount() - 2);
            var z = entity.Level.GetEntityLaneZ(entity.Level.GetMaxLaneCount() / 2);
            var y = 20;
            SetMoveTarget(entity, new Vector3(x, y, z));
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            if (!entity.Parent.ExistsAndAlive())
            {
                entity.Die(new DamageEffectList(VanillaDamageEffects.NO_NEUTRALIZE), entity);
                return;
            }

            // Move.
            Vector3 target = GetMoveTarget(entity);
            var pos = entity.Position;
            pos.x = pos.x * 0.6f + target.x * 0.4f;
            pos.z = pos.z * 0.6f + target.z * 0.4f;
            entity.Position = pos;

            Shoot(entity);
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
        private void PostPlantInitCallback(EntityCallbackParams param, CallbackResult result)
        {
            var entity = param.entity;
            if (entity.IsOnWater())
                return;
            foreach (Entity orb in entity.Level.FindEntities(VanillaEnemyID.seijaYinyangOrb))
            {
                SetMoveTarget(orb, entity.Position);
            }
        }
        private void Shoot(Entity entity)
        {
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
        private static Vector3 GetMoveTarget(Entity boss)
        {
            return boss.GetBehaviourField<Vector3>(ID, PROP_MOVE_TARGET);
        }
        private static void SetMoveTarget(Entity boss, Vector3 target)
        {
            boss.SetBehaviourField(ID, PROP_MOVE_TARGET, target);
        }
        public static float GetOrbitAngle(Entity entity) => entity.GetBehaviourField<float>(PROP_ORBIT_ANGLE);
        public static void SetOrbitAngle(Entity entity, float value) => entity.SetBehaviourField(PROP_ORBIT_ANGLE, value);
        public static float GetRepeatTime(Entity entity) => entity.GetBehaviourField<float>(REPEAT_TIME);
        public static void SetRepeatTime(Entity entity, float value) => entity.SetBehaviourField(REPEAT_TIME, value);
        public static FrameTimer GetShootTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, SHOOT_TIMER);
        public static void SetShootTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(ID, SHOOT_TIMER, value);
        public static FrameTimer GetRepeatShootTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, REPEAT_SHOOT_TIMER);
        public static void SetRepeatShootTimer(Entity entity, FrameTimer value) => entity.SetBehaviourField(ID, REPEAT_SHOOT_TIMER, value);

        private static readonly VanillaEntityPropertyMeta<float> PROP_ORBIT_ANGLE = new VanillaEntityPropertyMeta<float>("OrbitAngle");
        private static readonly VanillaEntityPropertyMeta<FrameTimer> SHOOT_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("ShootTimer");
        private static readonly VanillaEntityPropertyMeta<FrameTimer> REPEAT_SHOOT_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("RepeatShootTimer");
        private static readonly VanillaEntityPropertyMeta<float> REPEAT_TIME = new VanillaEntityPropertyMeta<float>("RepeatTime");
        private static readonly VanillaEntityPropertyMeta<Vector3> PROP_MOVE_TARGET = new VanillaEntityPropertyMeta<Vector3>("MoveTarget");
        public const float ORBIT_DISTANCE = 80;
        public const float ORBIT_ANGLE_SPEED = -2;
        private static readonly NamespaceID ID = VanillaEnemyID.seijaYinyangOrb;
    }
}
