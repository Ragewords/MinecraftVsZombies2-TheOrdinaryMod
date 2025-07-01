using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Shells;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Callbacks;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.tanookiZombieStone)]
    public class TanookiZombieStoneBuff : BuffDefinition
    {
        public TanookiZombieStoneBuff(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(VanillaLevelCallbacks.PRE_ENTITY_TAKE_DAMAGE, PreEntityTakeDamageCallback);
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
            AddModifier(new NamespaceIDModifier(EngineEntityProps.SHELL, VanillaShellID.stone));
            AddModifier(new FloatModifier(VanillaEntityProps.MASS, NumberOperator.Add, 1));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            var entity = buff.GetEntity();
            if (entity == null)
                return;
            entity.PlaySound(VanillaSoundID.gnawedLeafStone);
            buff.SetProperty(PROP_TIMER, new FrameTimer(300));
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
            entity.GetOrCreateFragment();
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var damage = param.input;
            var entity = damage.Entity;
            foreach (var buff in entity.GetBuffs<TanookiZombieStoneBuff>())
            {
                AddTakenDamage(buff, damage.Amount);
                entity.DamageBlink();
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
        public const float MAX_DAMAGE = 900;
        public static float GetTakenDamage(Buff buff) => buff.GetProperty<float>(PROP_TAKEN_DAMAGE);
        public static void SetTakenDamage(Buff buff, float value) => buff.SetProperty(PROP_TAKEN_DAMAGE, value);
        public static void AddTakenDamage(Buff buff, float value) => SetTakenDamage(buff, GetTakenDamage(buff) + value);
        public static readonly VanillaBuffPropertyMeta<float> PROP_TAKEN_DAMAGE = new VanillaBuffPropertyMeta<float>("takenDamage");
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("Timer");
    }
}
