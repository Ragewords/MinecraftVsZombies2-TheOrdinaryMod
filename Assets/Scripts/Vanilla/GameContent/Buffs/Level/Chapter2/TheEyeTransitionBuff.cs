using MVZ2.GameContent.Effects;
using MVZ2.GameContent.ProgressBars;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Buffs;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Level
{
    [BuffDefinition(VanillaBuffNames.Level.theEyeTransition)]
    public class TheEyeTransitionBuff : BuffDefinition
    {
        public TheEyeTransitionBuff(string nsp, string name) : base(nsp, name)
        {
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMER, new FrameTimer(MAX_TIMEOUT));
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);

            var timer = buff.GetProperty<FrameTimer>(PROP_TIMER);
            timer.Run();
            var level = buff.Level;

            // 让眼睛闭眼。
            foreach (var eye in level.FindEntities(VanillaEffectID.nightmareWatchingEye))
            {
                if (eye.Timeout <= 0)
                {
                    eye.Timeout = 30;
                }
            }
            // 音乐放缓。
            level.SetMusicVolume(Mathf.Clamp01(level.GetMusicVolume() - (1 / 30f)));
            if (timer.PassedFrame(35))
            {
                level.ShakeScreen(30, 0, 10);
                level.PlaySound(VanillaSoundID.explosion);
                Vector3 pos = new Vector3(level.GetEntityColumnX(4), 800, level.GetEntityLaneZ(2));
                level.Spawn(VanillaEffectID.nightmareMeteor, pos, null);
                level.PlaySound(VanillaSoundID.bombFalling);
            }
            if (timer.Expired)
            {
                level.SetMusicVolume(1);
                level.PlayMusic(VanillaMusicID.nightmareBoss3);
                level.SetProgressBarToBoss(VanillaProgressBarID.nightmare);
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("Timer");
        public const int MAX_TIMEOUT = 85;
    }
}
