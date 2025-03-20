using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MVZ2.GameContent.Buffs
{
    [BuffDefinition(VanillaBuffNames.mesmerize)]
    public class MesmerizeBuff : BuffDefinition
    {
        public MesmerizeBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new ColorModifier(EngineEntityProps.COLOR_OFFSET, new Color(0.5f, 0, 0.5f, 0.5f)));
            AddModifier(new FloatModifier(VanillaEnemyProps.SPEED, NumberOperator.Multiply, PROP_SPEED));
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_SPEED, 1f);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();
            if (entity == null)
                return;

            var mode = buff.GetProperty<int>(PROP_MODE);
            if (mode == MesmerizeModes.SOURCE)
            {
                var sourceID = buff.GetProperty<EntityID>(PROP_SOURCE);
                var source = sourceID?.GetEntity(buff.Level);
                if (source == null || !source.Exists() || source.IsDead)
                {
                    entity.PlaySound(VanillaSoundID.mindClear);
                    buff.Remove();
                    return;
                }
                else
                {
                    buff.SetProperty(PROP_SPEED, 2f);
                }
            }
        }

        public static void SetPermanent(Buff buff)
        {
            buff.SetProperty(PROP_MODE, MesmerizeModes.PERMANENT);
            buff.SetProperty(PROP_SPEED, 2f);
        }
        public static void SetSource(Buff buff, Entity source)
        {
            buff.SetProperty(PROP_MODE, MesmerizeModes.SOURCE);
            buff.SetProperty(PROP_SOURCE, new EntityID(source));
        }
        public static void CloneMesmerize(Buff buff, Entity target)
        {
            var targetBuff = target.GetFirstBuff<MesmerizeBuff>();
            if (targetBuff == null)
            {
                targetBuff = target.AddBuff<MesmerizeBuff>();
            }
            targetBuff.SetProperty(PROP_MODE, buff.GetProperty<int>(PROP_MODE));
            var oldSource = buff.GetProperty<EntityID>(PROP_SOURCE);
            var newSource = oldSource != null ? new EntityID(oldSource.ID) : null;
            targetBuff.SetProperty(PROP_SOURCE, newSource);
        }

        public static readonly VanillaBuffPropertyMeta PROP_MODE = new VanillaBuffPropertyMeta("Mode");
        public static readonly VanillaBuffPropertyMeta PROP_SPEED = new VanillaBuffPropertyMeta("Speed");
        public static readonly VanillaBuffPropertyMeta PROP_SOURCE = new VanillaBuffPropertyMeta("Source");
    }

    public static class MesmerizeModes
    {
        public const int PERMANENT = 0;
        public const int SOURCE = 1;
        public const int TIMEOUT = 2;
    }
}
