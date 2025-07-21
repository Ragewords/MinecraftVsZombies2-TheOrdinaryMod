using MVZ2.GameContent.Damages;
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

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.tanookiZombieStone)]
    public class TanookiZombieStoneBuff : BuffDefinition
    {
        public TanookiZombieStoneBuff(string nsp, string name) : base(nsp, name)
        {
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
            AddTrigger(VanillaLevelCallbacks.PRE_PROJECTILE_HIT, PreProjectileHitCallback, filter: VanillaProjectileID.knife);
            AddModifier(new BooleanModifier(EngineEntityProps.INVINCIBLE, true));
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
        }
        public override void PostRemove(Buff buff)
        {
            base.PostRemove(buff);
            var entity = buff.GetEntity();
            if (entity == null)
                return;
            if (entity.IsDead)
                return;
            entity.CreateFragmentAndPlay(entity.GetCenter(), entity.GetFragmentID(), 250);
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
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("Timer");
    }
}
