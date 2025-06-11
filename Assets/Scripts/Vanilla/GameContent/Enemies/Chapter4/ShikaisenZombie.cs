using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using UnityEngine;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.shikaisenZombie)]
    public class ShikaisenZombie : MeleeEnemy
    {
        public ShikaisenZombie(string nsp, string name) : base(nsp, name)
        {
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            SetStaff(entity, true);
            SetPot(entity, true);
        }
        protected override void UpdateAI(Entity enemy)
        {
            base.UpdateAI(enemy);
            if (HasStaff(enemy) && enemy.Health <= enemy.GetMaxHealth() * 0.5f && enemy.IsOnGround)
            {
                SpawnStaff(enemy);
                SetStaff(enemy, false);
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelHealthStateByCount(2);
            entity.SetAnimationBool("HasStaff", HasStaff(entity));
            entity.SetAnimationBool("HasPot", HasPot(entity));
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            if (HasPot(entity))
            {
                SpawnPot(entity);
                SetPot(entity, false);
            }
        }
        public static Entity SpawnStaff(Entity entity)
        {
            var pos = entity.Position + entity.GetFacingDirection() * 30;
            var staff = entity.Spawn(VanillaEnemyID.shikaisenStaff, pos);
            staff.PlaySound(VanillaSoundID.wood);
            staff.SetFaction(entity.GetFaction());
            return staff;
        }
        public static Entity SpawnPot(Entity entity)
        {
            var pos = entity.Position + new Vector3(-20 * entity.GetFacingX(), 44);
            var pot = entity.Spawn(VanillaEnemyID.shikaisenPot, pos);
            pot.SetParent(entity);
            pot.SetFaction(entity.GetFaction());
            return pot;
        }
        public static bool HasStaff(Entity enemy) => enemy.GetBehaviourField<bool>(PROP_HAS_STAFF);
        public static void SetStaff(Entity enemy, bool value) => enemy.SetBehaviourField(PROP_HAS_STAFF, value);
        public static bool HasPot(Entity enemy) => enemy.GetBehaviourField<bool>(PROP_HAS_POT);
        public static void SetPot(Entity enemy, bool value) => enemy.SetBehaviourField(PROP_HAS_POT, value);
        public static readonly VanillaEntityPropertyMeta<bool> PROP_HAS_STAFF = new VanillaEntityPropertyMeta<bool>("HasStaff");
        public static readonly VanillaEntityPropertyMeta<bool> PROP_HAS_POT = new VanillaEntityPropertyMeta<bool>("HasPot");
    }
}
