using MVZ2.GameContent.Armors;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.skelebomb)]
    public class Skelebomb : MeleeEnemy
    {
        public Skelebomb(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetStateTimer(entity, new FrameTimer(CAST_COOLDOWN));
            entity.SetAnimationBool("HoldingBomb", true);
            SetExplodingRNG(entity, -150);
            entity.EquipMainArmor(VanillaArmorID.skelebombCap);
        }
        protected override int GetActionState(Entity enemy)
        {
            var state = base.GetActionState(enemy);
            if (IsCasting(enemy))
            {
                return VanillaEntityStates.SKELEBOMB_EXPLODE;
            }
            return state;
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationInt("HealthState", entity.GetHealthState(2));
            if (entity.IsDead)
                return;
            var stateTimer = GetStateTimer(entity);
            if (entity.State == VanillaEntityStates.SKELEBOMB_EXPLODE)
            {
                stateTimer.Run();
                if (stateTimer.Expired)
                {
                    Explode(entity, entity.GetDamage() * 3, entity.GetFaction());
                    entity.Die(entity);
                    entity.SetAnimationBool("HoldingBomb", false);
                    entity.Level.ShakeScreen(5, 0, 20);
                }
            }
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            if (entity.IsDead)
                return;
            if (entity.State != VanillaEntityStates.SKELEBOMB_EXPLODE)
            {
                var exploding = GetExplodingRNG(entity);
                exploding++;
                SetExplodingRNG(entity, exploding);
                var explode_threshold = -114514;
                if (exploding >= 0 && exploding < 150)
                    explode_threshold = 5;
                else if (exploding >= 270 && exploding < 570)
                    explode_threshold = 95;
                var rng = entity.RNG.Next(CAST_RNG);
                if (rng < explode_threshold)
                {
                    StartCasting(entity);
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            if (info.Effects.HasEffect(VanillaDamageEffects.DROWN))
                return;
            if (entity.State == VanillaEntityStates.SKELEBOMB_EXPLODE)
            {
                Explode(entity, entity.GetDamage() * 3, entity.GetFaction());
                entity.SetAnimationBool("HoldingBomb", false);
                entity.Level.ShakeScreen(5, 0, 20);
                EndCasting(entity);
            }
            else
            {
                var offset = BOMB_OFFSET;
                offset.x *= entity.GetFacingX();
                var bomb = entity.SpawnWithParams(VanillaEnemyID.bomb, entity.Position + offset);
                bomb.Velocity = entity.Velocity;
                entity.SetAnimationBool("HoldingBomb", false);
            }
        }
        public static void SetCasting(Entity entity, bool timer)
        {
            entity.SetBehaviourField(ID, PROP_CASTING, timer);
        }
        public static bool IsCasting(Entity entity)
        {
            return entity.GetBehaviourField<bool>(ID, PROP_CASTING);
        }
        public static void SetStateTimer(Entity entity, FrameTimer timer)
        {
            entity.SetBehaviourField(ID, PROP_STATE_TIMER, timer);
        }
        public static FrameTimer GetStateTimer(Entity entity)
        {
            return entity.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);
        }
        public static void SetExplodingRNG(Entity entity, int timer)
        {
            entity.SetBehaviourField(ID, PROP_EXPLODING, timer);
        }
        public static int GetExplodingRNG(Entity entity)
        {
            return entity.GetBehaviourField<int>(ID, PROP_EXPLODING);
        }

        public static void StartCasting(Entity entity)
        {
            SetCasting(entity, true);
            entity.PlaySound(VanillaSoundID.fuse);
            var stateTimer = GetStateTimer(entity);
            stateTimer.ResetTime(CAST_TIME);
        }
        private void EndCasting(Entity entity)
        {
            SetCasting(entity, false);
            var stateTimer = GetStateTimer(entity);
            stateTimer.ResetTime(CAST_COOLDOWN);
        }
        public static void Explode(Entity entity, float damage, int faction)
        {
            var scale = entity.GetScale();
            var scaleX = Mathf.Abs(scale.x);
            var range = entity.GetRange() * scaleX;
            entity.Level.Explode(entity.GetCenter(), range, faction, damage, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE), entity);

            var explosion = entity.Level.Spawn(VanillaEffectID.explosion, entity.GetCenter(), entity);
            explosion.SetSize(Vector3.one * (range * 2));
            entity.PlaySound(VanillaSoundID.explosion, scaleX == 0 ? 1000 : 1 / (scaleX));
        }

        #region 常量
        private const int CAST_COOLDOWN = 300;
        private const int CAST_TIME = 30;
        private const int CAST_RNG = 3000;
        private Vector3 BOMB_OFFSET = new Vector3(32, 40, 0);
        public static readonly NamespaceID ID = VanillaEnemyID.skelebomb;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_CASTING = new VanillaEntityPropertyMeta<bool>("Casting");
        public static readonly VanillaEntityPropertyMeta<int> PROP_EXPLODING = new VanillaEntityPropertyMeta<int>("Exploding");
        #endregion 常量
    }
}
