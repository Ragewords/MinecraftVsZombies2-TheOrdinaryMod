using System;
using System.Collections.Generic;
using System.Linq;
using PVZEngine.Callbacks;
using PVZEngine.Definitions;
using PVZEngine.Entities;
using Tools;

namespace PVZEngine.Level
{
    public partial class LevelEngine
    {
        #region �ؿ�ʱ��
        public long GetLevelTime()
        {
            return levelTime;
        }
        private void AddLevelTime()
        {
            levelTime++;
        }
        public bool IsTimeInterval(long interval, long offset = 0)
        {
            return levelTime % interval == offset;
        }
        #endregion

        #region �ؿ��¼�
        public void PrepareForBattle()
        {
            StageDefinition.PrepareForBattle(this);
            var param = new LevelCallbackParams(this);
            Triggers.RunCallback(LevelCallbacks.POST_PREPARE_FOR_BATTLE, param);
        }
        public void RunFinalWaveEvent()
        {
            StageDefinition.PostFinalWaveEvent(this);
            var param = new LevelCallbackParams(this);
            Triggers.RunCallback(LevelCallbacks.POST_FINAL_WAVE_EVENT, param);
        }
        public void RunHugeWaveEvent()
        {
            StageDefinition.PostHugeWaveEvent(this);
            var param = new LevelCallbackParams(this);
            Triggers.RunCallback(LevelCallbacks.POST_HUGE_WAVE_EVENT, param);
        }
        #endregion

        #region �����ɵĹ���
        public void AddSpawnedEnemyID(NamespaceID enemyId)
        {
            if (IsEnemySpawned(enemyId))
                return;
            spawnedID.Add(enemyId);
        }
        public bool RemoveSpawnedEnemyID(NamespaceID enemyId)
        {
            return spawnedID.Remove(enemyId);
        }
        public NamespaceID[] GetSpawnedEnemiesID()
        {
            return spawnedID.ToArray();
        }
        public bool IsEnemySpawned(NamespaceID enemyId)
        {
            return spawnedID.Contains(enemyId);
        }
        #endregion

        #region �����
        public int GetRandomEnemySpawnLane()
        {
            var length = GetMaxLaneCount();
            return GetRandomEnemySpawnLane(Enumerable.Range(0, length));
        }
        public int GetRandomEnemySpawnLane(IEnumerable<int> lanes)
        {
            if (lanes.Count() <= 0)
                return -1;
            var possibleLanes = lanes.Where(l => !spawnedLanes.Contains(l));
            if (possibleLanes.Count() <= 0)
            {
                spawnedLanes.Clear();
                possibleLanes = lanes;
            }
            int row = possibleLanes.Random(GetSpawnRNG());
            spawnedLanes.Add(row);
            return row;
        }
        #endregion

        #region ��Ϸ����
        public void Clear()
        {
            IsCleared = true;
            OnClear?.Invoke();
            Triggers.RunCallback(LevelCallbacks.POST_LEVEL_CLEAR, new LevelCallbackParams(this));
        }
        public void GameOver(int type, Entity killer, string message)
        {
            KillerEnemy = killer;
            OnGameOver?.Invoke(type, killer, message);
            var param = new LevelCallbacks.PostGameOverParams()
            {
                level = this,
                type = type,
                killer = killer,
                message = message
            };
            Triggers.RunCallbackFiltered(LevelCallbacks.POST_GAME_OVER, param, type);
        }
        #endregion

        #region ���л�
        private void WriteProgressToSerializable(SerializableLevel seri)
        {
            seri.isCleared = IsCleared;
            seri.levelTime = levelTime;
            seri.spawnedLanes = spawnedLanes;
            seri.spawnedID = spawnedID;
        }
        private void ReadProgressFromSerializable(SerializableLevel seri)
        {
            levelTime = seri.levelTime;
            IsCleared = seri.isCleared;
            spawnedLanes = seri.spawnedLanes;
            spawnedID = seri.spawnedID;
        }
        #endregion

        public event Action<int, Entity, string> OnGameOver;
        public event Action OnClear;
        public int CurrentWave { get; set; }
        public int CurrentFlag { get; set; }
        public int WaveState { get; set; }
        public bool LevelProgressVisible { get; set; }
        public bool IsCleared { get; private set; }
        public Entity KillerEnemy { get; private set; }
        private string deathMessage;
        private long levelTime = 0;
        private List<int> spawnedLanes = new List<int>();
        private List<NamespaceID> spawnedID = new List<NamespaceID>();
    }
}