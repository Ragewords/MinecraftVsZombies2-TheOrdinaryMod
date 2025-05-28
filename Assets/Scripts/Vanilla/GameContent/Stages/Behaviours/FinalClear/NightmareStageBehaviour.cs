using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Buffs.Level;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Pickups;
using MVZ2.GameContent.ProgressBars;
using MVZ2.Vanilla.Audios;
using MVZ2.Vanilla.Entities;
using MVZ2.Vanilla.Level;
using MVZ2Logic.Level;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using PVZEngine.Level;
using System;
using System.Threading;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace MVZ2.GameContent.Stages
{
    public class NightmareStageBehaviour : BossStageBehaviour
    {
        public NightmareStageBehaviour(StageDefinition stageDef) : base(stageDef)
        {
        }
        protected override void AfterFinalWaveUpdate(LevelEngine level)
        {
            base.AfterFinalWaveUpdate(level);
            TheEyeTransitionUpdate(level);
        }
        protected override void BossFightWaveUpdate(LevelEngine level)
        {
            base.BossFightWaveUpdate(level);
            var state = GetBossState(level);
            switch (state)
            {
                case BOSS_STATE_THE_EYE:
                    TheEyeUpdate(level);
                    break;
                case BOSS_STATE_SLENDERMAN_TRANSITION:
                    SlendermanTransitionUpdate(level);
                    break;
                case BOSS_STATE_SLENDERMAN:
                    SlendermanUpdate(level);
                    break;
                case BOSS_STATE_NIGHTMAREAPER_TRANSITION:
                    NightmareaperTransitionUpdate(level);
                    break;
                case BOSS_STATE_NIGHTMAREAPER:
                    NightmareaperUpdate(level);
                    break;
            }
        }
        private void TheEyeTransitionUpdate(LevelEngine level)
        {
            if (level.EntityExists(e => e.Type == EntityTypes.BOSS && e.IsHostileEntity() && !e.IsDead))
            {
                // 眼出现
                level.WaveState = VanillaLevelStates.STATE_BOSS_FIGHT;
                level.PlayMusic(VanillaMusicID.nightmareBoss3);
                level.SetMusicVolume(1);
                level.SetProgressBarToBoss(VanillaProgressBarID.nightmare);
                return;
            }
            // 音乐放缓。
            level.SetMusicVolume(Mathf.Clamp01(level.GetMusicVolume() - (1 / 30f)));
            if (level.GetMusicVolume() <= 0)
            {
                Vector3 pos = new Vector3(level.GetEntityColumnX(4), 800, level.GetEntityLaneZ(2));
                var boss = level.Spawn(VanillaBossID.theEye, pos, null);
            }
        }
        private void TheEyeUpdate(LevelEngine level)
        {
            // 眼战斗
            // 如果不存在Boss，或者所有Boss死亡，进入BOSS后阶段。
            // 如果有Boss存活，不停生成怪物。
            var targetBosses = level.FindEntities(e => e.Type == EntityTypes.BOSS && e.IsHostileEntity() && !e.IsDead);
            if (targetBosses.Length <= 0)
            {
                level.StopMusic();
                SetBossState(level, BOSS_STATE_SLENDERMAN_TRANSITION);
                level.AddBuff<SlendermanTransitionBuff>();
            }
            else
            {
                RunBossWave(level);
            }
        }
        private void SlendermanTransitionUpdate(LevelEngine level)
        {
            ClearEnemies(level);
            if (level.EntityExists(e => e.Type == EntityTypes.BOSS && e.IsHostileEntity() && !e.IsDead))
            {
                // 瘦长鬼影出现
                SetBossState(level, BOSS_STATE_SLENDERMAN);
                level.SetUIAndInputDisabled(false);
                return;
            }
            if (!level.HasBuff<SlendermanTransitionBuff>())
            {
                level.AddBuff<SlendermanTransitionBuff>();
            }
        }
        private void SlendermanUpdate(LevelEngine level)
        {
            // 瘦长鬼影战斗
            // 如果不存在Boss，或者所有Boss死亡，进入BOSS后阶段。
            // 如果有Boss存活，不停生成怪物。
            var targetBosses = level.FindEntities(e => e.Type == EntityTypes.BOSS && e.IsHostileEntity() && !e.IsDead);
            if (targetBosses.Length <= 0)
            {
                SetBossState(level, BOSS_STATE_NIGHTMAREAPER_TRANSITION);
                level.AddBuff<NightmareaperTransitionBuff>();

                // 隐藏UI，关闭输入
                level.ResetHeldItem();
                level.SetUIAndInputDisabled(true);
                level.StopMusic();
            }
            else
            {
                RunBossWave(level);
            }
        }
        private void NightmareaperTransitionUpdate(LevelEngine level)
        {
            ClearEnemies(level);
            if (level.EntityExists(e => e.Type == EntityTypes.BOSS && e.IsHostileEntity() && !e.IsDead))
            {
                // 梦魇收割者出现
                level.SetUIAndInputDisabled(false);
                SetBossState(level, BOSS_STATE_NIGHTMAREAPER);
                return;
            }
            if (!level.HasBuff<NightmareaperTransitionBuff>())
            {
                level.AddBuff<NightmareaperTransitionBuff>();
            }
        }
        private void NightmareaperUpdate(LevelEngine level)
        {
            // 梦魇收割者战斗
            // 如果不存在Boss，或者所有Boss死亡，进入BOSS后阶段。
            // 如果有Boss存活，不停生成怪物。
            if (!level.EntityExists(e => e.Type == EntityTypes.BOSS && e.IsHostileEntity() && !e.IsDead))
            {
                level.WaveState = VanillaLevelStates.STATE_AFTER_BOSS;
                level.StopMusic();
                if (!level.IsRerun)
                {
                    // 隐藏UI，关闭输入
                    level.ResetHeldItem();
                    level.SetUIAndInputDisabled(true);
                }
                else
                {
                    var reaper = level.FindFirstEntity(VanillaBossID.nightmareaper);
                    Vector3 position;
                    if (reaper != null)
                    {
                        position = reaper.Position;
                    }
                    else
                    {
                        var x = level.GetLawnCenterX();
                        var z = level.GetLawnCenterZ();
                        var y = level.GetGroundY(x, z);
                        position = new Vector3(x, y, z);
                    }
                    level.Produce(VanillaPickupID.clearPickup, position, null);
                }
            }
            else
            {
                RunBossWave(level);
            }
        }
        protected override void AfterBossWaveUpdate(LevelEngine level)
        {
            base.AfterBossWaveUpdate(level);
            ClearEnemies(level);
            if (!level.IsRerun)
            {
                if (!level.IsCleared)
                {
                    if (!level.EntityExists(e => e.Type == EntityTypes.BOSS && e.IsHostileEntity()))
                    {
                        if (!level.HasBuff<NightmareClearedBuff>())
                        {
                            level.AddBuff<NightmareClearedBuff>();
                        }
                    }
                }
            }
        }

        public const int BOSS_STATE_THE_EYE = 0;
        public const int BOSS_STATE_SLENDERMAN_TRANSITION = 1;
        public const int BOSS_STATE_SLENDERMAN = 2;
        public const int BOSS_STATE_NIGHTMAREAPER_TRANSITION = 3;
        public const int BOSS_STATE_NIGHTMAREAPER = 4;
    }
}
