﻿using UnityEngine;

namespace MVZ2Logic.Models
{
    public static class SortingLayers
    {
        public static readonly int background = SortingLayer.NameToID("Background");
        public static readonly int pool = SortingLayer.NameToID("Pool");
        public static readonly int night = SortingLayer.NameToID("Night");
        public static readonly int shadow = SortingLayer.NameToID("Shadow");
        public static readonly int places = SortingLayer.NameToID("Places");
        public static readonly int carriers = SortingLayer.NameToID("Carriers");
        public static readonly int entities = SortingLayer.NameToID("Entities");
        public static readonly int pickups = SortingLayer.NameToID("Pickups");
        public static readonly int foreground = SortingLayer.NameToID("Foreground");
        public static readonly int collectedDrops = SortingLayer.NameToID("CollectedDrops");
        public static readonly int drops = SortingLayer.NameToID("Drops");
        public static readonly int frontUI = SortingLayer.NameToID("FrontUI");
        public static readonly int money = SortingLayer.NameToID("Money");
        public static readonly int screenCover = SortingLayer.NameToID("ScreenCover");
    }
}