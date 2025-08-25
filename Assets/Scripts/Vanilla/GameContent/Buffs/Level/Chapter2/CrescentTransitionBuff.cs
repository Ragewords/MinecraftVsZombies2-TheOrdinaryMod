using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.ProgressBars;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2.Vanilla.Properties;
using MVZ2Logic.Level;
using PVZEngine.Buffs;
using PVZEngine.Entities;
using PVZEngine.Level;
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
            buff.SetProperty(PROP_TIMEOUT, MAX_TIMEOUT);
        }
        public override void PostUpdate(Buff buff)
        {
            base.PostUpdate(buff);

            var timeout = buff.GetProperty<int>(PROP_TIMEOUT);
            timeout--;
            buff.SetProperty(PROP_TIMEOUT, timeout);

            var level = buff.Level;
            if (timeout > CREATE_DARKNESS_TIMEOUT)
            {
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
            }
            if (timeout < CREATE_DARKNESS_TIMEOUT && timeout > SPLASH_TIMEOUT)
            {
                if (level.IsTimeInterval(30))
                {
                    Vector3 pos = new Vector3(level.GetEntityColumnX(4), 0, level.GetEntityLaneZ(2));
                    var splash = level.Spawn(VanillaEffectID.splashParticles, pos, null);
                    splash.SetDisplayScale(Vector3.one * 3);
                    splash.SetTint(level.GetWaterColor());
                    level.PlaySound(VanillaSoundID.splash);
                }
            }
            if (timeout == CREATE_DARKNESS_TIMEOUT)
            {
                level.PlaySound(VanillaSoundID.thump);
                level.ShakeScreen(10, 0, 10);
            }
            else if (timeout <= 0)
            {
                // 音乐。
                level.PlayMusic(VanillaMusicID.junkoThemeArchived);
                level.SetMusicVolume(1);

                Vector3 pos = new Vector3(level.GetEntityColumnX(4), 0, level.GetEntityLaneZ(2));
                var boss = level.Spawn(VanillaBossID.crescent, pos, null);
                boss.Velocity = Vector3.up * 30;
                boss.PlaySound(VanillaSoundID.splashBig);
                boss.Spawn(VanillaEffectID.nightmareaperSplash, pos);
                Crescent.Appear(boss);

                level.SetProgressBarToBoss(VanillaProgressBarID.nightmare);

                buff.Remove();
            }
        }
        public static readonly VanillaBuffPropertyMeta<int> PROP_TIMEOUT = new VanillaBuffPropertyMeta<int>("Timeout");
        public const int MAX_TIMEOUT = CREATE_DARKNESS_TIMEOUT + 60;
        public const int SPLASH_TIMEOUT = FADEOUT_TIMEOUT + 30;
        public const int CREATE_DARKNESS_TIMEOUT = FADEOUT_TIMEOUT + 150;
        public const int FADEOUT_TIMEOUT = 0;
    }
}