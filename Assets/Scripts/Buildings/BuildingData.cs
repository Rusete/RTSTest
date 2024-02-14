using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.Buildings
{
    [CreateAssetMenu(fileName = "New Building", menuName = "New Building/Basic")]
    public class BuildingData : ScriptableObject
    {
        public enum EBuildingTypes
        {
            Barracks
        }

        [Space(15)]
        [Header("Building Settings")]

        public EBuildingTypes type;
        public new string name;
        public GameObject buildingPrefab;
        public GameObject icon;
        public float constructionTime;

        [Space(15)]
        [Header("Building Base Stats")]
        [Space(40)]

        public BuildingStatTypes.Base baseStats;  
    }
}

