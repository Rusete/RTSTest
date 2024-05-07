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
            Rock,
            Gold
        }
    }

    public class GatheringBag<TKey, TValue> : Dictionary<Resources.GameResources.EResourceType, int>
    {
        public event Action<Resources.GameResources.EResourceType, int, string> OnResourceUpdated;


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
            OnResourceUpdated?.Invoke(key, this[key], value.ToString());
        }

        public int TotalQuantity()
        {
            return Values.Sum();
        }
    }
}