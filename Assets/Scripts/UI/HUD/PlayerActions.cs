using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRC.RTS.UI.HUD
{
    [CreateAssetMenu(fileName ="NewPlayerAction", menuName ="New PlayerAction")]
    public class PlayerActions : ScriptableObject
    {
        [Space(5)]
        [Header("Units")]
        public List<Units.UnitData> basicUnits = new List<Units.UnitData>();

        [Space(5)]
        [Header("Buildings")]
        [Space(15)]
        public List<Buildings.BuildingData> basicBuildings = new List<Buildings.BuildingData>();
    }
}
