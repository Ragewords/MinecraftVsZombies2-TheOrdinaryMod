using MVZ2.GameContent.Bosses;
using MVZ2.GameContent.Buffs.Level;
using MVZ2.GameContent.Effects;
using MVZ2.GameContent.Pickups;
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
            SlendermanTransitionUpdate(level);
        }
        protected override void BossFightWaveUpdate(LevelEngine level)
        {
            base.BossFightWaveUpdate(level);
            var state = GetBossState(level);
            switch (state)
            {
                case BOSS_STATE_SLENDERMAN:
                    SlendermanUpdate(level);
                    break;
                case BOSS_STATE_NIGHTMAREAPER_TRANSITION:
                    NightmareaperTransitionUpdate(level);
                    break;
                case BOSS_STATE_NIGHTMAREAPER:
                    NightmareaperUpdate(level);
                    break;
                case BOSS_STATE_THE_EYE_TRANSITION:
                    TheEyeTransitionUpdate(level);
                    break;
                case BOSS_STATE_THE_EYE:
                    TheEyeUpdate(level);
                    break;
            }
        }
        private void SlendermanTransitionUpdate(LevelEngine level)
        {
            if (level.EntityExists(e => e.Type == EntityTypes.BOSS && e.IsHostileEntity() && !e.IsDead))
            {
                // 瘦长鬼影出现
                level.WaveState = VanillaLevelStates.STATE_BOSS_FIGHT;
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
                        var x = (level.GetGridLeftX() + level.GetGridRightX()) * 0.5f;
                        var z = (level.GetGridTopZ() + level.GetGridBottomZ()) * 0.5f;
                        var y = level.GetGroundY(x, z);
                        position = new Vector3(x, y, z);
                    }
                    level.Produce(VanillaPickupID.clearPickup, position, null);
                }
            }
            else
            {
                RunBossWave(level);
                // 碾压墙被强制合拢时生成眼。
                foreach (var wall in level.FindEntities(VanillaEffectID.crushingWalls))
                {
                    if (CrushingWalls.GetProgress(wall) >= 1 && CrushingWalls.GetFirstClosed(wall))
                    {
                        CrushingWalls.Enrage(wall);
                        var reaper = level.FindFirstEntity(VanillaBossID.nightmareaper);
                        reaper.Remove();
                        Vector3 pos = new Vector3(level.GetEntityColumnX(4), 0, level.GetEntityLaneZ(2));
                        var boss = level.Spawn(VanillaBossID.theEye, pos, null);
                        boss.PlaySound(VanillaSoundID.splashBig);
                        boss.Spawn(VanillaEffectID.nightmareaperSplash, pos);
                        level.ShakeScreen(30, 0, 30);
                        ClearEnemies(level);
                        SetBossState(level, BOSS_STATE_THE_EYE_TRANSITION);
                    }
                }
            }
        }
        private void TheEyeTransitionUpdate(LevelEngine level)
        {
            ClearEnemies(level);
            // 恢复场上所有器械的朝向。
            foreach (var entity in level.FindEntities(e => (e.Type == EntityTypes.PLANT) && !e.IsDead && e.IsFriendlyEntity()))
            {
                if (entity.GetScale().x < 0)
                {
                    entity.SetScale(new Vector3(-entity.GetScale().x, entity.GetScale().y, entity.GetScale().z));
                    entity.SetDisplayScale(new Vector3(-entity.GetDisplayScale().x, entity.GetDisplayScale().y, entity.GetDisplayScale().z));
                }
            }
            if (level.EntityExists(e => e.Type == EntityTypes.BOSS && e.IsHostileEntity() && !e.IsDead))
            {
                // 眼出现
                level.SetUIAndInputDisabled(false);
                SetBossState(level, BOSS_STATE_THE_EYE);
                level.PlayMusic(VanillaMusicID.nightmareBoss3);
                foreach (var wall in level.FindEntities(VanillaEffectID.crushingWalls))
                {
                    CrushingWalls.SetFirstClosed(wall, false);
                }
                foreach (var timer in level.FindEntities(VanillaEffectID.nightmareaperTimer))
                {
                    NightmareaperTimer.SetTimeout(timer, 6300);
                }
                return;
            }
        }
        private void TheEyeUpdate(LevelEngine level)
        {
            // 眼战斗
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
                    var reaper = level.FindFirstEntity(VanillaBossID.theEye);
                    Vector3 position;
                    if (reaper != null)
                    {
                        position = reaper.Position;
                    }
                    else
                    {
                        var x = (level.GetGridLeftX() + level.GetGridRightX()) * 0.5f;
                        var z = (level.GetGridTopZ() + level.GetGridBottomZ()) * 0.5f;
                        var y = level.GetGroundY(x, z);
                        position = new Vector3(x, y, z);
                    }
                    level.Produce(VanillaPickupID.clearPickup, position, null);
                }
            }
            else
            {
                RunBossWave(level);
                foreach (var wall in level.FindEntities(VanillaEffectID.crushingWalls))
                {
                    if (CrushingWalls.GetProgress(wall) <= 0.01)
                        wall.State = VanillaEntityStates.CRUSHING_WALLS_IDLE;
                }
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

        public const int BOSS_STATE_SLENDERMAN = 0;
        public const int BOSS_STATE_NIGHTMAREAPER_TRANSITION = 1;
        public const int BOSS_STATE_NIGHTMAREAPER = 2;
        public const int BOSS_STATE_THE_EYE_TRANSITION = 3;
        public const int BOSS_STATE_THE_EYE = 4;
    }
}
