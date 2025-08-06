using System.Linq;
using MVZ2.GameContent.Buffs.Contraptions;
using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Contraptions
{
    [EntityBehaviourDefinition(VanillaContraptionNames.revivegrave)]
    public class Revivegrave : ContraptionBehaviour
    {
        public Revivegrave(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.POST_ENEMY_FAINT, PostEnemyFaintCallback);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetReviveCooldown(entity, new FrameTimer(30));
        }
        protected override void UpdateAI(Entity entity)
        {
            base.UpdateAI(entity);
            GetReviveCooldown(entity).Run(entity.GetProduceSpeed());
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            var cooldown = GetReviveCooldown(entity);
            entity.SetAnimationBool("Active", cooldown.Expired);
            entity.SetAnimationFloat("MarkBlend", cooldown.GetPassedPercentage());
        }
        public override void PostTakeDamage(DamageOutput result)
        {
            base.PostTakeDamage(result);
            if (result.HasAnyFatal())
                result.Entity.Spawn(VanillaContraptionID.necrotombstone, result.Entity.Position);
        }
        private void PostEnemyFaintCallback(EntityCallbackParams param, CallbackResult result)
        {
            var entity = param.entity;
            var level = entity.Level;
            var graves = level.FindEntities(e => e.IsEntityOf(VanillaContraptionID.revivegrave) && e.GetLane() == entity.GetLane() && e.IsHostile(entity));
            foreach (var grave in graves)
            {
                ReviveEnemy(grave, entity.GetDefinitionID(), entity.GetMaxHealth());
            }
        }
        private void ReviveEnemy(Entity grave, NamespaceID id, float maxHealth)
        {
            if (!GetReviveCooldown(grave).Expired)
                return;
            var pos = grave.Position;
            pos.y = grave.GetGroundY() - 100;
            var revived = grave.SpawnWithParams(id, pos);
            revived.AddBuff<NecrotombstoneRisingBuff>();
            revived.UpdateModel();
            revived.PlaySound(VanillaSoundID.dirtRise);
            revived.PlaySound(VanillaSoundID.revived);
            var newTimer = Mathf.RoundToInt(maxHealth);
            GetReviveCooldown(grave).ResetTime(newTimer);
        }
        protected override void OnEvoke(Entity entity)
        {
            base.OnEvoke(entity);
            var cooldown = GetReviveCooldown(entity);
            cooldown.ResetTime(30);
            for (var lane = 0; lane < entity.Level.GetMaxLaneCount(); lane++)
            {
                var enemy = entity.Level.FindEntities(e => IsEvocationTarget(entity, e) && e.GetLane() == lane)
                .OrderBy(e => e.Position.x).Take(1);
                foreach (var e in enemy)
                {
                    e.Die(new DamageEffectList(VanillaDamageEffects.NO_DEATH_TRIGGER), entity);
                    var buff = e.AddBuff<RevivegraveReviveBuff>();
                    RevivegraveReviveBuff.SetFaction(buff, entity.GetFaction());
                }
            }
        }
        private static bool IsEvocationTarget(Entity self, Entity target)
        {
            if (target == null)
                return false;
            if (target.IsDead)
                return false;
            if (target.Type != EntityTypes.ENEMY)
                return false;
            if (!self.IsHostile(target))
                return false;
            if (target.IsNotActiveEnemy())
                return false;
            return true;
        }
        public static FrameTimer GetReviveCooldown(Entity entity) => entity.GetBehaviourField<FrameTimer>(PROP_REVIVE_COOLDOWN);
        public static void SetReviveCooldown(Entity entity, FrameTimer timer) => entity.SetBehaviourField(PROP_REVIVE_COOLDOWN, timer);
        private static readonly VanillaEntityPropertyMeta<FrameTimer> PROP_REVIVE_COOLDOWN = new VanillaEntityPropertyMeta<FrameTimer>("ReviveCooldown");
    }
}
