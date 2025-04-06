using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2.Vanilla.Models;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;
using MVZ2Logic.Models;
using MVZ2.GameContent.Models;
using Tools;
using MVZ2.GameContent.Enemies;
using MVZ2.Vanilla.Audios;
using PVZEngine.Callbacks;
using PVZEngine.Damages;
using MVZ2.GameContent.Damages;

namespace MVZ2.GameContent.Buffs.Enemies
{
    [BuffDefinition(VanillaBuffNames.terrorNest)]
    public class TerrorNestBuff : BuffDefinition
    {
        public TerrorNestBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new BooleanModifier(VanillaEntityProps.AI_FROZEN, true));
            AddModelInsertion(LogicModelHelper.ANCHOR_CENTER, VanillaModelKeys.terrorParasitized, VanillaModelID.terrorParasitized);
            AddModifier(new ColorModifier(EngineEntityProps.COLOR_OFFSET, new Color(1, 1, 1, 0.5f)));
            AddTrigger(LevelCallbacks.POST_ENTITY_DEATH, PostEntityDeathCallback);
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_SPAWN_TIMER, new FrameTimer(MAX_SPAWN_TIMEOUT));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();
            var timer = buff.GetProperty<FrameTimer>(PROP_SPAWN_TIMER);
            timer.Run();
            var iconModel = buff.GetInsertedModel(VanillaModelKeys.terrorParasitized);
            if (iconModel != null)
            {
                iconModel.SetAnimationBool("Awake", timer.Frame < 15);
            }
            if (timer.Expired)
            {
                var parasite = entity.Level.Spawn(VanillaEnemyID.parasiteTerror, entity.GetCenter(), entity);
                parasite.SetFactionAndDirection(entity.GetFaction());
                entity.PlaySound(VanillaSoundID.bloody);
                entity.TakeDamage(5, new DamageEffectList(VanillaDamageEffects.IGNORE_ARMOR, VanillaDamageEffects.SELF_DAMAGE, VanillaDamageEffects.MUTE), entity);
                entity.EmitBlood();
                timer.Reset();
            }
            if (entity.IsDead)
                buff.Remove();
        }
        private void PostEntityDeathCallback(Entity entity, DeathInfo info)
        {
            foreach (var buff in entity.GetBuffs<TerrorNestBuff>())
            {
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta PROP_SPAWN_TIMER = new VanillaBuffPropertyMeta("SpawnTimer");
        public const int MAX_SPAWN_TIMEOUT = 60;
    }
}
