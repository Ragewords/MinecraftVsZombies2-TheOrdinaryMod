﻿using MVZ2.HeldItems;
using MVZ2Logic.HeldItems;
using PVZEngine.Level;
using UnityEngine.EventSystems;

namespace MVZ2.Level
{
    public interface ILevelRaycastReceiver
    {
        bool IsValidReceiver(LevelEngine level, HeldItemDefinition definition, IHeldItemData data, PointerEventData eventData);
        int GetSortingLayer();
        int GetSortingOrder();
    }
}
