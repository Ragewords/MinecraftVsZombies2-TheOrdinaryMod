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
            entity.SetModelDamagePercent();
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
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
            else if (entity.State == VanillaEntityStates.ATTACK)
            {
                if (entity.RNG.Next(1, 200) == 1)
                {
                    StartCasting(entity);
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.NO_DEATH_TRIGGER))
                return;
            if (IsCasting(entity))
            {
                EndCasting(entity);
                Explode(entity, entity.GetDamage() * 3, entity.GetFaction());
                entity.SetAnimationBool("HoldingBomb", false);
                entity.Level.ShakeScreen(5, 0, 20);
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
            entity.Explode(entity.GetCenter(), range, faction, damage, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.DAMAGE_BODY_AFTER_ARMOR_BROKEN, VanillaDamageEffects.MUTE));

            var param = entity.GetSpawnParams();
            param.SetProperty(EngineEntityProps.SIZE, Vector3.one * (range * 2));
            entity.Spawn(VanillaEffectID.explosion, entity.GetCenter(), param);
            entity.PlaySound(VanillaSoundID.explosion, scaleX == 0 ? 1000 : 1 / (scaleX));
        }

        #region ����
        private const int CAST_COOLDOWN = 300;
        private const int CAST_TIME = 30;
        private Vector3 BOMB_OFFSET = new Vector3(32, 40, 0);
        public static readonly NamespaceID ID = VanillaEnemyID.skelebomb;
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_CASTING = new VanillaEntityPropertyMeta<bool>("Casting");
        public static readonly VanillaEntityPropertyMeta<int> PROP_EXPLODING = new VanillaEntityPropertyMeta<int>("Exploding");
        #endregion ����
    }
}
