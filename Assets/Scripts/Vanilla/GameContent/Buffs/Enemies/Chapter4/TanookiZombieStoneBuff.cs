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
            AddModifier(new FloatModifier(VanillaEnemyProps.SPEED, NumberOperator.Multiply, 0.3f));
            AddModifier(new FloatModifier(VanillaEntityProps.MASS, NumberOperator.Add, 1));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            var entity = buff.GetEntity();
            if (entity == null)
                return;
            entity.PlaySound(VanillaSoundID.gnawedLeafStone);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();
            var damage = GetTakenDamage(buff);
            if (damage >= MAX_DAMAGE)
            {
                buff.Remove();
                entity.GetOrCreateFragment();
            }
        }
        private void PreEntityTakeDamageCallback(VanillaLevelCallbacks.PreTakeDamageParams param, CallbackResult result)
        {
            var damage = param.input;
            var entity = damage.Entity;
            foreach (var buff in entity.GetBuffs<TanookiZombieStoneBuff>())
            {
                AddTakenDamage(buff, damage.Amount);
                result.SetFinalValue(false);
            }
        }
        private void PostEntityDeathCallback(LevelCallbacks.PostEntityDeathParams param, CallbackResult result)
        {
            var entity = param.entity;
            entity.RemoveBuffs<TanookiZombieStoneBuff>();
        }
        public const float MAX_DAMAGE = 900;
        public static float GetTakenDamage(Buff buff) => buff.GetProperty<float>(PROP_TAKEN_DAMAGE);
        public static void SetTakenDamage(Buff buff, float value) => buff.SetProperty(PROP_TAKEN_DAMAGE, value);
        public static void AddTakenDamage(Buff buff, float value) => SetTakenDamage(buff, GetTakenDamage(buff) + value);
        public static readonly VanillaBuffPropertyMeta<float> PROP_TAKEN_DAMAGE = new VanillaBuffPropertyMeta<float>("takenDamage");
    }
}
