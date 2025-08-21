using MVZ2.GameContent.Effects;
using MVZ2.GameContent.ProgressBars;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
using Tools;
using UnityEngine;

namespace MVZ2.GameContent.Buffs.Level
{
    [BuffDefinition(VanillaBuffNames.Level.crescentTransition)]
    public class CrescentTransitionBuff : BuffDefinition
    {
        public CrescentTransitionBuff(string nsp, string name) : base(nsp, name)
        {
        }
        public override void PostAdd(Buff buff)
        {
            base.PostAdd(buff);
            buff.SetProperty(PROP_TIMER, new FrameTimer(MAX_TIMEOUT));
            buff.SetProperty(PROP_MUSIC_FADE, true);
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
            if (buff.GetProperty<bool>(PROP_MUSIC_FADE))
                level.SetMusicVolume(Mathf.Clamp01(level.GetMusicVolume() - (1 / 30f)));

            if (timer.PassedFrame(60))
            {
                level.ShakeScreen(30, 0, 10);
                level.PlaySound(VanillaSoundID.explosion);
                level.SetMusicVolume(1);
                level.PlayMusic(VanillaMusicID.nightmareBoss3);
                buff.SetProperty(PROP_MUSIC_FADE, false);
            }
            if (timer.Frame <= 60 && timer.Frame > 30)
            {
                if (timer.PassedInterval(3))
                {
                    Vector3 pos = new Vector3(level.GetEntityColumnX(4), 0, level.GetEntityLaneZ(2));
                    var splash = level.Spawn(VanillaEffectID.splashParticles, pos, null);
                    splash.SetTint(level.GetWaterColor());
                    splash.SetDisplayScale(Vector3.one * 4);
                }
                level.ShakeScreen(5, 0, 10);
            }
            if (timer.PassedFrame(30))
            {
                level.PlaySound(VanillaSoundID.splashBig);
                Vector3 pos = new Vector3(level.GetEntityColumnX(4), 0, level.GetEntityLaneZ(2));
                var splash = level.Spawn(VanillaEffectID.splashParticles, pos, null);
                splash.SetTint(level.GetWaterColor());
                splash.SetDisplayScale(Vector3.one * 4);
                level.Spawn(VanillaEffectID.nightmareaperSplash, pos, null);
                var meteor = level.Spawn(VanillaEffectID.nightmareMeteor, pos, null);
                meteor.Velocity = Vector3.up * 20;
            }
            if (timer.Expired)
            {
                level.SetProgressBarToBoss(VanillaProgressBarID.nightmare);
                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<FrameTimer> PROP_TIMER = new VanillaBuffPropertyMeta<FrameTimer>("Timer");
        public static readonly VanillaBuffPropertyMeta<bool> PROP_MUSIC_FADE = new VanillaBuffPropertyMeta<bool>("MusicFade");
        public const int MAX_TIMEOUT = 120;
    }
}
