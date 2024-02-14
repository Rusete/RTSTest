using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Buildings
{
    public class BuildingsHandler : MonoBehaviour
    {
        public static BuildingsHandler instance;
        [SerializeField]
        private BuildingData barracks;
        public LayerMask pBuildingLayer, eBuildingLayer;

        private void Awake()
        {
            instance = this;
        }

        public BuildingStatTypes.Base GetBasicBuildingStats(string type)
        {
            BuildingData building;
            switch (type)
            {
                case "barrack":
                    building = barracks;
                    break;
                default:
                    Debug.Log($"Building Typpe: {type} could not be found or does not exist");
                    return null;
            }
            return building.baseStats;
        }
    }
}

