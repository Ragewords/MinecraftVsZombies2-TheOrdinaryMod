﻿using System.Collections.Generic;
using System.Linq;
using PVZEngine.Base;
using PVZEngine.Entities;
using PVZEngine.Level;

namespace PVZEngine.Definitions
{
    public abstract class StageDefinition : Definition
    {
        public StageDefinition(string nsp, string name) : base(nsp, name)
        {
            SetProperty(EngineStageProps.WAVES_PER_FLAG, 10);
        }
        public void Setup(LevelEngine level)
        {
            behaviours.ForEach(b => b.Setup(level));
            OnSetup(level);
        }
        public void Start(LevelEngine level)
        {
            behaviours.ForEach(b => b.Start(level));
            OnStart(level);
        }
        public void Update(LevelEngine level)
        {
            behaviours.ForEach(b => b.Update(level));
            OnUpdate(level);
        }
        public void PrepareForBattle(LevelEngine level)
        {
            behaviours.ForEach(b => b.PrepareForBattle(level));
            OnPrepareForBattle(level);
        }
        public void PostWave(LevelEngine level, int wave)
        {
            behaviours.ForEach(b => b.PostWave(level, wave));
            OnPostWave(level, wave);
        }
        public void PostHugeWaveEvent(LevelEngine level)
        {
            behaviours.ForEach(b => b.PostHugeWaveEvent(level));
            OnPostHugeWave(level);
        }
        public void PostFinalWaveEvent(LevelEngine level)
        {
            behaviours.ForEach(b => b.PostFinalWaveEvent(level));
            OnPostFinalWave(level);
        }
        public void PostEnemySpawned(Entity entity)
        {
            behaviours.ForEach(b => b.PostEnemySpawned(entity));
            OnPostEnemySpawned(entity);
        }
        public virtual void OnSetup(LevelEngine level) { }
        public virtual void OnStart(LevelEngine level) { }
        public virtual void OnUpdate(LevelEngine level) { }
        public virtual void OnPrepareForBattle(LevelEngine level) { }
        public virtual void OnPostWave(LevelEngine level, int wave) { }
        public virtual void OnPostHugeWave(LevelEngine level) { }
        public virtual void OnPostFinalWave(LevelEngine level) { }
        public virtual void OnPostEnemySpawned(Entity entity) { }
        protected void AddBehaviour(StageBehaviour behaviour)
        {
            behaviours.Add(behaviour);
        }
        public bool HasBehaviour<T>() where T : StageBehaviour
        {
            foreach (var behaviour in behaviours)
            {
                if (behaviour is T tBehaviour)
                    return true;
            }
            return false;
        }
        public T GetBehaviour<T>() where T : StageBehaviour
        {
            foreach (var behaviour in behaviours)
            {
                if (behaviour is T tBehaviour)
                    return tBehaviour;
            }
            return null;
        }
        public sealed override string GetDefinitionType() => EngineDefinitionTypes.STAGE;
        private List<StageBehaviour> behaviours = new List<StageBehaviour>();
    }
}
