using System;
using System.Collections.Generic;
using PVZEngine;
using PVZEngine.Triggers;

namespace MVZ2.Games
{
    public partial class Game : IGameTriggerSystem
    {
        #region ���з���
        public void AddTrigger(ITrigger trigger)
        {
            triggerSystem.AddTrigger(trigger);
        }
        public bool RemoveTrigger(ITrigger trigger)
        {
            return triggerSystem.RemoveTrigger(trigger);
        }
        public void GetTriggers(CallbackReference callbackID, List<ITrigger> results)
        {
            triggerSystem.GetTriggers(callbackID, results);
        }
        #endregion

        #region �����ֶ�
        private TriggerSystem triggerSystem = new TriggerSystem();
        #endregion
    }
}