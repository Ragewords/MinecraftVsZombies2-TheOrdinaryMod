using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.curseer)]
    public class Curseer : MeleeEnemy
    {
        public Curseer(string nsp, string name) : base(nsp, name)
        {
        }

        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetStateTimer(entity, new FrameTimer(CAST_COOLDOWN));
        }
        protected override int GetActionState(Entity enemy)
        {
            var state = base.GetActionState(enemy);
            if (state == STATE_WALK && IsCasting(enemy))
            {
                return STATE_CAST;
            }
            return state;
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetAnimationInt("HealthState", entity.GetHealthState(2));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);

            if (entity.State == STATE_WALK)
            {
                var stateTimer = GetStateTimer(entity);
                stateTimer.Run(entity.GetAttackSpeed());
                if (stateTimer.Expired)
                {
                    var heal_target = entity.Level.FindEntities(e => e.IsFriendly(entity) && e.Type == EntityTypes.ENEMY && e.ID != entity.ID && !e.IsNotActiveEnemy() && !e.IsDead && e.Health < e.GetMaxHealth()).RandomTake(1, entity.RNG);
                    if (heal_target == null)
                    {
                        stateTimer.Frame = CONTROL_DETECT_TIME;
                    }
                    else
                    {
                        foreach (var target in heal_target)
                        {
                            target.HealEffects(entity.GetDamage() * 2, entity);
                            entity.PlaySound(VanillaSoundID.heal);
                            SetCasting(entity, true);
                        }
                        stateTimer.Frame = CONTROL_DETECT_TIME;
                    }
                }
            }
            else if (entity.State == STATE_CAST)
            {
                var stateTimer = GetStateTimer(entity);
                stateTimer.Run(entity.GetAttackSpeed());
                if (stateTimer.Expired)
                {
                    EndCasting(entity);
                }
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (entity.State == STATE_CAST)
            {
                EndCasting(entity);
            }
        }
        private void EndCasting(Entity entity)
        {
            var stateTimer = GetStateTimer(entity);
            stateTimer.Reset();
            SetCasting(entity, false);
        }

        public static void SetCasting(Entity entity, bool timer) => entity.SetBehaviourField(ID, PROP_CASTING, timer);
        public static bool IsCasting(Entity entity) => entity.GetBehaviourField<bool>(ID, PROP_CASTING);
        public static void SetStateTimer(Entity entity, FrameTimer timer) => entity.SetBehaviourField(ID, PROP_STATE_TIMER, timer);
        public static FrameTimer GetStateTimer(Entity entity) => entity.GetBehaviourField<FrameTimer>(ID, PROP_STATE_TIMER);

        #region ����
        private const int CAST_COOLDOWN = 300;
        private const int CONTROL_DETECT_TIME = 30;

        public const int STATE_WALK = VanillaEntityStates.WALK;
        public const int STATE_ATTACK = VanillaEntityStates.ATTACK;
        public const int STATE_CAST = VanillaEntityStates.MESMERIZER_CAST;
        public static readonly NamespaceID ID = VanillaEnemyID.curseer;
        public static readonly VanillaEntityPropertyMeta<bool> PROP_CASTING = new VanillaEntityPropertyMeta<bool>("Casting");
        public static readonly VanillaEntityPropertyMeta<EntityID> PROP_ORB = new VanillaEntityPropertyMeta<EntityID>("Orb");
        public static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_STATE_TIMER = new VanillaEntityPropertyMeta<FrameTimer>("StateTimer");
        #endregion ����
    }
}