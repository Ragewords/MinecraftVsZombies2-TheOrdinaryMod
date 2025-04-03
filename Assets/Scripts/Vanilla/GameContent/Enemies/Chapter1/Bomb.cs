using MVZ2.GameContent.Buffs;
using MVZ2.GameContent.Damages;
using MVZ2.GameContent.Detections;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Detections;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Damages;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MVZ2.GameContent.Enemies
{
    [EntityBehaviourDefinition(VanillaEnemyNames.bomb)]
    public class Bomb : EnemyBehaviour
    {
        public Bomb(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new ColorModifier(EngineEntityProps.COLOR_OFFSET, PROP_COLOR_OFFSET));
        }
        public override void Init(Entity entity)
        {
            base.Init(entity);
            entity.Timeout = entity.GetMaxTimeout();
        }
        protected override void UpdateLogic(Entity entity)
        {
            base.UpdateLogic(entity);

            if (entity.Timeout >= 0)
            {
                entity.Timeout--;
                if (entity.Timeout <= 0)
                {
                    entity.Die(entity);
                }
            }
            if (entity.Timeout == 30)
            {
                entity.PlaySound(VanillaSoundID.fuse);
            }
            Color color = Color.clear;
            if (entity.Timeout <= 30 && entity.Timeout % 4 < 2)
            {
                color = Color.white;
            }
            entity.SetProperty(PROP_COLOR_OFFSET, color);
        }
        public override void PostDeath(Entity entity, DeathInfo info)
        {
            base.PostDeath(entity, info);
            if (info.Effects.HasEffect(VanillaDamageEffects.REMOVE_ON_DEATH))
                return;
            if (info.Effects.HasEffect(VanillaDamageEffects.DROWN))
                return;
            Skelebomb.Explode(entity, entity.GetDamage() * 3, entity.GetFaction());
            entity.Remove();
        }
        public static readonly VanillaEntityPropertyMeta PROP_COLOR_OFFSET = new VanillaEntityPropertyMeta("ColorOffset");
    }
}
