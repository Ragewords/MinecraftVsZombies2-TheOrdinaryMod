using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Effects;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Buffs;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;

namespace MVZ2.GameContent.Buffs
{
    [BuffDefinition(VanillaBuffNames.aboutToExplode)]
    public class AboutToExplodeBuff : BuffDefinition
    {
        public AboutToExplodeBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new ColorModifier(EngineEntityProps.COLOR_OFFSET, PROP_COLOR));
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_EXPLODE_TIME, MAX_EXPLODE_TIME);
            UpdateMultipliers(buff);
            buff.GetEntity().PlaySound(VanillaSoundID.parabotTick);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            UpdateExplosion(buff);
            UpdateMultipliers(buff);
        }
        private void UpdateExplosion(Buff buff)
        {
            var explodeTime = GetExplodeTime(buff);
            explodeTime--;
            buff.SetProperty(PROP_EXPLODE_TIME, explodeTime);

            if (explodeTime <= 0)
            {
                ParabotExplode(buff);
                buff.Remove();
            }
        }
        private void ParabotExplode(Buff buff)
        {
            var entity = buff.GetEntity();
            if (entity == null)
                return;
            var level = entity.Level;
            var range = 120;
            Vector3 centerPos = entity.GetCenter();
            entity.Explode(centerPos, range, GetFaction(buff), 600, new DamageEffectList(VanillaDamageEffects.EXPLOSION, VanillaDamageEffects.DAMAGE_BOTH_ARMOR_AND_BODY, VanillaDamageEffects.MUTE));
            var explosion = level.Spawn(VanillaEffectID.explosion, centerPos, entity);
            explosion.SetSize(Vector3.one * (range * 2));
            entity.PlaySound(VanillaSoundID.explosion);
            entity.Die();
            level.ShakeScreen(10, 0, 15);
        }
        private int GetFaction(Buff buff)
        {
            return buff.GetProperty<int>(PROP_FACTION);
        }
        private int GetExplodeTime(Buff buff)
        {
            return buff.GetProperty<int>(PROP_EXPLODE_TIME);
        }
        private void PostEntityDeathCallback(LevelCallbacks.PostEntityDeathParams param, CallbackResult result)
        {
            var entity = param.entity;
            var info = param.deathInfo;
            var buffs = entity.GetBuffs<AboutToExplodeBuff>();
            foreach (var buff in buffs)
            {
                if (GetExplodeTime(buff) > 0)
                {
                    ParabotExplode(buff);
                }
            }
            entity.RemoveBuffs(buffs);
        }
        private void UpdateMultipliers(Buff buff)
        {
            var time = buff.GetProperty<int>(PROP_EXPLODE_TIME);
            var alpha = (Mathf.Sin(time * 60 * Mathf.Deg2Rad) + 1) * 0.5f;
            buff.SetProperty(PROP_COLOR, new Color(1, 1, 1, alpha));
        }
        public static readonly VanillaBuffPropertyMeta<Color> PROP_COLOR = new VanillaBuffPropertyMeta<Color>("Color");
        public static readonly VanillaBuffPropertyMeta<int> PROP_FACTION = new VanillaBuffPropertyMeta<int>("Faction");
        public static readonly VanillaBuffPropertyMeta<int> PROP_EXPLODE_TIME = new VanillaBuffPropertyMeta<int>("ExplodeTime");
        public const int MAX_EXPLODE_TIME = 24;
    }
}
