using MVZ2.GameContent.Buffs.Contraptions;
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

            bool canRevive = false;
            var reviveBuffs = enemy.GetBuffs<ShikaisenReviveBuff>();
            foreach (var buff in reviveBuffs)
            {
                if (ShikaisenReviveBuff.GetSource(buff).GetEntity(enemy.Level).ExistsAndAlive())
                canRevive = true;
            }
            if (canRevive)
                return;
            if (HasPot(enemy) && enemy.IsDead)
            {
                enemy.IsDead = false;
                SpawnPot(enemy);
                SetPot(enemy, false);
                enemy.AddBuff<ShikaisenInvincibleBuff>();
            }
            var children = enemy.GetChildren();
            foreach (var child in children)
            {
                if (child.IsEntityOf(VanillaEnemyID.shikaisenPot))
                {
                    if (child.IsDead)
                        enemy.Die(child);
                }
            }
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);
            entity.SetModelDamagePercent();
            entity.SetModelProperty("NoStaff", !HasStaff(entity));
            entity.SetAnimationBool("HasPot", HasPot(entity));
        }
        public static Entity SpawnStaff(Entity entity)
        {
            var pos = entity.Position + entity.GetFacingDirection() * 30;
            var staff = entity.SpawnWithParams(VanillaEnemyID.shikaisenStaff, pos);
            staff.PlaySound(VanillaSoundID.wood);
            return staff;
        }
        public static Entity SpawnPot(Entity entity)
        {
            var pos = entity.Position + new Vector3(-20 * entity.GetFacingX(), 44);
            var pot = entity.SpawnWithParams(VanillaEnemyID.shikaisenPot, pos);
            pot.SetParent(entity);
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
