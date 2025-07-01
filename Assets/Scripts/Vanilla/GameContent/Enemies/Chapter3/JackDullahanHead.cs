using MVZ2.GameContent.Buffs.Enemies;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Enemies;
using MVZ2.Vanilla.Entities;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.jackDullahanHead)]
    public class JackDullahanHead : MeleeEnemy
    {
        public JackDullahanHead(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.POST_ENEMY_MELEE_ATTACK, PostEnemyMeleeAttackCallback);
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            var buff = entity.AddBuff<FlyBuff>();
            buff.SetProperty(FlyBuff.PROP_TARGET_HEIGHT, 30f);
        }
        public override void PreTakeDamage(DamageInput input, CallbackResult result)
        {
            base.PreTakeDamage(input, result);
            if (input.Effects.HasEffect(VanillaDamageEffects.GOLD))
            {
                input.Multiply(3);
            }
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
            {
                return;
            }
            if (info.Effects.HasEffect(VanillaDamageEffects.DROWN))
                return;
            entity.ShootProjectile(new ShootParams()
            {
                projectileID = VanillaProjectileID.witherSkull,
                position = entity.Position,
                velocity = entity.GetFacingDirection() * 10,
                faction = VanillaFactions.NEUTRAL,
                damage = entity.GetMaxHealth() * 10,
                soundID = VanillaSoundID.witherShoot
            });
            entity.Remove();
        }
        private void PostEnemyMeleeAttackCallback(VanillaLevelCallbacks.EnemyMeleeAttackParams param, CallbackResult result)
        {
            var enemy = param.enemy;
            var target = param.target;
            if (!enemy.IsEntityOf(VanillaEnemyID.jackDullahanHead))
                return;
            if (!target.IsHostile(enemy))
                return;
            target.InflictWither(30);
        }
    }
}
