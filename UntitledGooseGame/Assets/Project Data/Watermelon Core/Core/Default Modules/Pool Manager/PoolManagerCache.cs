using System;
using System.Collections.Generic;

namespace Watermelon
{
    [Serializable]
    public class PoolManagerCache
    {
        private Dictionary<string, List<PoolCache>> poolsCache = new Dictionary<string, List<PoolCache>>();

        //public List<PoolCache> GetPoolCache(string levelId)
        //{
        //    if (poolsCache.ContainsKey(levelId))
        //    {
        //        return poolsCache[levelId];
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public bool IsEmpty()
        //{
        //    return poolsCache.Count == 0 ? true : false;
        //}

        //public bool ContainsLevel(string levelId)
        //{
        //    return poolsCache.ContainsKey(levelId);
        //}

        //public void UpdateCache(string levelId, List<PoolCache> poolCache)
        //{
        //    poolsCache[levelId] = poolCache;
        //}

        //public void DeleteCache(string levelId)
        //{
        //    if (poolsCache.ContainsKey(levelId))
        //    {
        //        poolsCache.Remove(levelId);
        //    }
        //}
    }
}

// -----------------
// IAP Manager v 1.6.4
// -----------------