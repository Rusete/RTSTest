using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Buildings
{
    public class BuildingsHandler : MonoBehaviour
    {
        public static BuildingsHandler instance;
        [SerializeField] private BuildingData barracks;
        public LayerMask pBuildingLayer, eBuildingLayer;
        public KeyValueDictionary data = new KeyValueDictionary();

        private void Awake()
        {
            instance = this;
        }

        public BuildingStatTypes.Base GetBasicBuildingStats(string type)
        {
            BuildingData building;
            if (data.Contains(type.ToLower()))
            {
                building = data.GetValue(type.ToLower());
                return building.baseStats;
            }
            else
            {
                Debug.Log($"Building Typpe: {type} could not be found or does not exist");
                return null;
            }
            /*
            switch (type.ToLower())
            {
                case "barrack":
                    building = barracks;
                    break;
                case "storage":
                    building = storages;
                    break;
                default:
                    Debug.Log($"Building Typpe: {type} could not be found or does not exist");
                    return null;
            }*/
        }
    }
    [System.Serializable]
    public class KeyValue
    {
        public string key;
        public BuildingData value;
    }

    [System.Serializable]
    public class KeyValueDictionary
    {
        public List<KeyValue> keyValueList = new List<KeyValue>();

        public void Add(string key, BuildingData value)
        {
            KeyValue keyValue = new KeyValue();
            keyValue.key = key;
            keyValue.value = value;
            keyValueList.Add(keyValue);
        }

        public void Remove(string key)
        {
            keyValueList.RemoveAll(kv => kv.key == key);
        }

        public BuildingData GetValue(string key)
        {
            KeyValue keyValue = keyValueList.Find(kv => kv.key == key);
            if (keyValue != null)
                return keyValue.value;
            else
                return null; // O devuelve un valor predeterminado
        }

        public bool Contains(string key)
        {
            return keyValueList.Exists(kv => kv.key == key);
        }

    }

}

