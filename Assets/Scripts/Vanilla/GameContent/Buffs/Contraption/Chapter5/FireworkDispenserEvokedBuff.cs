using MVZ2.GameContent.Projectiles;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using PVZEngine.Modifiers;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Contraptions
{
    [BuffDefinition(VanillaBuffNames.Contraption.fireworkDispenserEvoked)]
    public class FireworkDispenserEvokedBuff : BuffDefinition
    {
        public FireworkDispenserEvokedBuff(string nsp, string name) : base(nsp, name)
        {
            AddModifier(new FloatModifier(VanillaEntityProps.RANGE, NumberOperator.Add, 80));
            AddModifier(new NamespaceIDModifier(VanillaEntityProps.PROJECTILE_ID, VanillaProjectileID.fireworkBig));
            AddModifier(new Vector3Modifier(EngineEntityProps.DISPLAY_SCALE, NumberOperator.Multiply, PROP_SCALE));
            AddModifier(new Vector3Modifier(VanillaEntityProps.SHADOW_SCALE, NumberOperator.Multiply, PROP_SCALE));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);
            var entity = buff.GetEntity();
            if (entity != null)
            {
                var timer = buff.GetProperty<FrameTimer>(PROP_TIMER);
                if (timer != null)
                {
                    timer.Run();
                    buff.SetProperty(PROP_SCALE, new Vector3(1.333f, 1, 1));
                    entity.Level.AddLoopSoundEntity(VanillaSoundID.songFoDeniseSample, entity.ID);
                }
                if (timer == null || timer.Expired)
                {
                    buff.SetProperty(PROP_SCALE, Vector3.one);
                    entity.Level.RemoveLoopSoundEntity(VanillaSoundID.songFoDeniseSample, entity.ID);
                }
            }
        }
        public const int TIMEOUT = 150;
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("timer");
        public static readonly VanillaBuffPropertyMeta<Vector3> PROP_SCALE = new VanillaBuffPropertyMeta<Vector3>("scale", Vector3.one);
    }
}
