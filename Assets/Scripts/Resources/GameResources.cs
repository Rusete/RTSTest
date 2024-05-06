using DRC.RTS.Interactables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DRC.RTS.Resources
{
    public class GameResources
    {
        public enum EResourceType
        {
            Wood,
            Rock
        }
    }

    public class GatheringBag<TKey, TValue> : Dictionary<Resources.GameResources.EResourceType, int>
    {
        public void AddOrUpdate(Resources.GameResources.EResourceType key, int value)
        {
            if (ContainsKey(key))
            {
                this[key] += value;
            }
            else
            {
                Add(key, value);
            }
        }

        public int TotalQuantity()
        {
            return Values.Sum();
        }
    }
}