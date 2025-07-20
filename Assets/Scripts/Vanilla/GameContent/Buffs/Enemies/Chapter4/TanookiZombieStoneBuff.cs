using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Enemies;
using MVZ2.GameContent.Projectiles;
using MVZ2.GameContent.Shells;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Contraptions;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.tanookiZombieStone)]
    public class TanookiZombieStoneBuff : BuffDefinition
    {
        public TanookiZombieStoneBuff(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.PRE_ENTITY_TAKE_DAMAGE, PreEntityTakeDamageCallback);
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
            AddTrigger(VanillaLevelCallbacks.PRE_PROJECTILE_HIT, PreProjectileHitCallback, filter: VanillaProjectileID.knife);
            AddModifier(new BooleanModifier(FragmentExt.PROP_NO_DAMAGE_FRAGMENTS, false));
            AddModifier(new NamespaceIDModifier(EngineEntityProps.SHELL, VanillaShellID.stone));
            AddModifier(new FloatModifier(VanillaEntityProps.MASS, NumberOperator.Add, 1));
            AddModifier(new IntModifier(VanillaEnemyProps.STATE_OVERRIDE, NumberOperator.Set, VanillaEntityStates.IDLE));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            var entity = buff.GetEntity();
            if (entity == null)
                return;
            entity.PlaySound(VanillaSoundID.gnawedLeafStone);
            buff.SetProperty(PROP_TIMER, new FrameTimer(90));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var timer = buff.GetProperty<FrameTimer>(PROP_TIMER);
            if (timer == null)
            {
                buff.Remove();
                return;
            }
            timer.Run();
            if (timer.Expired)
            {
                buff.Remove();
            }
            var damage = GetTakenDamage(buff);
            if (damage >= MAX_DAMAGE)
            {
                buff.Remove();
            }
        }
        public override void PostRemove(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();
            if (entity == null)
                return;
            if (entity.IsDead)
                return;
            var damage = GetTakenDamage(buff);
            if (damage <= 0)
                return;
            entity.PlaySound(VanillaSoundID.danmaku);
            entity.CreateFragmentAndPlay(entity.GetCenter(), entity.GetFragmentID(), 250);
            for (int i = 0; i < 15; i++)
            {
                var angle = i * 24;
                var param = entity.GetShootParams();
                param.velocity = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * (5 + i);
                param.projectileID = VanillaProjectileID.seijaBullet;
                param.damage = damage;
                param.position = entity.GetCenter();
                var projectile = entity.ShootProjectile(param);
            }
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var damage = param.input;
            var entity = damage.Entity;
            var amount = damage.Amount;
            foreach (var buff in entity.GetBuffs<TanookiZombieStoneBuff>())
            {
                amount *= damage.HasEffect(VanillaDamageEffects.PUNCH) ? 20 : 1;
                amount *= damage.Source.DefinitionID == VanillaEnemyID.silverfish ? 5 : 1;
                AddTakenDamage(buff, amount);
                entity.DamageBlink();
                entity.AddFragmentTickDamage(amount);
                if (!damage.HasEffect(VanillaDamageEffects.MUTE))
                    entity.PlaySound(VanillaSoundID.stone);
                result.SetFinalValue(false);
            }
        }
        private void PostEntityDeathCallback(LevelCallbacks.PostEntityDeathParams param, CallbackResult result)
        {
            var entity = param.entity;
            var info = param.deathInfo;
            if (info.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            foreach (var buff in entity.GetBuffs<TanookiZombieStoneBuff>())
            {
                buff.Remove();
            }
        }
        private void PreProjectileHitCallback(VanillaLevelCallbacks.PreProjectileHitParams param, CallbackResult result)
        {
            var hitInput = param.hit;
            var other = hitInput.Other;
            if (!other.HasBuff<TanookiZombieStoneBuff>())
                return;
            var knife = hitInput.Projectile;
            knife.Remove();
        }
        public const float MAX_DAMAGE = 900;
        public static float GetTakenDamage(Buff buff) => buff.GetProperty<float>(PROP_TAKEN_DAMAGE);
        public static void SetTakenDamage(Buff buff, float value) => buff.SetProperty(PROP_TAKEN_DAMAGE, value);
        public static void AddTakenDamage(Buff buff, float value) => SetTakenDamage(buff, GetTakenDamage(buff) + value);
        public static readonly VanillaBuffPropertyMeta<float> PROP_TAKEN_DAMAGE = new VanillaBuffPropertyMeta<float>("takenDamage");
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("Timer");
    }
}
