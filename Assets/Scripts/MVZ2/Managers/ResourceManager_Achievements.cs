﻿using System.Collections.Generic;
using System.Linq;
using MVZ2.Metas;
using PVZEngine;
using UnityEngine;

namespace MVZ2.Managers
{
    public partial class ResourceManager : MonoBehaviour
    {
        public AchievementMetaList GetAchievementMetaList(string spaceName)
        {
            var modResource = GetModResource(spaceName);
            if (modResource == null)
                return null;
            return modResource.AchievementMetaList;
        }
        public AchievementMeta[] GetModAchievementMetas(string spaceName)
        {
            var stageMetalist = GetAchievementMetaList(spaceName);
            if (stageMetalist == null)
                return null;
            return stageMetalist.metas;
        }
        public NamespaceID[] GetAllAchievements()
        {
            return achievementCacheDict.Keys.ToArray();
        }
        public AchievementMeta GetAchievementMeta(NamespaceID entityID)
        {
            return achievementCacheDict.TryGetValue(entityID, out var meta) ? meta : null;
        }
        private Dictionary<NamespaceID, AchievementMeta> achievementCacheDict = new Dictionary<NamespaceID, AchievementMeta>();
    }
}
